using System.Diagnostics;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Auth.WebApp.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IHostingEnvironment _environment;

        public ErrorModel(IIdentityServerInteractionService interaction, IHostingEnvironment environment)
        {
            _interaction = interaction;
            _environment = environment;
        }

        public ErrorMessage ErrorMessage { get; set; }
        public string RequestId { get; set; }
        
        public async Task OnGetAsync([FromQuery] string errorId)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorMessage = await _interaction.GetErrorContextAsync(errorId);
            if (!_environment.IsDevelopment())
            {
                ErrorMessage.ErrorDescription = null;
            }
        }
}

}