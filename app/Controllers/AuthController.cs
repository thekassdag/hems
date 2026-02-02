using Microsoft.AspNetCore.Mvc;
using HMS.Services;
using System.Threading.Tasks;
using HMS.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using HMS.Models;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims; // Added this
using Microsoft.AspNetCore.Authentication; // Added this

namespace HMS.Controllers
{
    public class AuthController : Controller
    {
        private readonly EmailService _email;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(EmailService email, ApplicationDbContext context, ILogger<AuthController> logger)
        {
            _email = email;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View(); // Create a simple AccessDenied.cshtml view
        }

        public class OtpRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            public string? IpAddress { get; set; } // Added IpAddress
        }

        [HttpPost("auth/request-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> RequestOtp([FromBody] OtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid email address." });
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return NotFound(new { message = "User with this email does not exist." });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new { message = "Your account is blocked. Please contact support." });
                }

                // Generate 6-digit OTP
                var otp = new Random().Next(100000, 999999).ToString();
                var now = DateTime.UtcNow;

                var loginSession = new LoginSession
                {
                    UserId = user.Id,
                    OtpCode = otp,
                    DeviceInfo = Request.Headers["User-Agent"].ToString(),
                    IpAddress = request.IpAddress, // Use IP from request body
                    IsLoggedIn = false,
                };

                _context.LoginSessions.Add(loginSession);
                await _context.SaveChangesAsync();

                var emailBody = $"<p>Your HMS login code is: <strong>{otp}</strong></p><p>This code will expire in 5 minutes.</p>";
                await _email.Send(user.Email, "Your HMS Login Code", emailBody);

                return Ok(new { message = "A 6-digit code has been sent to your email." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while requesting an OTP for {Email}", request.Email);
                return StatusCode(500, new { message = "Something went wrong on our end. Please try again later." });
            }
        }

        public class VerifyOtpRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            [Required]
            [StringLength(6, MinimumLength = 6)]
            public string OtpCode { get; set; }
        }

        [HttpPost("auth/verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid email or OTP format." });
            }

            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                // Find the latest unexpired OTP for this user
                var loginSession = await _context.LoginSessions
                    .Where(ls => ls.UserId == user.Id && ls.OtpCode == request.OtpCode && ls.IsLoggedIn == false)
                    .OrderByDescending(ls => ls.CreatedAt)
                    .FirstOrDefaultAsync();

                if (loginSession == null)
                {
                    return BadRequest(new { message = "Invalid OTP code." });
                }

                var now = DateTime.UtcNow;
                // if (loginSession.OtpExpireAt < now) // Removed as OtpExpireAt is removed from LoginSession
                // {
                //     return BadRequest(new { message = "OTP code has expired." });
                // }

                // OTP is valid, mark as logged in
                loginSession.IsLoggedIn = true;
                await _context.SaveChangesAsync();

                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString()) // Assuming UserRole enum
                };

                var claimsIdentity = new ClaimsIdentity(claims, "AccessTokenCookie");
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                await HttpContext.SignInAsync("AccessTokenCookie", claimsPrincipal);

                return Ok(new { message = "Login successful! Congratulations!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while verifying OTP for {Email}", request.Email);
                return StatusCode(500, new { message = "Something went wrong on our end. Please try again later." });
            }
        }

        public async Task<IActionResult> Test()
        {
            await _email.Send(
                "thekassdag@gmail.com",
                "Test Email",
                "<p>Hello from .NET!</p>"
            );

            return Ok("sent");
        }

        [HttpGet("auth/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("AccessTokenCookie");
            return RedirectToAction("Index", "Auth");
        }
    }
}
