using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Trainer;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers.Trainer;

[Route("api/trainer/me/students")]
[ApiController]
[Authorize(Roles = "Trainer")]
public class TrainerStudentsController(ITrainerStudentService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMyStudents(
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
        => Ok(await service.GetMyStudentsAsync(search, status));

    [HttpPost]
    public async Task<IActionResult> AddStudent([FromBody] AddStudentDto request)
        => Ok(await service.AddStudentAsync(request));

    [HttpDelete("{studentId:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid studentId)
        => Ok(await service.RemoveStudentAsync(studentId));
}
