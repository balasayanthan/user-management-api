using Application.Common;
using Application.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public sealed class UsersController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<PagedResult<UserDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20,
            [FromQuery] Guid? groupId = null, [FromQuery] string? search = null, [FromQuery] string? sortBy = "LastName",
            [FromQuery] string sortDir = "asc", CancellationToken ct = default)
        {
            return Ok(await mediator.Send(new ListUsersQuery(page, pageSize, groupId, search, sortBy, sortDir), ct));
        }
            

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> Get(Guid id, CancellationToken ct = default)
        {
            return Ok(await mediator.Send(new GetUserByIdQuery(id), ct));
        }
            

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateUserDto dto, CancellationToken ct = default)
        {
            var id = await mediator.Send(new CreateUserCommand(dto), ct);
            return CreatedAtAction(nameof(Get), new { id, version = "1.0" }, id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken ct = default)
        {
            await mediator.Send(new UpdateUserCommand(id, dto), ct);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            await mediator.Send(new DeleteUserCommand(id), ct);
            return NoContent();
        }
    }
}
