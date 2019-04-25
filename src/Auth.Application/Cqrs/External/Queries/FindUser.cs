using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Auth.Domain.Entities;
using IdentityModel;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Cqrs.External.Queries
{
    public class FindUser : IRequest<ExternalUserDto>
    {
        public FindUser(AuthenticateResult authenticateResult) => AuthenticateResult = authenticateResult;

        public AuthenticateResult AuthenticateResult { get; }


        public class Handler : IRequestHandler<FindUser, ExternalUserDto>
        {
            private readonly UserManager<ApplicationUser> _userManager;

            public Handler(UserManager<ApplicationUser> userManager)
            {
                _userManager = userManager;
            }

            public async Task<ExternalUserDto> Handle(FindUser request, CancellationToken cancellationToken)
            {
                var externalUser = request.AuthenticateResult.Principal;

                // try to determine the unique id of the external user (issued by the provider)
                // the most common claim type for that are the sub claim and the NameIdentifier
                // depending on the external provider, some other claim type might be used
                var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                                  externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                                  throw new Exception("Unknown userid");

                // remove the user id claim so we don't include it as an extra claim if/when we provision the user
                var claims = externalUser.Claims.ToList();
                claims.Remove(userIdClaim);

                var provider = request.AuthenticateResult.Properties.Items["scheme"];
                var providerUserId = userIdClaim.Value;

                // find external user
                var user = await _userManager.FindByLoginAsync(provider, providerUserId);

                return new ExternalUserDto()
                {
                    User = user,
                    Provider = provider,
                    ProviderUserId =  providerUserId,
                    ExternalClaims = claims
                };
            }
        }
    }
}