using System;
using Auth.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Internal;

namespace Auth.WebApp.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            switch (exception)
            {
                case NotFoundException notFoundException:
                    context.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    context.Result = new NotFoundResult();
                    return;
                case BadRequestException badRequestException:
                    context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                    if (badRequestException.Errors != null && badRequestException.Errors.Any())
                    {
                        context.Result = new BadRequestObjectResult(badRequestException.Errors);
                    }
                    else
                    {
                        context.Result = new BadRequestResult();
                    }

                    return;
            }
        }
    }
}