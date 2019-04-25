using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth.WebApp.Pages
{
    public class LoggedOutModel : PageModel
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IHostingEnvironment _env;

        public LoggedOutModel(IIdentityServerInteractionService interaction, IHostingEnvironment env)
        {
            _interaction = interaction;
            _env = env;
            AutomaticRedirectAfterSignOut = !env.IsDevelopment();
        }
        public string ClientName { get; private set; }
        public string PostLogoutRedirectUri { get; private set; }
        public string SignOutIframeUrl { get; private set; }
        public bool AutomaticRedirectAfterSignOut { get; }
        
        public async Task OnGet(string logoutId)
        {
            var logout = await _interaction.GetLogoutContextAsync(logoutId);
            ClientName = string.IsNullOrEmpty(logout?.ClientName) ? logout?.ClientId : logout?.ClientName;
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri;
            SignOutIframeUrl = logout?.SignOutIFrameUrl;
        }
    }
}