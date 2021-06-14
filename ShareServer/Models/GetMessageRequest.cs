using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareServer.Models
{
    public class GetMessageRequest
    {
        public string Address { get; set; } = default!;
        public string Skylink { get; set; } = default!;
        public string SecretHash { get; set; } = default!;
    }
}
