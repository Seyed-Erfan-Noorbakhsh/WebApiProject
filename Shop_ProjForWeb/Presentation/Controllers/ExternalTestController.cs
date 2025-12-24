namespace Shop_ProjForWeb.Presentation.Controllers;

using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Core.Application.Services;

[ApiController]
[Route("api/[controller]")]
public class ExternalTestController : ControllerBase
{
    private readonly AgifyService _agifyService;

    public ExternalTestController(AgifyService agifyService)
    {
        _agifyService = agifyService;
    }

    [HttpGet("age/{name}")]
    public async Task<IActionResult> GetAge(string name)
    {
        var age = await _agifyService.GetPredictedAgeAsync(name);
        return Ok(new { name, age });
    }
}
