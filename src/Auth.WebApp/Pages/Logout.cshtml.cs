using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Auth.Domain.Entities;

namespace Auth.WebApp.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public LogoutModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events)
        {
            _signInManager = signInManager;
            _interaction = interaction;
            _events = events;
        }

        public string LogoutId { get; private set; }

        public async Task<IActionResult> OnGetAsync(string logoutId = null)
        {
            LogoutId = logoutId;
            if (User.Identity.IsAuthenticated != true)
            {
                return Redirect("~/");
            }

            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (context?.ShowSignoutPrompt == false)
            {
                return await OnPostAsync(logoutId);
            }

            return Page();
        }


        public async Task<IActionResult> OnPostAsync(string logoutId = null)
        {
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (User?.Identity.IsAuthenticated == true)
            {
                await _signInManager.SignOutAsync();
                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            }

            string idp = User?.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            if (idp != null &&
                idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider &&
                await HttpContext.GetSchemeSupportsSignOutAsync(idp))
            {
                logoutId = await _interaction.CreateLogoutContextAsync();
                string url = Url.Page("Logout", new {LogoutId = logoutId});
                return SignOut(new AuthenticationProperties {RedirectUri = url}, idp);
            }

            return RedirectToPage("LoggedOut", new {LogoutId = logoutId});
        }
    }
}