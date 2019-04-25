using System.Collections.Generic;
using System.Security.Claims;
using Auth.Domain.Entities;

namespace Auth.Application.Cqrs.External
{
    public class ExternalUserDto
    {
        public ApplicationUser User { get; set; }
        public string Provider { get; set; }
        public string ProviderUserId { get; set; }
        public IList<Claim> ExternalClaims { get; set; }
    }
}