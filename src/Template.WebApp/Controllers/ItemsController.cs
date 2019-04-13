using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Cqrs.Items.Queries;
using Template.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Template.Application.Cqrs.Items;
using Template.Application.Cqrs.Items.Commands;

namespace Template.WebApp.Controllers
{
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ItemsController : ApiController
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll()
            => Ok(await Mediator.Send(new GetAllItems()));

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> Get(Guid id)
        {
            var item = await Mediator.Send(new GetItem {Id = id});
            return item ?? (ActionResult<ItemDto>) NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> Create(CreateItem item)
        {
            var itemDto = await Mediator.Send(item);
            return CreatedAtAction(nameof(Get), new {itemDto.Id}, item);
        }
    }
}