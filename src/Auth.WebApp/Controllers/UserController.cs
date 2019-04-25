using System.Threading.Tasks;
using Auth.Application.Cqrs.User.Commands;
using Auth.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.WebApp.Controllers
{
    // TODO: hard cors 
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class UserController : ApiController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUserCommand)
        {
            await Mediator.Send(registerUserCommand);
            return Ok();
        }
    }
}