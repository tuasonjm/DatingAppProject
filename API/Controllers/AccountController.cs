using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {
            if( await UserExists(dto.userName)) 
            {
                return BadRequest("UserName is taken.");
            }

            using HMACSHA512 hmac = new HMACSHA512();

            AppUser user = new AppUser
            {
                UserName = dto.userName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.password)),
                PassworldSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

             UserDto userDto = new UserDto
            {
                userName = dto.userName,
                token = _tokenService.CreateToken(user)
            };

            return  userDto;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> LogIn(LogInDto dto)
        {
            AppUser user = await _context.Users.SingleOrDefaultAsync(users => users.UserName == dto.userName);

            if(user == null)
            {
                return Unauthorized("Invalid username.");
            }

            using HMACSHA512 hmac = new HMACSHA512(user.PassworldSalt);

            var incomingHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(dto.password));

            for (int index = 0; user.PasswordHash.Length > index; index++)
            {
                if(incomingHash[index] != user.PasswordHash[index]){return Unauthorized("Wrong password.");}
            }

             UserDto userDto = new UserDto
            {
                userName = dto.userName,
                token = _tokenService.CreateToken(user)
            };

            return userDto;
        }

        private async Task<bool> UserExists(string userName)
        {
            return await _context.Users.AnyAsync(user => user.UserName == userName.ToLower());
        }
    }
}