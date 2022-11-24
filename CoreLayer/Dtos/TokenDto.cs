using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Dtos
{
    public class TokenDto
    {
        public string AccessToken { get; set; } // Oluşturulan Token
        public DateTime AccessTokenExpiration { get; set; } // Oluşturulan Tokenin Ömrü
        public string RefreshToken { get; set; } // Yenilenen Token
        public DateTime RefreshTokenExpiration { get; set; } // Yenilenen Tokenin Ömrü
    }
}
