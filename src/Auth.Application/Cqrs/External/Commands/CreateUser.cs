using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Auth.Domain.Entities;
using IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Cqrs.External.Commands
{
    public class CreateUser : IRequest<ApplicationUser>
    {
        public CreateUser(ExternalUserDto externalUserDto)
        {
            ExternalUserDto = externalUserDto;
        }

        public ExternalUserDto ExternalUserDto { get; }

        public class Handler : IRequestHandler<CreateUser, ApplicationUser>
        {
            private readonly UserManager<ApplicationUser> _userManager;

            public Handler(UserManager<ApplicationUser> userManager)
            {
                _userManager = userManager;
            }

            public async Task<ApplicationUser> Handle(CreateUser request, CancellationToken cancellationToken)
            {
                var externalUserInfo = request.ExternalUserDto;
                var claims = externalUserInfo.ExternalClaims;
                // create a list of claims that we want to transfer into our store
                var filtered = new List<Claim>();

                // user's display name
                var name = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name)?.Value ??
                           claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
                if (name != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, name));
                }
                else
                {
                    // build the name from first + last (for facebook)
                    var first = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value ??
                                claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
                    var last = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value ??
                               claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;
                    if (first != null && last != null)
                    {
                        name = first + " " + last;
                        filtered.Add(new Claim(JwtClaimTypes.Name, name));
                    }
                    else if (first != null)
                    {
                        filtered.Add(new Claim(JwtClaimTypes.Name, first));
                    }
                    else if (last != null)
                    {
                        filtered.Add(new Claim(JwtClaimTypes.Name, last));
                    }
                }

                // email will get mapped, so we don't add it explicitly
                var email = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Email)?.Value ??
                            claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;


                var pictureUrl = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Picture)?.Value;
                if (pictureUrl != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Picture, pictureUrl));
                }

                var user = new ApplicationUser
                {
                    UserName = name,
                    Email = email
                };
                var identityResult = await _userManager.CreateAsync(user);
                if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

                if (filtered.Any())
                {
                    identityResult = await _userManager.AddClaimsAsync(user, filtered);
                    if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);
                }

                // Add this idP login to user
                identityResult =
                    await _userManager.AddLoginAsync(user, new UserLoginInfo(
                        externalUserInfo.Provider,
                        externalUserInfo.ProviderUserId,
                        externalUserInfo.Provider)
                    );
                if (!identityResult.Succeeded) throw new Exception(identityResult.Errors.First().Description);

                return user;
            }
        }
    }
}