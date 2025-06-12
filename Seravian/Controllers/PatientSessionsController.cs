using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seravian.ActionFilters;
using Seravian.DTOs.PatientSessions;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public class PatientSessionsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _dbContext;

    //validators
    private readonly IValidator<CreateSessionBookingRequestDto> _sendSessionBookingRequestDtoValidator;

    public PatientSessionsController(
        ApplicationDbContext dbContext,
        IValidator<CreateSessionBookingRequestDto> sendSessionBookingRequestDtoValidator,
        IConfiguration configuration
    )
    {
        _config = configuration;
        _dbContext = dbContext;
        _sendSessionBookingRequestDtoValidator = sendSessionBookingRequestDtoValidator;
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
                    DoctorImageUrl =
                        d.ProfileImagePath != null ? _config["BaseUrl"] + d.ProfileImagePath : null,
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

    [HttpPost("create-session-booking")]
    public async Task<IActionResult> CreateSessionBookingAsync(
        [FromBody] CreateSessionBookingRequestDto request
    )
    {
        var validationResult = _sendSessionBookingRequestDtoValidator.Validate(request);
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        if (!validationResult.IsValid)
        {
            return BadRequest(new { Errors = validationResult.Errors.Select(x => x.ErrorMessage) });
        }

        try
        {
            var doctor = await _dbContext
                .Doctors.AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == request.DoctorId);

            if (doctor is null)
                return BadRequest(new { Errors = new List<string> { "Doctor not found." } });

            if (doctor.VerifiedAtUtc is null)
                return BadRequest(new { Errors = new List<string> { "Doctor is not verified." } });

            var sessionBooking = new SessionBooking
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                DoctorId = request.DoctorId,
                PatientIsAvailableFromUtc = request.PatientIsAvailableFromUtc,
                PatientIsAvailableToUtc = request.PatientIsAvailableToUtc,
                SessionPrice = doctor.SessionPrice!.Value,
                ScheduledAtUtc = null,
                Status = SessionBookingStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow,
                PatientNote = request.PatientNote,
                DoctorNote = null,
            };
            await _dbContext.SessionBookings.AddAsync(sessionBooking);
            await _dbContext.SaveChangesAsync();
            return Ok(new CreateSessionBookingResponseDto { SessionBookingId = sessionBooking.Id });
        }
        catch
        {
            return BadRequest(new { Errors = new List<string> { "Something went wrong." } });
        }
    }

    [HttpDelete("delete-pending-session-booking")]
    public async Task<IActionResult> DeletePendingSessionBookingAsync(
        [FromQuery] DeletePendingSessionBookingRequestDto request
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var sessionBooking = await _dbContext.SessionBookings.FirstOrDefaultAsync(x =>
                x.Id == request.SessionBookingId && x.PatientId == patientId
            );

            if (sessionBooking is null)
                return BadRequest(
                    new { Errors = new List<string> { "Session booking not found." } }
                );

            if (sessionBooking.Status != SessionBookingStatus.Pending)
                return BadRequest(
                    new { Errors = new List<string> { "Session booking is not pending." } }
                );

            sessionBooking.Status = SessionBookingStatus.Deleted;
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(
                new
                {
                    Errors = new List<string>
                    {
                        "The session booking was modified by someone else. Please refresh and try again.",
                    },
                }
            );
        }
        catch
        {
            return BadRequest(new { Errors = new List<string> { "Something went wrong." } });
        }
    }

    [HttpGet("get-session-bookings")]
    public async Task<ActionResult<List<GetSessionBookingsResponseDto>>> GetSessionBookings(
        [FromQuery] GetSessionBookingsRequestDto request
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var sessionBookings = _dbContext
                .SessionBookings.AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Doctor)
                .ThenInclude(x => x.User)
                .Where(x => x.PatientId == patientId);

            if (request.StatusFilter is not null)
            {
                sessionBookings = sessionBookings.Where(x => x.Status == request.StatusFilter);
            }

            var response = await sessionBookings
                .Select(x => new GetSessionBookingsResponseDto
                {
                    Id = x.Id,
                    DoctorId = x.DoctorId,
                    DoctorFullName = x.Doctor.User.FullName!,
                    DoctorTitle = x.Doctor.Title!.Value,
                    DoctorDescription = x.Doctor.Description!,
                    DoctorAge = DateTime.Now.Year - x.Doctor.User.DateOfBirth!.Value.Year,
                    DoctorGender = x.Doctor.User.Gender!.Value,
                    PatientIsAvailableFromUtc = x.PatientIsAvailableFromUtc,
                    PatientIsAvailableToUtc = x.PatientIsAvailableToUtc,
                    ScheduledAtUtc = x.ScheduledAtUtc,
                    SessionPrice = x.SessionPrice,
                    PatientNote = x.PatientNote,
                    Status = x.Status,
                    DoctorNote = x.DoctorNote,
                    CreatedAtUtc = x.CreatedAtUtc,
                    DoctorImageUrl =
                        x.Doctor.ProfileImagePath != null
                            ? _config["BaseUrl"] + x.Doctor.ProfileImagePath
                            : null,
                })
                .OrderByDescending(x =>
                    x.ScheduledAtUtc == null ? x.CreatedAtUtc : x.ScheduledAtUtc
                )
                .ToListAsync();
            return Ok(response);
        }
        catch
        {
            return BadRequest(new { Errors = new List<string> { "Something went wrong." } });
        }
    }

    [HttpGet("get-session-booking")]
    public async Task<ActionResult<GetSessionBookingsResponseDto>> GetSessionBooking(
        [FromQuery] GetSessionBookingRequestDto request
    )
    {
        var patientId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var sessionBooking = await _dbContext
                .SessionBookings.AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Doctor)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == request.SessionBookingId && x.PatientId == patientId
                );

            if (sessionBooking is null)
                return BadRequest(
                    new { Errors = new List<string> { "Session booking not found." } }
                );

            var response = new GetSessionBookingsResponseDto
            {
                Id = sessionBooking.Id,
                DoctorId = sessionBooking.DoctorId,
                DoctorFullName = sessionBooking.Doctor.User.FullName!,
                DoctorTitle = sessionBooking.Doctor.Title!.Value,
                DoctorDescription = sessionBooking.Doctor.Description!,
                DoctorAge = DateTime.Now.Year - sessionBooking.Doctor.User.DateOfBirth!.Value.Year,
                DoctorGender = sessionBooking.Doctor.User.Gender!.Value,
                PatientIsAvailableFromUtc = sessionBooking.PatientIsAvailableFromUtc,
                PatientIsAvailableToUtc = sessionBooking.PatientIsAvailableToUtc,
                ScheduledAtUtc = sessionBooking.ScheduledAtUtc,
                SessionPrice = sessionBooking.SessionPrice,
                PatientNote = sessionBooking.PatientNote,
                Status = sessionBooking.Status,
                DoctorNote = sessionBooking.DoctorNote,
                CreatedAtUtc = sessionBooking.CreatedAtUtc,
                DoctorImageUrl = sessionBooking.Doctor.ProfileImagePath is not null
                    ? _config["BaseUrl"] + sessionBooking.Doctor.ProfileImagePath
                    : null,
            };
            return Ok(response);
        }
        catch
        {
            return BadRequest(new { Errors = new List<string> { "Something went wrong." } });
        }
    }
}
