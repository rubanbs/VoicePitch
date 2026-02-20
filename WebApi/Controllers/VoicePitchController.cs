using Microsoft.AspNetCore.Mvc;
using WebApi.Dto;
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

    [HttpGet("getpitches")]
    public List<PitchDto> GetPitches()
    {
        var result = new List<PitchDto>
        {
            new PitchDto("Бас", "75 - 330 Гц"),
            new PitchDto("Тенор", "120 - 500 Гц"),
            new PitchDto("Меццо - сопрано","170 - 700 Гц"),
            new PitchDto("Сопрано", "230 - 1100 Гц")
        };

        return result;
    }
}
