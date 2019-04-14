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
        public async Task<ActionResult<ItemDto>> Get([FromRoute] Guid id)
        {
            var item = await Mediator.Send(new GetItem(id));
            return item;
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> Create([FromBody] ItemInputModel item)
        {
            var itemDto = await Mediator.Send(new CreateItem(item));
            return CreatedAtAction(nameof(Get), new {itemDto.Id}, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] ItemDto itemDto)
        {
            await Mediator.Send(new EditItem(id, itemDto));
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            await Mediator.Send(new DeleteItem(id));
            return Ok();
        }
    }
}