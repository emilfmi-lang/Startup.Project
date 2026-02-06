using Startup_Project.Application.Dtos.GymDtos;
using Startup_Project.Application.Dtos.GymOpeningHour;
using Startup_Project.Application.Dtos.Packages;
using Startup_Project.Application.Dtos.Trainer;

namespace Startup_Project.Application.Dtos.GymDtos.GymDetail;

public class GymDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
    public string? MainImage { get; set; }
    public List<GymImageDto> Images { get; set; } = [];

    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public bool IsOpen { get; set; }
    public bool IsFavorite { get; set; }
    public int FavoriteCount { get; set; }

    public List<GymAmenityDto> Amenities { get; set; } = [];

    public List<OpeningHourResponseDto> OpeningHours { get; set; } = [];

    public List<PackageDetailDto> Packages { get; set; } = [];

    public List<TrainerListDto> Trainers { get; set; } = [];

    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
