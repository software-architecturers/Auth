using Microsoft.AspNetCore.Mvc;

namespace Template.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public abstract class ApiController: ControllerBase
    {
        
    }
}