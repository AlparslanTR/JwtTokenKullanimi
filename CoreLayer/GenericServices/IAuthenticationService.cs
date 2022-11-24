using CoreLayer.Dtos;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.GenericServices
{
    public interface IAuthenticationService
    {
        Task<Response<TokenDto>> CreateToken(LoginDto loginDto); // Giriş Yapan Kullanıcı İçin Üretilen Token.
        Task<Response<TokenDto>> CreateRefreshToken(string refreshToken); // Süresi Biten Token İçin Yeni Token Üretme.
        Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken); // Sürekli İstek Atılan Token Olursa NULL Set edilir.
        Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto); // Üyelik Sistemi Olmayanlar İçin Token Oluşturma.
    }
}
