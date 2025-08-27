using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deno.Models
{
    public class LoginResponse
    {
        internal string message;
        internal int status;

        public string Message { get; set; }
        public string Username { get; set; }
        public string Auth { get; set; }
        public int Id { get; set; }
        public int Status { get; set; }
        public string StatusMessage { get; set; } 
    }
    public class CashierLoginResponse
    {
        public int Id { get; set; }
        public string Auth { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }

        public int Status { get; set; }
    }

}
