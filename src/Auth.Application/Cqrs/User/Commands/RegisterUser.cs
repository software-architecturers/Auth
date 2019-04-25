using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Auth.Application.Exceptions;
using Auth.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Application.Cqrs.User.Commands
{
    public class RegisterUser : IRequest<Unit>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public class Handler : IRequestHandler<RegisterUser, Unit>
        {
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly SignInManager<ApplicationUser> _signInManager;

            public Handler(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager)
            {
                _userManager = userManager;
                _signInManager = signInManager;
            }

            public async Task<Unit> Handle(RegisterUser request, CancellationToken cancellationToken)
            {
                var user = new ApplicationUser {UserName = request.Email, Email = request.Email, EmailConfirmed = true};
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var identityErrors = result.Errors.Select(error => error.Description).ToArray();
                    var builder =
                        ImmutableDictionary.CreateBuilder<string, string[]>();
                    builder.Add(string.Empty, identityErrors);
                    throw new BadRequestException(builder.ToImmutable());
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return Unit.Value;
            }
        }

        public class Validator : AbstractValidator<RegisterUser>
        {
            public Validator()
            {
                RuleFor(model => model.Email).EmailAddress().NotEmpty();
                RuleFor(model => model.Password).Length(6, 100);
                RuleFor(model => model.ConfirmPassword).Length(6, 100);
                RuleFor(model => model.Password).Equal(model => model.ConfirmPassword);
            }
        }
    }
}