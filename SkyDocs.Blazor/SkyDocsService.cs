using SiaSkynet;
using SkyDocs.Blazor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SkyDocs.Blazor
{
    /// <summary>
    /// ViewModel Service for Blazor Index.razor
    /// </summary>
    public class SkyDocsService
    {
        private readonly string salt = "skydocs";
        private readonly RegistryKey listDataKey = new RegistryKey("skydocs-list");
        private SiaSkynetClient client = new SiaSkynetClient();
        private byte[]? privateKey;
        private byte[]? publicKey;

        public bool IsLoggedIn { get; set; }
        public bool IsLoading { get; set; }
        public List<DocumentSummary> DocumentList { get; set; } = new List<DocumentSummary>();
        public Document? CurrentDocument { get; set; }
        public string? Error { get; set; }

        public void SetPortalDomain(string baseUrl)
        {
            client = new SiaSkynetClient(baseUrl);
        }

        /// <summary>
        /// Login with username/password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void Login(string username, string password)
        {
            string seedPhrase = $"{username}-{password}-{salt}";
            var key = SiaSkynetClient.GenerateKeys(seedPhrase);
            privateKey = key.privateKey;
            publicKey = key.publicKey;

            IsLoggedIn = true;
        }

        /// <summary>
        /// Load list with all documents
        /// </summary>
        /// <returns></returns>
        public async Task LoadDocumentList()
        {
            IsLoading = true;
            DocumentList = await GetDocumentList();
            IsLoading = false;
        }

        /// <summary>
        /// Load document based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task LoadDocument(Guid id)
        {
            IsLoading = true;
            CurrentDocument = await GetDocument(id);
            IsLoading = false;
        }

        /// <summary>
        /// Save current document
        /// </summary>
        /// <param name="fallbackTitle"></param>
        /// <returns></returns>
        public async Task SaveCurrentDocument(string fallbackTitle, byte[] img)
        {
            Error = null;
            if (CurrentDocument != null)
            {
                var existing = DocumentList.Where(x => x.Id == CurrentDocument.Id).FirstOrDefault();
                if (existing != null)
                {
                    DocumentList.Remove(existing);
                }

                var created = existing?.CreatedDate ?? DateTimeOffset.UtcNow;

                //Fix title if there is no title
                var title = CurrentDocument.Title;
                if (string.IsNullOrWhiteSpace(title))
                    title = fallbackTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = "Untitled document " + created;

                CurrentDocument.Title = title;

                //Save document
                bool success = await SaveDocument(CurrentDocument);

                if (success)
                {
                    Console.WriteLine("Document saved");

                    DocumentSummary sum = new DocumentSummary()
                    {
                        Id = CurrentDocument.Id,
                        Title = CurrentDocument.Title,
                        CreatedDate = created,
                        ModifiedDate = DateTimeOffset.UtcNow
                    };
                    DocumentList.Add(sum);

                    string? imgLink = null;
                    if (img != null)
                    {
                        using (Stream stream = new MemoryStream(img))
                        {
                            //Save preview image to Skynet file
                            var response = await client.UploadFileAsync("document.jpg", stream);

                            imgLink = response.Skylink;
                            Console.WriteLine("Image saved");

                        }
                    }
                    sum.PreviewImage = imgLink;

                    //Save updated document list
                    await SaveDocumentList(DocumentList);
                    Console.WriteLine("Document list saved");
                }

                if (!success)
                    Error = "Error saving document. Please try again";
                else
                    CurrentDocument = null;
            }

        }

        public async Task DeleteCurrentDocument()
        {
            Error = null;
            if (CurrentDocument != null)
            {
                var existing = DocumentList.Where(x => x.Id == CurrentDocument.Id).FirstOrDefault();
                if (existing != null)
                {
                    DocumentList.Remove(existing);
                }

                //Save updated document list
                bool success = await SaveDocumentList(DocumentList);
                if (success)
                    CurrentDocument = null;
            }
        }

        /// <summary>
        /// Initialize a new document
        /// </summary>
        public void StartNewDocument()
        {
            CurrentDocument = new Document();
        }

        /// <summary>
        /// Get list with all documents
        /// </summary>
        /// <returns></returns>
        private async Task<List<DocumentSummary>> GetDocumentList()
        {
            try
            {
                Error = null;
                var encryptedJson = await client.SkyDbGet(publicKey, listDataKey, TimeSpan.FromSeconds(5));
                if (!encryptedJson.HasValue)
                    return new List<DocumentSummary>();
                else
                {
                    //Decrypt data
                    var jsonBytes = Utils.Decrypt(encryptedJson.Value.file, privateKey);
                    var json = Encoding.UTF8.GetString(jsonBytes);

                    var loadedList = JsonSerializer.Deserialize<List<DocumentSummary>>(json) ?? new List<DocumentSummary>();
                    return loadedList.Where(x => x != null).ToList();
                }
            }
            catch
            {
                Error = "Unable to get list of documents from Skynet. Please try again.";
            }
            return new List<DocumentSummary>();
        }

        /// <summary>
        /// Save list with all documents
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private async Task<bool> SaveDocumentList(List<DocumentSummary> list)
        {
            var json = JsonSerializer.Serialize(list);
            bool success = false;
            try
            {
                var data = Encoding.UTF8.GetBytes(json);
                var encryptedData = Utils.Encrypt(data, privateKey);
                success = await client.SkyDbSet(privateKey, publicKey, listDataKey, encryptedData);
            }
            catch
            {
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Get document from SkyDB based on ID as key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<Document?> GetDocument(Guid id)
        {
            try
            {
                Error = null;
                var json = await client.SkyDbGetAsString(publicKey, new RegistryKey(id.ToString()), TimeSpan.FromSeconds(10));
                if (string.IsNullOrEmpty(json))
                    return new Document();
                else
                    return JsonSerializer.Deserialize<Document>(json) ?? new Document();
            }
            catch
            {
                Error = "Unable to load document from Skynet. Please try again.";
            }

            return null;
        }

        /// <summary>
        /// Save document to SkyDB
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private async Task<bool> SaveDocument(Document doc)
        {
            var json = JsonSerializer.Serialize(doc);
            bool success = false;
            try
            {
                success = await client.SkyDbSet(privateKey, publicKey, new RegistryKey(doc.Id.ToString()), json);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
            }
            return success;
        }

    }
}
