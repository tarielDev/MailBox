using AutoMapper;
using Azure;
using Microsoft.EntityFrameworkCore;
using WebApiLib;
using WebApiLib.Abstraction;
using WebApiLib.DataStore.Entity;
using WebApiLib.Responce;

namespace MessagesApi.Services
{
    public class MessageService : IMessageService
    {
        public readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MessageService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MessageResponce> GetNewMessages(string recipientEmail)
        {
            var responce = new MessageResponce
            {
                Messages = new List<MessageModel>(),
                IsSuccess = false
            };

            using (var context = _context)
            {
                var messages = await context.Messages
                    .Include(m=> m.Recipient)
                    .Include(m => m.Sender)
                    .Where(m=> m.Recipient.UserName == recipientEmail && !m.IsRead).ToListAsync();

                foreach (var message in messages)
                {
                    message.IsRead = true;
                }
                context.UpdateRange(messages);
                await context.SaveChangesAsync();

                responce.Messages.AddRange(messages.Select(_mapper.Map<MessageModel>));
                responce.IsSuccess = true;
            }
            return responce;
        }

        public async Task<MessageResponce> SendMessage(MessageModel model)
        {
            var senderGuid = _context.Users
                             .Where(x => x.UserName == model.SenderEmail)
                             .Select(x => x.Id)
                             .SingleOrDefault();

            var recipientGuid = _context.Users
                                .Where(x => x.UserName == model.RecipientEmail)
                                .Select(x => x.Id)
                                .SingleOrDefault();

            if (senderGuid == default || recipientGuid == default)
            {
                var res = new MessageResponce();
                res.IsSuccess = false;

                return res;

            }

            model.SenderId = senderGuid;
            model.RecipientId = recipientGuid;

            var response = new MessageResponce
            {
                Messages = new List<MessageModel>(),
                IsSuccess = false
            };

            using (var context = _context)
            {
                var message = _mapper.Map<MessageEntity>(model);
                context.Messages.Add(message);
                await context.SaveChangesAsync();

                response.Messages.Add(model);
                response.IsSuccess = true;
            }
            return response;
        }
    }
}
