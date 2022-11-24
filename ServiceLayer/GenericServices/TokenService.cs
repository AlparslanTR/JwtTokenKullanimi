using CoreLayer.Configuration;
using CoreLayer.Dtos;
using CoreLayer.GenericServices;
using CoreLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace ServiceLayer.GenericServices
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<UserApp> _userManager; // Identityden Çağırıyoruz.
        private readonly CustomTokenOptions _customTokenOptions; // Tanımlanan CustomTokeni Çağırıyoruz.

        public TokenService(UserManager<UserApp> userManager, IOptions<CustomTokenOptions> options) // IOptions Interface Kullanıyoruz.
        {
            _userManager = userManager;
            _customTokenOptions = options.Value; // Eşitlemek için Value almalıyız.
        }

        private string CreateRefreshToken() // Sadece Burada kullanmak için Özel tanımlıyoruz. 
        {
            var numberByte= new byte[32]; // 32Byte Random String Değer Üret.
            using var random = RandomNumberGenerator.Create(); // Random Bir Değer Üret.
            random.GetBytes(numberByte);// Üretilen random değerlerinin Byte'nı al ve ilk değere gönder.
            return Convert.ToBase64String(numberByte); // Number Byte'yi Stringe Çevir.
        }
        private async Task <IEnumerable<Claim>>GetClaim(UserApp userApp, List<string> audiences) // Giriş Çıkışlı API'ler için.
        {
            var userRoles =await _userManager.GetRolesAsync(userApp);
            var userList = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userApp.Id), // Kullanıcı ID Tutar.
                new Claim(JwtRegisteredClaimNames.Email, userApp.Email), // Kullanıcı Maili Jwt Token Mailin Datasına İşler.
                new Claim(ClaimTypes.Name, userApp.UserName), // Kullanıcı Adı Jwt Token Ad Datasına İşler.
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()), // Json Kimliklendirici.
                new Claim("City",userApp.City)
            };

            userList.AddRange(audiences.Select(x=>new Claim(JwtRegisteredClaimNames.Aud,x))); /* Birden fazla list tutulduğu için
            AddRange kullanıyoruz. Sonraki işlem ise JWT Datasındaki veriyi kontrol ediyor.*/
            userList.AddRange(userRoles.Select(x => new Claim(ClaimTypes.Role, x))); // Ne kadar rol varsa o kadar claim nesnesi oluştur.
            return userList;
        }
        private IEnumerable<Claim> GetClaimsByClient(Client client) // Üyelik Sistemi Gerektirmeyen API'ler İçin.
        {
            var claims = new List<Claim>();
            claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x))); // Dataya Namesleri Getir.
            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()); // JWT ye Guid Olarak Yazdır.
            new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString()); // Id'yi Guid olarak Stringe çevir.
            return claims;
        }
        public TokenDto CreateToken(UserApp userApp) // Kullanıcı İçin Token Üret.
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(_customTokenOptions.AccessTokenExpiration); // Üretilen Tokenin Tarihini Al.
            var refreshTokenExpiration = DateTime.Now.AddMinutes(_customTokenOptions.RefreshTokenExpiration); // Yenilenen Tokenin Tarihini Al.
            var securityKey = SignService.GetSymmetricSecurityKey(_customTokenOptions.SecurityKey); // Gizli Anahtarı Al.
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);// Tür Seç.
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken
            (
                issuer: _customTokenOptions.Issuer, //Custom Tokenden gelen Issueri yazdır.
                expires: accessTokenExpiration, // Tokenin oluşma tarihini yazdır.
                notBefore: DateTime.Now, 
                claims: GetClaim(userApp, _customTokenOptions.Audience).Result, // Datayı Claimse Al.
                signingCredentials: signingCredentials // İmzayı Al.
            );
            var handler = new JwtSecurityTokenHandler(); 
            var token=handler.WriteToken(jwtSecurityToken); // Değişkende oluşan Tokeni Yazdır.
            var tokenDto = new TokenDto
            {
                AccessToken = token,
                RefreshToken = CreateRefreshToken(),
                AccessTokenExpiration = accessTokenExpiration,
                RefreshTokenExpiration = refreshTokenExpiration
            };
            return tokenDto;
        }

        public ClientTokenDto CreateTokenByClient(Client client) // Üyelik Barındırmayanlar İçin.
        {
            var accessTokenExpiration = DateTime.Now.AddMinutes(_customTokenOptions.AccessTokenExpiration); // Üretilen Tokenin Tarihini Al.
            var securityKey = SignService.GetSymmetricSecurityKey(_customTokenOptions.SecurityKey); // Gizli Anahtarı Al.
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);// Tür Seç.
            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken
            (
                issuer: _customTokenOptions.Issuer, //Custom Tokenden gelen Issueri yazdır.
                expires: accessTokenExpiration, // Tokenin oluşma tarihini yazdır.
                notBefore: DateTime.Now, 
                claims: GetClaimsByClient(client), // Datayı Claimse Al.
                signingCredentials: signingCredentials // İmzayı Al.
            );
            var handler = new JwtSecurityTokenHandler(); 
            var token=handler.WriteToken(jwtSecurityToken); // Değişkende oluşan Tokeni Yazdır.
            var clientDto = new ClientTokenDto
            {
                AccessToken = token,
                AccessTokenExpiration = accessTokenExpiration,
            };
            return clientDto;
        }
    }
}
