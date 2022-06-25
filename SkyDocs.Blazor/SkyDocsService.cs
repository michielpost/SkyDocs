using Dfinity.Blazor;
using SiaSkynet;
using SkyDocs.Blazor.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        private readonly string salt = "skydocs-2";
        private readonly RegistryKey listDataKey = new RegistryKey("skydocs-list");
        private readonly DfinityService dfinityService;
        private readonly IHttpClientFactory httpClientFactory;
        private SiaSkynetClient client = new SiaSkynetClient();
        private byte[]? privateKey;
        private byte[]? publicKey;

        public bool IsLoggedIn { get; set; }
        public bool IsMetaMaskLogin { get; set; }
        public bool IsDfinityLogin { get; set; }
        public bool IsDfinityNetwork { get; set; }

        public bool IsLoading { get; set; }
        public DocumentList DocumentList { get; set; } = new DocumentList();
        public Document? CurrentDocument { get; set; }
        public DocumentSummary? CurrentSum => DocumentList.Where(x => x.Id == CurrentDocument?.Id).FirstOrDefault();
        public static string? Error { get; set; }
        public List<TheGraphShare> Shares { get; set; } = new List<TheGraphShare>();
        public string CurrentNetwork => IsDfinityNetwork ? "Internet Computer" : "Skynet";

        public SkyDocsService(DfinityService dfinityService, IHttpClientFactory httpClientFactory)
        {
            this.dfinityService = dfinityService;
            this.httpClientFactory = httpClientFactory;

            var httpClient = httpClientFactory.CreateClient("API");
            client = new SiaSkynetClient(client: httpClient);
        }

        public List<TheGraphShare> NewShares()
        {
            var existing = DocumentList.Select(x => x.ShareOrigin);
            return Shares.Where(x => x.Receiver == null).Where(x => !existing.Contains(x.Id)).OrderByDescending(x => x.BlockNumber).ToList();
        }

        public List<TheGraphShare> ExistingShares()
        {
            var existing = DocumentList.Select(x => x.ShareOrigin);
            return Shares.Where(x => existing.Contains(x.Id) || x.Sender == null).OrderByDescending(x => x.BlockNumber).ToList();
        }

        public void SetPortalDomain(string scheme, string domain)
        {
            //Do not use Internet Computer as domain for Sia Skynet calls
            if (domain.Contains("ic0.app", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            string[] urlParts = domain.Split('.');

            //Only take last two parts
            var lastParts = urlParts.Skip(urlParts.Count() - 2).Take(2);

            if (lastParts.Count() == 2)
            {
                var url = $"{scheme}://{string.Join('.', lastParts)}";
                Console.WriteLine($"Using API domain: {url}");

                var httpClient = httpClientFactory.CreateClient("API");
                client = new SiaSkynetClient(url, client: httpClient);
            }
        }

        /// <summary>
        /// Login with username/password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void Login(string username, string password, bool isMetaMaskLogin = false)
        {
            string seedPhrase = $"{username}-{password}-{salt}";
            var key = SiaSkynetClient.GenerateKeys(seedPhrase);
            privateKey = key.privateKey;
            publicKey = key.publicKey;

            IsLoggedIn = true;
            IsMetaMaskLogin = isMetaMaskLogin;
        }

        public void LoginDfinity()
        {
            privateKey = null;
            publicKey = null;

            IsLoggedIn = true;
            IsDfinityLogin = true;
            IsDfinityNetwork = true;
        }

        /// <summary>
        /// Load list with all documents
        /// </summary>
        /// <returns></returns>
        public async Task LoadDocumentList()
        {
            IsLoading = true;
            var loadedDocuments = await GetDocumentList();

            //Remove existing items;
            DocumentList.RemoveAll(x => loadedDocuments.Select(l => l.Id).Contains(x.Id));

            DocumentList.AddRange(loadedDocuments);
            DocumentList.Revision = loadedDocuments.Revision;

            IsLoading = false;
        }

        public void AddDocumentSummary(Guid docId, byte[] pubKey, byte[]? privKey, string contentSeed, StorageSource storageSource)
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
                Title = "Shared document",
                StorageSource = storageSource
            };
            DocumentList.Add(sum);
        }

        internal (int total, int newShares) SetShares(List<TheGraphShare> shares)
        {
            Shares = shares;

            return (Shares.Count, this.NewShares().Count);
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
                var sum = CurrentSum;
                if (sum != null && sum.PrivateKey == null)
                {
                    DocumentList.Remove(sum);
                    sum = null;
                    CurrentDocument.Id = Guid.NewGuid();
                }

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
                        PublicKey = key.publicKey,
                        StorageSource = this.IsDfinityLogin ? StorageSource.Dfinity : StorageSource.Skynet
                    };

                    DocumentList.Add(sum);
                }

                if(sum.PrivateKey == null)
                {
                }


                //Fix title if there is no title
                var title = CurrentDocument.Title;
                if (string.IsNullOrWhiteSpace(title))
                    title = fallbackTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = "Untitled document " + sum.CreatedDate;

                CurrentDocument.Title = title;
                sum.Title = title;

                CurrentDocument.ModifiedDate = DateTimeOffset.UtcNow;
                sum.ModifiedDate = DateTimeOffset.UtcNow;

                //Save image
                string? imgLink = null;
                try
                {
                    imgLink = await SaveDocumentImage(img);
                    sum.PreviewImage = imgLink;
                    CurrentDocument.PreviewImage = imgLink;
                }
                catch { }

                //Save document
                bool success = await SaveDocument(CurrentDocument, sum);

                if (success)
                {
                    Console.WriteLine("Document saved");

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

        private async Task<string?> SaveDocumentImage(byte[] img)
        {
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

            return imgLink;
        }

        public async Task DeleteCurrentDocument()
        {
            Error = null;
            if (CurrentDocument != null)
            {
                var existing = CurrentSum;
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
        private async Task<DocumentList> GetDocumentList()
        {
            try
            {
                Error = null;

                if (IsDfinityNetwork)
                {
                    string? json = await dfinityService.GetValueForUser(listDataKey.Key);
                    if (string.IsNullOrEmpty(json))
                        return new();

                    var loadedList = JsonSerializer.Deserialize<List<DocumentSummary>>(json) ?? new List<DocumentSummary>();
                    var list = new DocumentList(loadedList.Where(x => x != null).ToList());
                    return list;

                }
                else if(publicKey != null)
                {
                    var encryptedJson = await client.SkyDbGet(publicKey, listDataKey, TimeSpan.FromSeconds(5));
                    if (!encryptedJson.HasValue)
                        return new DocumentList();
                    else if(privateKey != null)
                    {
                        //Decrypt data
                        var jsonBytes = Utils.Decrypt(encryptedJson.Value.file, privateKey);
                        var json = Encoding.UTF8.GetString(jsonBytes);

                        var loadedList = JsonSerializer.Deserialize<List<DocumentSummary>>(json) ?? new List<DocumentSummary>();
                        var list = new DocumentList(loadedList.Where(x => x != null).ToList());
                        list.Revision = encryptedJson.Value.registryEntry?.Revision ?? 0;
                        return list;
                    }
                }
            }
            catch
            {
                Error = $"Unable to get list of documents from {CurrentNetwork}. Please try again.";
            }
            return new DocumentList();
        }

        /// <summary>
        /// Save list with all documents
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<bool> SaveDocumentList(DocumentList list)
        {
            var json = JsonSerializer.Serialize(list);
            bool success = false;
            try
            {
                if (IsDfinityNetwork)
                {
                    await dfinityService.SetValueForUser(listDataKey.Key, json);
                    success = true;

                }
                else if(publicKey != null && privateKey != null)
                {
                    var data = Encoding.UTF8.GetBytes(json);
                    var encryptedData = Utils.Encrypt(data, privateKey);
                    list.Revision++;

                    success = await client.SkyDbSet(privateKey, publicKey, listDataKey, encryptedData, list.Revision);
                }
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
        private async Task<Document?> GetDocument(DocumentSummary sum)
        {
            try
            {
                Error = null;
                Console.WriteLine("Loading document");
                if (IsDfinityNetwork || sum.StorageSource == StorageSource.Dfinity)
                {
                    string? json = await dfinityService.GetValue(sum.Id.ToString());
                    if (string.IsNullOrEmpty(json))
                        return new Document();

                    var document = JsonSerializer.Deserialize<Document>(json) ?? new Document();
                    sum.Title = document.Title;
                    sum.PreviewImage = document.PreviewImage;
                    sum.CreatedDate = document.CreatedDate;
                    sum.ModifiedDate = document.ModifiedDate;

                    return document;
                }
                else
                {
                    var encryptedData = await client.SkyDbGet(sum.PublicKey, new RegistryKey(sum.Id.ToString()), TimeSpan.FromSeconds(10));
                    if (!encryptedData.HasValue)
                    {
                        return new Document();
                    }
                    else
                    {
                        //Decrypt data
                        var key = SiaSkynetClient.GenerateKeys(sum.ContentSeed);
                        var jsonBytes = Utils.Decrypt(encryptedData.Value.file, key.privateKey);
                        var json = Encoding.UTF8.GetString(jsonBytes);

                        var document = JsonSerializer.Deserialize<Document>(json) ?? new Document();
                        sum.Title = document.Title;
                        sum.PreviewImage = document.PreviewImage;
                        sum.CreatedDate = document.CreatedDate;
                        sum.ModifiedDate = document.ModifiedDate;

                        document.Revision = encryptedData.Value.registryEntry?.Revision ?? 0;

                        return document;
                    }
                }
            }
            catch
            {
                Error = $"Unable to load document from {CurrentNetwork}. Please try again.";
            }

            return null;
        }

        /// <summary>
        /// Save document to SkyDB
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private async Task<bool> SaveDocument(Document doc, DocumentSummary sum)
        {
            //Only allowed to save if you have a private key for this document
            if (sum.PrivateKey == null)
                return false;

            var json = JsonSerializer.Serialize(doc);
            bool success = false;
            try
            {
                if (IsDfinityNetwork || sum.StorageSource == StorageSource.Dfinity)
                {
                    await dfinityService.SetValue(doc.Id.ToString(), json);
                    success = true;
                }
                else
                {
                    //Encrypt with ContentSeed
                    var key = SiaSkynetClient.GenerateKeys(sum.ContentSeed);
                    var data = Encoding.UTF8.GetBytes(json);
                    var encryptedData = Utils.Encrypt(data, key.privateKey);

                    success = await client.SkyDbSet(sum.PrivateKey, sum.PublicKey, new RegistryKey(doc.Id.ToString()), encryptedData, doc.Revision + 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
            }
            return success;
        }

    }
}
