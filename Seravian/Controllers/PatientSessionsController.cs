using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seravian.ActionFilters;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public class PatientSessionsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public PatientSessionsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("get-doctors")]
    public async Task<ActionResult<List<GetDoctorsResponseDto>>> GetDoctors()
    {
        try
        {
            var doctors = await _dbContext
                .Doctors.AsNoTracking()
                .Include(d => d.User)
                .Where(d => d.VerifiedAtUtc != null)
                .Select(d => new GetDoctorsResponseDto
                {
                    DoctorId = d.UserId,
                    DoctorTitle = d.Title!.Value,
                    DoctorSessionPrice = d.SessionPrice!.Value,
                    DoctorDescription = d.Description!,
                    DoctorFullName = d.User.FullName!,
                    DoctorAge = DateTime.UtcNow.Year - d.User.DateOfBirth!.Value.Year,
                    DoctorGender = d.User.Gender!.Value,
                })
                .OrderBy(d => d.DoctorFullName)
                .ToListAsync();

            return Ok(doctors);
        }
        catch
        {
            return NotFound(new { Errors = new List<string> { "Something went wrong." } });
        }
    }

    [HttpGet("get-doctor")]
    public async Task<ActionResult<GetDoctorsResponseDto>> GetDoctor(
        [FromQuery] GetDoctorRequestDto request
    )
    {
        try
        {
            var doctor = await _dbContext
                .Doctors.AsNoTracking()
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.VerifiedAtUtc != null && d.UserId == request.DoctorId);

            if (doctor is null)
                return BadRequest(new { Errors = new List<string> { "Doctor not found." } });

            var response = new GetDoctorsResponseDto
            {
                DoctorId = doctor.UserId,
                DoctorTitle = doctor.Title!.Value,
                DoctorSessionPrice = doctor.SessionPrice!.Value,
                DoctorDescription = doctor.Description!,
                DoctorFullName = doctor.User.FullName!,
                DoctorAge = DateTime.Now.Year - doctor.User.DateOfBirth!.Value.Year,
                DoctorGender = doctor.User.Gender!.Value,
            };
            return Ok(response);
        }
        catch
        {
            return BadRequest(new { Errors = new List<string> { "Something went wrong." } });
        }
    }
}

public class GetDoctorRequestDto
{
    public Guid DoctorId { get; set; }
}

public class GetDoctorsResponseDto
{
    public Guid DoctorId { get; set; }
    public DoctorTitle DoctorTitle { get; set; }
    public int DoctorSessionPrice { get; set; }
    public string DoctorDescription { get; set; }

    public string DoctorFullName { get; set; }
    public int DoctorAge { get; set; }
    public Gender DoctorGender { get; set; }
}
