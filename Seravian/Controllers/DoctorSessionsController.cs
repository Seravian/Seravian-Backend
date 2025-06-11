using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seravian.ActionFilters;
using Seravian.DTOs.DoctorSessions;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Doctor")]
[UseVerifiedDoctorOnly]
public class DoctorSessionsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IValidator<AcceptPendingSessionBookingRequestDto> _acceptPendingSessionBookingRequestDtoValidator;

    //validators
    // private readonly IValidator<CreateSessionBookingRequestDto> _sendSessionBookingRequestDtoValidator;

    public DoctorSessionsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("get-session-bookings")]
    public async Task<ActionResult<List<GetSessionBookingResponseDto>>> GetSessionBookings(
        [FromQuery] GetSessionBookingsRequestDto request
    )
    {
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var sessionBookings = _dbContext
                .SessionBookings.AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Patient)
                .ThenInclude(x => x.User)
                .Where(x => x.DoctorId == doctorId);

            if (request.StatusFilter is not null)
            {
                sessionBookings = sessionBookings.Where(x => x.Status == request.StatusFilter);
            }

            var response = await sessionBookings
                .Select(x => new GetSessionBookingResponseDto
                {
                    Id = x.Id,
                    PatientIsAvailableFromUtc = x.PatientIsAvailableFromUtc,
                    PatientIsAvailableToUtc = x.PatientIsAvailableToUtc,
                    ScheduledAtUtc = x.ScheduledAtUtc,
                    SessionPrice = x.SessionPrice,
                    PatientNote = x.PatientNote,
                    DoctorNote = x.DoctorNote,
                    CreatedAtUtc = x.CreatedAtUtc,
                    Status = x.Status,
                    PatientAge = DateTime.Now.Year - x.Patient.User.DateOfBirth!.Value.Year,
                    PatientGender = x.Patient.User.Gender!.Value,
                })
                .OrderByDescending(x =>
                    x.ScheduledAtUtc.HasValue ? x.ScheduledAtUtc.Value : x.CreatedAtUtc
                )
                .ToListAsync();

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(
                new { Errors = new List<string> { "Failed to get session bookings" } }
            );
        }
    }

    [HttpGet("get-session-booking")]
    public async Task<ActionResult<GetSessionBookingResponseDto>> GetSessionBooking(
        [FromQuery] GetSessionBookingRequestDto request
    )
    {
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var sessionBooking = await _dbContext
                .SessionBookings.AsNoTracking()
                .AsSplitQuery()
                .Include(x => x.Patient)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x =>
                    x.Id == request.SessionBookingId && x.DoctorId == doctorId
                );

            if (sessionBooking is null)
                return BadRequest(
                    new { Errors = new List<string> { "Session booking not found." } }
                );

            var response = new GetSessionBookingResponseDto
            {
                Id = sessionBooking.Id,
                PatientIsAvailableFromUtc = sessionBooking.PatientIsAvailableFromUtc,
                PatientIsAvailableToUtc = sessionBooking.PatientIsAvailableToUtc,
                ScheduledAtUtc = sessionBooking.ScheduledAtUtc,
                SessionPrice = sessionBooking.SessionPrice,
                PatientNote = sessionBooking.PatientNote,
                DoctorNote = sessionBooking.DoctorNote,
                CreatedAtUtc = sessionBooking.CreatedAtUtc,
                Status = sessionBooking.Status,
                PatientAge =
                    DateTime.Now.Year - sessionBooking.Patient.User.DateOfBirth!.Value.Year,
                PatientGender = sessionBooking.Patient.User.Gender!.Value,
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(
                new { Errors = new List<string> { "Failed to get session booking" } }
            );
        }
    }

    [HttpPatch("accept-pending-session-booking")]
    public async Task<IActionResult> AcceptPendingSessionBookingAsync(
        [FromBody] AcceptPendingSessionBookingRequestDto request
    )
    {
        var utcNow = DateTime.UtcNow;
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        var validationResult = await _acceptPendingSessionBookingRequestDtoValidator.ValidateAsync(
            request
        );

        if (!validationResult.IsValid)
        {
            return BadRequest(new { Errors = validationResult.Errors.Select(x => x.ErrorMessage) });
        }

        try
        {
            var sessionBooking = await _dbContext.SessionBookings.FirstOrDefaultAsync(x =>
                x.Id == request.SessionBookingId && x.DoctorId == doctorId
            );

            if (sessionBooking is null)
                return BadRequest(
                    new { Errors = new List<string> { "Session booking not found." } }
                );

            if (sessionBooking.Status != SessionBookingStatus.Pending)
                return BadRequest(
                    new { Errors = new List<string> { "Session booking is not pending." } }
                );

            if (request.ScheduledAtUtc >= sessionBooking.PatientIsAvailableToUtc)
            {
                return BadRequest(
                    new
                    {
                        Errors = new[]
                        {
                            "Scheduled time must be before patient's availability end time.",
                        },
                    }
                );
            }

            if (request.ScheduledAtUtc < sessionBooking.PatientIsAvailableFromUtc)
            {
                return BadRequest(
                    new
                    {
                        Errors = new[]
                        {
                            "Scheduled time must be after or equal to the patient's availability start time.",
                        },
                    }
                );
            }

            if ((sessionBooking.PatientIsAvailableFromUtc - utcNow) < TimeSpan.FromMinutes(61))
            {
                return BadRequest(
                    new
                    {
                        Errors = new[]
                        {
                            "Patient must be available for at least one hour before session.",
                        },
                    }
                );
            }
            if ((sessionBooking.PatientIsAvailableToUtc - request.ScheduledAtUtc).TotalMinutes < 60)
            {
                return BadRequest(
                    new
                    {
                        Errors = new[]
                        {
                            "Scheduled time must be at least one hour before patient's availability end time.",
                        },
                    }
                );
            }

            sessionBooking.Status = SessionBookingStatus.Accepted;

            //  remove the next line when using real payment gateway
            sessionBooking.Status = SessionBookingStatus.Paid;

            sessionBooking.UpdatedAtUtc = utcNow;
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
                        "someone has changed the session booking status at the same time",
                    },
                }
            );
        }
        catch (Exception ex)
        {
            return BadRequest(
                new { Errors = new List<string> { "Failed to accept session booking" } }
            );
        }
    }

    [HttpPatch("reject-pending-session-booking")]
    public async Task<IActionResult> RejectPendingSessionBookingAsync(
        [FromBody] RejectPendingSessionBookingRequestDto request
    )
    {
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var sessionBooking = await _dbContext.SessionBookings.FirstOrDefaultAsync(x =>
                x.Id == request.SessionBookingId && x.DoctorId == doctorId
            );

            if (sessionBooking is null)
                return NotFound(new { Errors = new List<string> { "Session booking not found." } });

            if (sessionBooking.Status != SessionBookingStatus.Pending)
                return BadRequest(
                    new { Errors = new List<string> { "Session booking is not pending." } }
                );

            sessionBooking.Status = SessionBookingStatus.Rejected;
            sessionBooking.UpdatedAtUtc = DateTime.UtcNow;
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
                        "someone has changed the session booking status at the same time",
                    },
                }
            );
        }
        catch (Exception ex)
        {
            return BadRequest(
                new { Errors = new List<string> { "Failed to reject session booking" } }
            );
        }
    }
}

namespace Seravian.DTOs.DoctorSessions
{
    public class RejectPendingSessionBookingRequestDto
    {
        public Guid SessionBookingId { get; set; }
    }

    public class AcceptPendingSessionBookingRequestDto
    {
        public Guid SessionBookingId { get; set; }

        public DateTime ScheduledAtUtc { get; set; }
    }
}
