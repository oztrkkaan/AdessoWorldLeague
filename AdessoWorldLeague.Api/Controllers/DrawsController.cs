using AdessoWorldLeague.Application.Features.MakeDraw;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AdessoWorldLeague.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrawsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> MakeDraw([FromBody] MakeDrawCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Ok(result);
    }
}
