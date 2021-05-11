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
        private static SiaSkynetClient client = new SiaSkynetClient();
        private byte[]? privateKey;
        private byte[]? publicKey;

        public bool IsLoggedIn { get; set; }
        public bool IsLoading { get; set; }
        public List<DocumentSummary> DocumentList { get; set; } = new List<DocumentSummary>();
        public Document? CurrentDocument { get; set; }
        public static string? Error { get; set; }

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

        public void AddDocumentSummary(Guid docId, byte[] pubKey, byte[]? privKey, string contentSeed)
        {
            var existing = DocumentList.Where(x => x.Id == docId).FirstOrDefault();
            if (existing != null)
                return;

            DocumentSummary sum = new DocumentSummary()
            {
                Id = docId,
                PublicKey = pubKey,
                PrivateKey = privKey,
                ContentSeed = contentSeed,
                CreatedDate = DateTimeOffset.UtcNow,
                ModifiedDate = DateTimeOffset.UtcNow,
                Title = "TODO: Shared document",
            };
            DocumentList.Add(sum);
        }

        /// <summary>
        /// Load document based on id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task LoadDocument(Guid id)
        {
            IsLoading = true;
            var sum = DocumentList.Where(x => x.Id == id).FirstOrDefault();
            if(sum != null)
                CurrentDocument = await GetDocument(sum);
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
                var sum = DocumentList.Where(x => x.Id == CurrentDocument.Id).FirstOrDefault();
                if (sum == null)
                {
                    string contentSeed = Guid.NewGuid().ToString();
                    string fileSeed = Guid.NewGuid().ToString();
                    string seedPhrase = $"{fileSeed}-{salt}";
                    var key = SiaSkynetClient.GenerateKeys(seedPhrase);

                    sum = new DocumentSummary()
                    {
                        Id = CurrentDocument.Id,
                        Title = CurrentDocument.Title,
                        CreatedDate = DateTimeOffset.UtcNow,
                        ModifiedDate = DateTimeOffset.UtcNow,
                        ContentSeed = contentSeed,
                        PrivateKey = key.privateKey,
                        PublicKey = key.publicKey
                    };

                    DocumentList.Add(sum);
                }


                //Fix title if there is no title
                var title = CurrentDocument.Title;
                if (string.IsNullOrWhiteSpace(title))
                    title = fallbackTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = "Untitled document " + sum.CreatedDate;

                CurrentDocument.Title = title;
                sum.Title = title;
                sum.ModifiedDate = DateTimeOffset.UtcNow;

                //Save document
                bool success = await SaveDocument(CurrentDocument, sum);

                if (success)
                {
                    Console.WriteLine("Document saved");

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
        private async static Task<Document?> GetDocument(DocumentSummary sum)
        {
            try
            {
                Error = null;
                var encryptedData = await client.SkyDbGet(sum.PublicKey, new RegistryKey(sum.Id.ToString()), TimeSpan.FromSeconds(10));
                if (!encryptedData.HasValue)
                    return new Document();
                else
                {
                    //Decrypt data
                    var key = SiaSkynetClient.GenerateKeys(sum.ContentSeed);
                    var jsonBytes = Utils.Decrypt(encryptedData.Value.file, key.privateKey);
                    var json = Encoding.UTF8.GetString(jsonBytes);

                    return JsonSerializer.Deserialize<Document>(json) ?? new Document();
                }
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
        private async static Task<bool> SaveDocument(Document doc, DocumentSummary sum)
        {
            //Only allowed to save if you have a private key for this document
            if (sum.PrivateKey == null)
                return false;

            var json = JsonSerializer.Serialize(doc);
            bool success = false;
            try
            {
                //Encrypt with ContentSeed
                var key = SiaSkynetClient.GenerateKeys(sum.ContentSeed);
                var data = Encoding.UTF8.GetBytes(json);
                var encryptedData = Utils.Encrypt(data, key.privateKey);

                success = await client.SkyDbSet(sum.PrivateKey, sum.PublicKey, new RegistryKey(doc.Id.ToString()), encryptedData);
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
