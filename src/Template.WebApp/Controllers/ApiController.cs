using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Template.WebApp.Controllers
{
    [ApiController, Route("api/[controller]")]
    
    public abstract class ApiController: ControllerBase
    {

        private IMediator _mediator;
        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
    }
}