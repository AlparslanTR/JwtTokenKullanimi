using CoreLayer.Dtos;
using CoreLayer.GenericServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.GenericServices;
using SharedLibrary.Exceptions;

namespace JwtTokenService.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : CustomBaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateUserDto createUserDto)
        {
            //throw new CustomException("Databasede bir hata meydana geldi");
            return ActionResultInstance(await _userService.CreateUserAsync(createUserDto));
        }
        [Authorize(Roles ="Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUser() 
        {
            return ActionResultInstance(await _userService.GetUserByName(HttpContext.User.Identity.Name));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult>CreateAdminRols(string userMail)
        {
            return ActionResultInstance(await _userService.CreateAdminRoles(userMail));
        }

        [Authorize(Roles = "Admin",Policy ="KütahyaPolicy")]
        [HttpPost]
        public async Task<IActionResult>CreateUserRols(string userMail)
        {
            return ActionResultInstance(await _userService.CreateUserRoles(userMail));
        }
    }
}
