using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WebApiLib.Abstraction;
using WebApiLib.DataStore.Entity;

namespace MessagesApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMessageService _messageService;

        public MessageController(IMapper mapper, IMessageService messageService)
        {
            _mapper = mapper;
            _messageService = messageService;
        }

        [Authorize(Roles = "Admin, User")]
        [HttpGet("get")]
        public async Task<ActionResult> GetNewMessage()
        {
            var senderEmail = GetUserEmailFromToken().GetAwaiter().GetResult();

            var response = await _messageService.GetNewMessages(senderEmail);
            if (!response.IsSuccess)
                return BadRequest(response.Errors!.FirstOrDefault()!.Message);

            return Ok(response.Messages);
        }

        [Authorize(Roles = "Admin, User")]
        [HttpPost("send")]
        public async Task<ActionResult> SendMessage(string recipientEmail, string text)
        {
            var senderEmail = await GetUserEmailFromToken();

            var message = new MessageModel
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.Now,
                RecipientEmail = recipientEmail,
                SenderEmail = senderEmail,
                Text = text
            };

            var response = await _messageService.SendMessage(message);
            if (!response.IsSuccess)
                return BadRequest(response.Errors!.FirstOrDefault()!.Message);

            return Ok(response.Messages);
        }

        private async Task<string> GetUserEmailFromToken()
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
            var claim = jwtToken!.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);

            if (claim == null)
            {
                throw new InvalidOperationException("Email claim not found in token.");
            }
            ;
            return claim.Value;

        }
    }
}

