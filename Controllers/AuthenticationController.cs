    using Capstone_2_BE.DTOs;
using Capstone_2_BE.DTOs.Authentication;
using Capstone_2_BE.Services;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_2_BE.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : Controller
    {
        public readonly AuthenticationService _authenticationService;
        public readonly ILogger<AuthenticationController> _logger;
        
        public AuthenticationController(AuthenticationService authenticationService, ILogger<AuthenticationController> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await _authenticationService.Login(loginDTO);
            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode, new { message = result.Error });
            }
            return StatusCode(result.StatusCode, result.Data);
        }
    }
}