using Auth.Domain.Entities;
using AutoMapper;

namespace Auth.Application.Cqrs.UserInfo
{
    public class UserInfoMapper : AutoMapper.Profile
    {
        public UserInfoMapper()
        {
            CreateMap<ApplicationUser, UserInfoDto>(MemberList.None)
                .ForMember(dto => dto.Id, opt => opt.MapFrom(user => user.Id))
                .ForMember(dto => dto.UserName, opt => opt.MapFrom(user => user.UserName));
        }
    }
}