using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.GymDtos.GymReview;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewController(IReviewService service) : ControllerBase
{
    [HttpPost("add-review")]
    [Authorize]
    public async Task<IActionResult> AddReview([FromBody] ReviewCreateDto dto)
        => Ok(await service.AddReviewAsync(dto));

    [HttpGet("gym/{gymId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGymReviews([FromRoute] Guid gymId)
        => Ok(await service.GetGymReviewsAsync(gymId));

    [HttpDelete("{reviewId}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview([FromRoute] string reviewId)
        => Ok(await service.DeleteReviewAsync(reviewId));
}
