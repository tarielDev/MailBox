using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;
using UserApi.Services;
using WebApiLib;
using WebApiLib.Abstraction;
using WebApiLib.DataStore.Entity;
using WebApiLib.Rsa;




namespace UserApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly Account _account;
        private readonly IConfiguration _configuration;

        public UserController(IUserService userService, Account account, IConfiguration configuration)
        {
            _userService = userService;
            _account = account;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public  IActionResult Login([Description("User Auth")][FromBody] LoginModel model)
        {
            
            if(_account.GetToken() is not null)
                return BadRequest("Пользователь уже залогинен");

            var responce = _userService.Authentification(model);

            if (!responce.IsSuccess)
                return NotFound();

            _account.Login(responce.Users[0]);
            _account.RefreshToken(GenerateToken(_account));
            return Ok(_account.GetToken());
        }

        [AllowAnonymous]
        [HttpPost("addUser")]
        public ActionResult AddUser([FromBody] LoginModel model)
        {

            if (ValidMail(model.Name) == false)
                return BadRequest($"Email:{model.Name} - должен быть Email");

            var responce = _userService.UserAdd(model);
            if (!responce.IsSuccess)
                return BadRequest($"Error: {responce.Errors.FirstOrDefault().Message}");

            return Ok(responce.UserId);
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet("healthCheck")]
        public ActionResult<Guid> HealthCheck()
        {
            var token = HttpContext.GetTokenAsync("access_token").Result;

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return BadRequest("Invalid token");
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Id" || c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return BadRequest("User ID not found in token");
            }

            return Ok(userIdClaim.Value);

        }

        [AllowAnonymous]
        [HttpPost("addAdmin")]
        public ActionResult AddAdmin([FromBody] LoginModel model)
        {
            if (ValidMail(model.Name) == false)
                return BadRequest($"Error: {model.Name} - должен быть Email");
            var userId = _userService.AdminAdd(model);

            if (userId.Equals(default))
            {
                return BadRequest("Пользователь существует");
            }
            return Ok(userId);
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost("usersList")]
        public IActionResult GetUsers()
        {
            var usersList = _userService.GetUsers();

            if(usersList.Count == 0)
            {
                return BadRequest("Пользователь не найден");
            }
            return Ok(usersList);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/(userToDeleteName)")]
        public IActionResult DeleteUser(string userToDeleteName)
        {
            bool isDeleted = _userService.Delete(userToDeleteName);

            if (!isDeleted)
            {
                return BadRequest("Пользователь не найден, не может быть удален или недостаточно привелегий");

            }
            return Ok("Пользователь удален");
        }


        [HttpPost("logout")]
        public ActionResult LogOut()
        {
            _account.Logout();
            return Ok();
        }

        private string GenerateToken(Account account)
        {
            var key = new RsaSecurityKey(RsaService.GetPrivateKey());
            var credentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature);
            var claim = new[]
            {
                new Claim(ClaimTypes.Email, account.UserName.ToString()),
                new Claim(ClaimTypes.Role, account.Role.ToString()!),
                new Claim("Id", account.Id.ToString()!)
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claim,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    //    private string GetUserEmailFromToken()
    //    {
    //        var token = HttpContext.GetTokenAsync("access_token").Result;

    //        var handler = new JwtSecurityTokenHandler();
    //        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
    //        var claim = jwtToken!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

    //        if (claim == null)
    //        {
    //            throw new InvalidOperationException("Email claim not found in token.");
    //        }
    //;
    //        return claim.Value;

    //    }

        private static bool ValidMail(string name)
        {
            try
            {
                MailAddress mailAddress = new (name);
                return true;
            }
            catch(FormatException) 
            {
                return false;
            }
        }

    }
}
