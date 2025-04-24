using aramex;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ShippingController : ControllerBase
{
    private readonly AramexService _aramexService;

    public ShippingController(AramexService aramexService)
    {
        _aramexService = aramexService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateShipment([FromBody] SimpleShipmentRequest request)
    {
        var result = await _aramexService.CreateShipment(request);
        return Ok(result);
    }
}
