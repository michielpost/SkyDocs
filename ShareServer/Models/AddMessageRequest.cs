using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareServer.Models
{
    public class AddMessageRequest
    {
        public string Address { get; set; } = default!;
        public string Message { get; set; } = default!;
    }
}
