using AutoMapper.Internal.Mappers;
using CoreLayer.Dtos;
using CoreLayer.GenericServices;
using CoreLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Mapper;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.GenericServices
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserApp> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<UserApp> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new UserApp { Email= createUserDto.Email,UserName=createUserDto.UserName };
            var reseult=await _userManager.CreateAsync(user,createUserDto.Password);
            if (!reseult.Succeeded)
            {
                var errors = reseult.Errors.Select(x => x.Description).ToList();
                return Response<UserAppDto>.Fail(new ErrorDto(errors, true), 400);
            }
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user),200);
        }
        // Admin ve User Rolüne Sahip Olur
        public async Task<Response<NoDataDto>> CreateAdminRoles(string userMail)
        {
            await _roleManager.CreateAsync(new() { Name = "Admin" });
            await _roleManager.CreateAsync(new() { Name = "User" });
            var user = await _userManager.FindByEmailAsync(userMail);
            await _userManager.AddToRoleAsync(user, "Admin");
            await _userManager.AddToRoleAsync(user, "User");
            return Response<NoDataDto>.Success(StatusCodes.Status201Created);
        }
        // User Rolüne Sahip Olur
        public async Task<Response<NoDataDto>> CreateUserRoles(string userMail)
        {
            await _roleManager.CreateAsync(new() { Name = "User" });
            var user = await _userManager.FindByEmailAsync(userMail);
            await _userManager.AddToRoleAsync(user, "User");
            return Response<NoDataDto>.Success(StatusCodes.Status201Created);
        }
        public async Task<Response<UserAppDto>> GetUserByName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return Response<UserAppDto>.Fail("Kullanıcı Bulunamadı", 404, true);
            }
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user),200);
        }

       
    }
}
