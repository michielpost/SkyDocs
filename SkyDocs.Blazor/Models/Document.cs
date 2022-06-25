using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Models
{
    public class DocumentSummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }
        public string? PreviewImage { get; set; }

        public StorageSource StorageSource { get; set; }


        /// <summary>
        /// Public key to read file from storage location
        /// </summary>
        public byte[] PublicKey { get; set; } = default!;

        /// <summary>
        /// Private key to write file to storage
        /// </summary>
        public byte[]? PrivateKey { get; set; }

        /// <summary>
        /// Content of the file is encrypted with a key from this seed
        /// </summary>
        public string ContentSeed { get; set; } = default!;

        public string? ShareOrigin { get; set; }
    }

    public class Document
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Content { get; set; } = string.Empty;
        public string? PreviewImage { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public DateTimeOffset ModifiedDate { get; set; }


        [JsonIgnore]
        public int Revision { get; set; }

        public Document()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTimeOffset.UtcNow;
            ModifiedDate = DateTimeOffset.UtcNow;
        }

    }

    public enum StorageSource
    {
        Skynet,
        Dfinity
    }
}
