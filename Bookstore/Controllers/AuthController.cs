using Bookstore.data;
using Bookstore.model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AuthController(AppDbContext context) => _context = context;

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(new { message = "Registered successfully" });
        }

        [HttpPost("login")]
        public IActionResult Login(User creds)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == creds.Email && u.Password == creds.Password);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            return Ok(new { token = "dummy-token" }); // Implement JWT later
        }
    }
}
