using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Crmf;
using Startup_Project.Application.Dtos.GymDtos.GymReview;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewController(IReviewService service) : ControllerBase
{
    [Authorize]
    [HttpPost("add-review")]
    public async Task<IActionResult> AddReview([FromBody] ReviewCreateDto dto)
    {
        var result = await service.AddReviewAsync(dto);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
    [HttpGet("gym/{gymId}")]
    public async Task<IActionResult> GetGymReviews([FromRoute] Guid gymId)
    {
        var result = await service.GetGymReviewsAsync(gymId);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
