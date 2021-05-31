using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Models
{
    public class DocumentList : List<DocumentSummary>
    {
        [JsonIgnore]
        public int Revision { get; set; }

        public DocumentList()
        {

        }

        public DocumentList(IEnumerable<DocumentSummary> collection) : base(collection)
        {

        }
    }
}
