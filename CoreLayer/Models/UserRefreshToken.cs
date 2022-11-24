using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Models
{
    public class UserRefreshToken
    {
        public string UserId { get; set; } // Kullanıcının Tokeni
        public string RefreshToken { get; set; } // Kullanıcının Tokeninin Yenilenmesi
        public DateTime Expiration { get; set; } // Kullanıcının Tokeninin Süresi
    }
}
