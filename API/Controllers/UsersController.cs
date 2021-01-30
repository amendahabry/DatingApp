using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class UsersController : BaseApiController
    {
        private readonly DataContext _context;
        public UsersController(DataContext context)
        {
            _context = context;
        }

        // all users
        [HttpGet]
        [AllowAnonymous]
        // async is to make the code available to more users
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers(){
            
            return await _context.Users.ToListAsync();

        }

        // single user by id
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUser(int id){
            
            return await _context.Users.FindAsync(id);
        }
    }
}