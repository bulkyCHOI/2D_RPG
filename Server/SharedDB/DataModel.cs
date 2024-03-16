using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedDB
{
    [Table("Token")]
    public class TokenDb
    {
        public int TokenDbId { get; set; }
        public int AccountDbId { get; set; }
        public int Token { get; set; }
        public DateTime Expired { get; set; }
    }

    [Table("ServerInfo")]
    public class ServerInfoDb
    {
        public int ServerInfoDbId { get; set; }
        public string ServerName { get; set; }
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public int BusyCcore { get; set; }
    }
}
