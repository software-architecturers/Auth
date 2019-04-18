using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Template.WebApp.Filters;

namespace Auth.WebApp.Controllers
{
    [ApiController, Route("api/[controller]")]
    [ApiExceptionFilter]
    public abstract class ApiController : ControllerBase
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
    }
}