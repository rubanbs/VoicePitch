using Microsoft.AspNetCore.Mvc;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoicePitchController : Controller
{
    private readonly PitchStreamService _streamService;

    public VoicePitchController(PitchStreamService streamService)
    {
        _streamService = streamService;
    }

    [HttpGet("ws")]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await _streamService.HandleConnectionAsync(socket, CancellationToken.None);
    }
}
