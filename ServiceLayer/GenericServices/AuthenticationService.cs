using CoreLayer.Configuration;
using CoreLayer.Dtos;
using CoreLayer.GenericServices;
using CoreLayer.Models;
using CoreLayer.Repositories;
using CoreLayer.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.GenericServices
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefreshTokenRepository;

        public AuthenticationService(IOptions<List<Client>> clients, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshTokenRepository)
        {
            _clients = clients.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefreshTokenRepository = userRefreshTokenRepository;
        }

        public async Task<Response<TokenDto>> CreateRefreshToken(string refreshToken)
        {
            var existrefreshToken=await _userRefreshTokenRepository.Where(x=>x.RefreshToken==refreshToken).SingleOrDefaultAsync();
            if (existrefreshToken==null)
            {
                return Response<TokenDto>.Fail("Refresh Token Bulunamadı", 404, true);
            }
            var user = await _userManager.FindByIdAsync(existrefreshToken.UserId);
            if (user==null)
            {
                return Response<TokenDto>.Fail("User Bulunamadı", 404, true);
            }
            var tokenDto=_tokenService.CreateToken(user);
            existrefreshToken.RefreshToken = tokenDto.RefreshToken;
            existrefreshToken.Expiration = tokenDto.RefreshTokenExpiration;
            await _unitOfWork.SaveChangesAsync();
            return Response<TokenDto>.Success(tokenDto, 200);
        }

        public async Task<Response<TokenDto>> CreateToken(LoginDto loginDto)
        {
            if (loginDto==null) // Eğer hiçbir değer girilmezse null at.
            {
                throw new ArgumentNullException(nameof(loginDto));
            }
            var user = await _userManager.FindByEmailAsync(loginDto.Email); // Sisteme Giriş yapan maili bul.
            
            if (user==null)
            {
                return Response<TokenDto>.Fail("Mail Adresiniz Veya Şifreniş Yanlış", 404,true); // Mail Hatalı girerse mesaj döndür.
            }
            
            if (!await _userManager.CheckPasswordAsync(user,loginDto.Password)) // Şifre Kontrolü.
            {
                return Response<TokenDto>.Fail("Mail Adresiniz Veya Şifreniş Yanlış", 404, true);
            }

            var token = _tokenService.CreateToken(user);
            var userRefreshToken = await _userRefreshTokenRepository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();

            if (userRefreshToken==null)
            {
                await _userRefreshTokenRepository.AddAsync(new UserRefreshToken 
                {
                    UserId=user.Id,RefreshToken=token.RefreshToken,Expiration=token.RefreshTokenExpiration
                });
            }
            else
            {
                userRefreshToken.RefreshToken = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }
            await _unitOfWork.SaveChangesAsync();
            return Response<TokenDto>.Success(token, 200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = _clients.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);
            if (client==null)
            {
                return Response<ClientTokenDto>.Fail("Client Id veya Secret Bulunamadı",404,true);
            }
            var token = _tokenService.CreateTokenByClient(client);
            return Response<ClientTokenDto>.Success(token, 200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _userRefreshTokenRepository.Where(x => x.RefreshToken == refreshToken).SingleOrDefaultAsync();
            if (existRefreshToken==null)
            {
                return Response<NoDataDto>.Fail("Refresh Token Bulunamadı", 404, true);
            }
            _userRefreshTokenRepository.Remove(existRefreshToken);
            await _unitOfWork.SaveChangesAsync();
            return Response<NoDataDto>.Success(200);
        }
    }
}
