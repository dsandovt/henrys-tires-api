using HenryTires.Inventory.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace HenryTires.Inventory.Api.Controllers;

[ApiController]
[Route("api/v1/config")]
public class ConfigController : ControllerBase
{
    private readonly ITimezoneConverter _timezoneConverter;

    public ConfigController(ITimezoneConverter timezoneConverter)
    {
        _timezoneConverter = timezoneConverter;
    }

    [HttpGet]
    public IActionResult GetConfig()
    {
        return Ok(new { timezone = _timezoneConverter.GetTimezoneId() });
    }
}
