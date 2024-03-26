using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApiLib.DataStore.Entity
{
    public class MessageModel
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string? SenderEmail { get; set; }
        public Guid RecipientId { get; set; }
        public string? RecipientEmail { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Text { get; set; }
    }
}
