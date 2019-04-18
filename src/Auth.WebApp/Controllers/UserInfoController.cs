using System;
using System.Threading.Tasks;
using Auth.Application.Cqrs.UserInfo;
using Auth.Application.Cqrs.UserInfo.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Auth.WebApp.Controllers
{
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class UserInfoController : ApiController
    {
        // GET
        [HttpGet("{id}")]
        public async Task<ActionResult<UserInfoDto>> Get([FromRoute] Guid id) 
            => await Mediator.Send(new UserInfo(id.ToString()));
    }
}