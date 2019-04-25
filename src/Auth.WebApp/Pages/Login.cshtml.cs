using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using IdentityServer4.Events;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using Auth.Domain.Entities;

namespace Auth.WebApp.Pages
{
    public class LoginModel : PageModel
    {
        private static ILogger Log = Serilog.Log.Logger.ForContext<LoginModel>();
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;

        public LoginModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
        }

        [BindProperty] public LoginInputModel InputModel { get; set; }
        public IEnumerable<AuthenticationScheme> ExternalProviders { get; private set; }

        public bool AllowLocal { get; } = true;

        public string ReturnUrl { get; private set; }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            InputModel = new LoginInputModel {Username = context?.LoginHint};
            if (context?.IdP != null)
            {
                return RedirectToAction("Challenge", "External",
                    new {provider = context.IdP, returnUrl});
            }

            ExternalProviders = await _signInManager.GetExternalAuthenticationSchemesAsync();
            var client = await _clientStore.FindEnabledClientByIdAsync(context?.ClientId);
            if (client?.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
            {
                ExternalProviders = ExternalProviders.Where(provider =>
                    client.IdentityProviderRestrictions.Contains(provider.Name));
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string button, string returnUrl = null)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            // the user clicked the "cancel" button
            if (button != "login")
            {
                // since we don't have a valid context, then we just go back to the home page
                if (context == null) return Redirect("~/");

                // if the user cancels, send a result back into IdentityServer as if they 
                // denied the consent (even if this client does not require consent).
                // this will send back an access denied OIDC error response to the client.
                await _interaction.GrantConsentAsync(context, ConsentResponse.Denied);
                return Redirect(returnUrl);
            }

            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.PasswordSignInAsync(InputModel.Username, InputModel.Password,
                InputModel.RememberMe, true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(InputModel.Username);
                await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

                if (context != null || Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect("~/");
                }
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            await _events.RaiseAsync(new UserLoginFailureEvent(InputModel.Username, "invalid credentials"));
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            if (result.IsLockedOut)
            {
                Log.Warning("{Username} locked out", InputModel.Username);
            }

            return Page();
        }


        public class LoginInputModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public bool RememberMe { get; set; }
        }

        public class LoginInputModelValidator : AbstractValidator<LoginInputModel>
        {
            public LoginInputModelValidator()
            {
                RuleFor(model => model.Username).NotEmpty();
                RuleFor(model => model.Password).NotEmpty();
            }
        }
    }
}