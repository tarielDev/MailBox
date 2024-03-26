using AutoMapper;
using WebApiLib.DataStore.Entity;

namespace MessagesApi.Mapper
{
    public partial class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<MessageEntity, MessageModel>()
                .ForMember(dest => dest.SenderEmail, opt => opt.MapFrom(src => src.Sender!.UserName))
                .ForMember(dest => dest.RecipientEmail, opt => opt.MapFrom(src => src.Recipient!.UserName));

            CreateMap<MessageModel, MessageEntity>()
                .ForMember(dest => dest.Sender, opt => opt.Ignore()) 
                .ForMember(dest => dest.Recipient, opt => opt.Ignore());
        }
    }

}
