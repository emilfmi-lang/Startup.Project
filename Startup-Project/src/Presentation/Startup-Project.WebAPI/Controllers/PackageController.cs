using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Services.Interfaces;

namespace Startup_Project.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PackageController(IPackageService service) : ControllerBase
{

}
