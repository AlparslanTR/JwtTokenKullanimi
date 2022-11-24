using CoreLayer.Dtos;
using CoreLayer.GenericServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JwtTokenService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : CustomBaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken(LoginDto loginDto)
        {
            var createToken= await _authenticationService.CreateToken(loginDto);
            // Uzun kod yazmak yerine CustomBaseController oluşturup kısaca tek satırda halledebiliriz.
            //if (createToken.StatusCode==200)
            //{
            //    return Ok(createToken);
            //}
            //else
            //{
            //    return NotFound();
            //}
            return ActionResultInstance(createToken);
        }

        [HttpPost]
        public IActionResult CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var createTokenClient= _authenticationService.CreateTokenByClient(clientLoginDto);
            return ActionResultInstance(createTokenClient);
        }

        [HttpPost]
        public async Task <IActionResult> RevokeRefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var revokeTokenClient = await _authenticationService.RevokeRefreshToken(refreshTokenDto.RefreshToken);
            return ActionResultInstance(revokeTokenClient);
        }

        [HttpPost]
        public async Task<IActionResult>CreateTokenByRefreshToken(RefreshTokenDto refreshTokenDto) 
        {
            var createRefreshToken=await _authenticationService.RevokeRefreshToken(refreshTokenDto.RefreshToken);
            return ActionResultInstance(createRefreshToken);    
        }

    }
}
