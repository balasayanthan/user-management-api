using Application.Common;
using Application.Users;
using MediatR;
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
            [FromQuery] string sortDir = "asc")
        {
            return Ok(await mediator.Send(new ListUsersQuery(page, pageSize, groupId, search, sortBy, sortDir)));
        }
            

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> Get(Guid id)
        {
            return Ok(await mediator.Send(new GetUserByIdQuery(id)));
        }
            

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateUserDto dto)
        {
            var id = await mediator.Send(new CreateUserCommand(dto));
            return CreatedAtAction(nameof(Get), new { id, version = "1.0" }, id);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            await mediator.Send(new UpdateUserCommand(id, dto));
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await mediator.Send(new DeleteUserCommand(id));
            return NoContent();
        }
    }
}
