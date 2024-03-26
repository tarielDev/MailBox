using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiLib.DataStore.Entity;
using WebApiLib.Responce;

namespace WebApiLib.Abstraction
{
    public interface IMessageService
    {
        Task<MessageResponce> GetNewMessages(string recipientEmail);
        Task<MessageResponce> SendMessage(MessageModel model);
    }
}
