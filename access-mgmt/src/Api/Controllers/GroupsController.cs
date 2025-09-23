using Application.Common;
using Application.Groups;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/groups")]
    public sealed class GroupsController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PagedResult<GroupDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 50 , CancellationToken ct = default)
        {
            return  Ok(await mediator.Send(new ListGroupsQuery(page, pageSize),ct));
        }
           

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<GroupDto>> Get(Guid id , CancellationToken ct = default)
        {
             return Ok(await mediator.Send(new GetGroupByIdQuery(id), ct));
        }
           

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateGroupDto dto, CancellationToken ct = default)
        {
            var id = await mediator.Send(new CreateGroupCommand(dto),ct);
            return CreatedAtAction(nameof(Get), new { id, version = "1.0" }, id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGroupDto dto, CancellationToken ct = default)
        {
            await mediator.Send(new UpdateGroupCommand(id, dto),ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            await mediator.Send(new DeleteGroupCommand(id), ct);
            return NoContent();
        }
    }
}
