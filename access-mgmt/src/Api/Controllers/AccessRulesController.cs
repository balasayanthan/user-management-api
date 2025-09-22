using Application.AccessRules;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/groups/{groupId:guid}/rules")]
    public sealed class AccessRulesController(IMediator mediator) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AccessRuleDto>>> List(Guid groupId)
        {
            return Ok(await mediator.Send(new ListRulesByGroupQuery(groupId)));
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create(Guid groupId, [FromBody] CreateAccessRuleDto dto)
        {
            var id = await mediator.Send(new AddAccessRuleCommand(dto with { UserGroupId = groupId }));
            return CreatedAtAction(nameof(List), new { groupId, version = "1.0" }, id);
        }

        [HttpDelete("{ruleId:guid}")]
        public async Task<IActionResult> Delete(Guid groupId, Guid ruleId)
        {
            await mediator.Send(new RemoveAccessRuleCommand(groupId, ruleId));
            return NoContent();
        }
    }
}
