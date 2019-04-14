using AutoMapper;
using Template.Domain.Entities;

namespace Template.Application.Cqrs.Items
{
    public class ItemsMapperProfile : Profile
    {
        public ItemsMapperProfile()
        {
            CreateMap<Item, ItemDto>().ReverseMap();
            CreateMap<ItemInputModel, Item>(MemberList.Source);
        }
    }
}