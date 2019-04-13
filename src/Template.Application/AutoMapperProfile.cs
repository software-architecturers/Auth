using AutoMapper;
using Template.Application.Cqrs.Items.Commands;
using Template.Application.Cqrs.Items.Queries;
using Template.Domain.Entities;

namespace Template.Application
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Item, ItemDto>();
            CreateMap<CreateItem, Item>(MemberList.Source);
        }
    }
}