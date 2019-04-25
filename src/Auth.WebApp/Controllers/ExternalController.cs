using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Auth.Application.Cqrs.External.Commands;
using Auth.Application.Cqrs.External.Queries;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Auth.Domain.Entities;
using MediatR;

namespace Auth.WebApp.Controllers
{
    /// <summary>
    /// This controller is used for external auth
    /// </summary>
    [AllowAnonymous]
    public class ExternalController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;
        private readonly IMediator _mediator;

        public ExternalController(
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IEventService events,
            IMediator mediator)
        {
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
            _mediator = mediator;
        }

        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpGet]
        public IActionResult Challenge(string provider, string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (!(Url.IsLocalUrl(returnUrl) || _interaction.IsValidReturnUrl(returnUrl)))
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    ["returnUrl"] = returnUrl,
                    ["scheme"] = provider
                }
            };

            return Challenge(props, provider);
        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Callback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            // lookup our user and external provider info
            var externalUserDto = await _mediator.Send(new FindUser(result));


            var user = externalUserDto.User ?? await _mediator.Send(new CreateUser(externalUserDto));

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for sign-out from oidc
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallbackForOidc(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            // we must issue the cookie manually, and can't use the SignInManager because
            // it doesn't expose an API to issue additional claims from the login workflow
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            additionalLocalClaims.AddRange(principal.Claims);
            var name = principal.FindFirst(JwtClaimTypes.Name)?.Value ?? user.UserName ?? user.Id;

            await _events.RaiseAsync(new UserLoginSuccessEvent(externalUserDto.Provider, externalUserDto.ProviderUserId,
                user.Id, name));

            await HttpContext.SignInAsync(user.Id, name, externalUserDto.Provider, localSignInProps,
                additionalLocalClaims.ToArray());

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // validate return URL and redirect back to authorization endpoint or a local page
            var returnUrl = result.Properties.Items["returnUrl"];
            return _interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : Redirect("~/");
        }


        private static void ProcessLoginCallbackForOidc(AuthenticateResult externalResult,
            ICollection<Claim> localClaims,
            AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] {new AuthenticationToken {Name = "id_token", Value = idToken}});
            }
        }
    }
}