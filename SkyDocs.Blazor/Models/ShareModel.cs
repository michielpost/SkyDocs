using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkyDocs.Blazor.Models
{
    public class ShareModel
    {
        public string? Message { get; set; }
        public DocumentSummary Sum { get; set; } = default!;

    }
}
