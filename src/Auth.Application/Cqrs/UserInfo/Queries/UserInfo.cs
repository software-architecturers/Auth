using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Auth.Domain.Entities;
using Auth.Persistence;
using AutoMapper;
using IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Cqrs.UserInfo.Queries
{
    public class UserInfo : IRequest<UserInfoDto>
    {
        public UserInfo(string userId)
        {
            UserId = userId;
        }

        public string UserId { get; }

        public class Handler : IRequestHandler<UserInfo, UserInfoDto>
        {
            private readonly IMapper _mapper;
            private readonly ApplicationDbContext _context;
            private readonly UserManager<ApplicationUser> _userManager;

            public Handler(IMapper mapper, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
            {
                _mapper = mapper;
                _context = context;
                _userManager = userManager;
            }

            public Task<UserInfoDto> Handle(UserInfo request, CancellationToken cancellationToken)
            {
                var dto = _mapper.Map<UserInfoDto>(_context.Users.Find(request.UserId));
                dto.PictureUrl = _context.UserClaims
                    .FirstOrDefault(claim => claim.UserId == dto.Id &&
                                             claim.ClaimType == JwtClaimTypes.Picture)
                    ?.ClaimValue;
                return Task.FromResult(dto);
            }
        }
    }
}