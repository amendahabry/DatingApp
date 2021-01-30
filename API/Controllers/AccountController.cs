using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            // if the user already exists
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            // making hash to the password
            using var hmac = new HMACSHA512();

            // creating a new user
            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                // hashing the password
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            // saving the user to the collection
            _context.Users.Add(user);
            // calling the database to save the user
            await _context.SaveChangesAsync();

            return new UserDto 
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // check if this username is the same username in the database
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            //check if the username is valid not null
            if (user == null) return Unauthorized("Invalid Username");

            // reverse the hash code of the password
            using var hmac = new HMACSHA512(user.PasswordSalt);

            //compute the hash of the password
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            // iterate on the hash and check if its values the same as the login password
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return new UserDto 
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };

        }

        private async Task<bool> UserExists(string username)
        {
            // return boolean if exists
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

    }
}