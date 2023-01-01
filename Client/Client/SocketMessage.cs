using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiClient
{
    class SocketMessage
    {
        public string Flag { get; set; }
        public string Message { get; set; }

    }
    class SignedSocketMessage
    {
        public string Data { get; set; }
        public string Signature { get; set; }

    }
}
