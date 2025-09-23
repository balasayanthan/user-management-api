using Application.Reports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reports")]
    [Authorize]
    public sealed class ReportsController(IMediator mediator) : ControllerBase
    {
        [Authorize(Policy = "CanViewReports")]
        [HttpGet("user-names-by-permission")]
        public async Task<ActionResult<IReadOnlyList<string>>> Names([FromQuery] bool permission, [FromQuery] string? ruleName = null, CancellationToken ct = default)
        {
            return Ok(await mediator.Send(new ListUserNamesByPermissionQuery(permission, ruleName),ct));
        }
    }
}
