using System;
using System.Threading;
using System.Threading.Tasks;
using Auth.Application.HttpClients.EventsCore;
using Auth.Domain.Entities;
using MediatR;

namespace Auth.Application.Cqrs.External.Commands
{
    public class GetUserToken : IRequest<TokenResponse>
    {
        public GetUserToken(ApplicationUser user)
        {
            User = user;
        }

        public ApplicationUser User { get; }


        public class Handler : IRequestHandler<GetUserToken, TokenResponse>
        {
            private readonly EventsCoreHttpClient _httpClient;

            public Handler(EventsCoreHttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task<TokenResponse> Handle(GetUserToken request, CancellationToken cancellationToken)
            {
                var user = request.User;
                // Note: ask @1besser11  about password-less
                var password = new Guid().ToString();
                var register = new RegisterRequestDto
                {
                    Login = user.UserName,
                    Email = user.Email,
                    Password = password,
                    ConfirmPassword = password
                };
                await _httpClient.Register(register);
                var login = new LoginRequestDto
                {
                    Login = user.UserName,
                    Password = password
                };
                return await _httpClient.Login(login);
            }
        }
    }
}