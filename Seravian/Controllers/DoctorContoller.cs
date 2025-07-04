using System.Collections.Concurrent;
using System.Net.Mail;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seravian.DTOs.Doctor;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Doctor")]
public class DoctorController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DoctorController> _logger;
    private readonly DoctorsVerificationRequestsAttachmentFilesAccessLockingManager _lockingManager;
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> _uploadProfileImageLocks = [];

    private readonly IValidator<SendVerificationRequestRequestDto> _sendVerificationRequestDtoValidator;

    public DoctorController(
        ApplicationDbContext dbContext,
        ILogger<DoctorController> logger,
        IValidator<SendVerificationRequestRequestDto> sendVerificationRequestDtoValidator,
        DoctorsVerificationRequestsAttachmentFilesAccessLockingManager lockingManager,
        IConfiguration configuration
    )
    {
        _dbContext = dbContext;
        _logger = logger;
        _sendVerificationRequestDtoValidator = sendVerificationRequestDtoValidator;
        _lockingManager = lockingManager;
        _config = configuration;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<DoctorProfileResponseDto>> Profile()
    {
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        try
        {
            var doctor = await _dbContext
                .Doctors.Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == doctorId);

            if (doctor is null)
                return BadRequest("Doctor not found.");

            var response = new DoctorProfileResponseDto
            {
                Id = doctor.UserId,
                Email = doctor.User.Email,
                FullName = doctor.User.FullName,
                Title = doctor.Title,
                Description = doctor.Description,
                DateOfBirth = doctor.User.DateOfBirth,
                Gender = doctor.User.Gender,
                CreatedAtUtc = doctor.User.CreatedAtUtc,
                VerifiedAtUtc = doctor.VerifiedAtUtc,
                SessionPrice = doctor.SessionPrice,
                ProfileImageUrl = doctor.ProfileImagePath is not null
                    ? _config["BaseUrl"] + doctor.ProfileImagePath
                    : null,
            };
            return Ok(response);
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpPost("upload-profile-image")]
    public async Task<IActionResult> UploadDoctorImage([FromForm] IFormFile profileImageFile)
    {
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.UserId == doctorId);
        if (doctor is null)
            return BadRequest("Doctor not found.");

        if (profileImageFile == null || profileImageFile.Length == 0)
            return BadRequest("No image provided.");

        // Max 5MB
        const long maxFileSize = 5 * 1024 * 1024;
        if (profileImageFile.Length > maxFileSize)
            return BadRequest("File size exceeds the 5MB limit.");

        // Only allow specific content types
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
        if (!allowedContentTypes.Contains(profileImageFile.ContentType.ToLower()))
            return BadRequest("Only PNG and JPEG images are allowed.");

        var uploadFolderPath = Path.Combine("wwwroot", "doctor-images");
        if (!Directory.Exists(uploadFolderPath))
            Directory.CreateDirectory(uploadFolderPath);

        var extension = Path.GetExtension(profileImageFile.FileName).ToLower();

        // Sanitize extension
        if (!new[] { ".jpg", ".jpeg", ".png" }.Contains(extension))
            return BadRequest("Invalid image extension.");

        var filename = doctorId + extension;
        var filePath = Path.Combine(uploadFolderPath, filename);

        var semaphore = _uploadProfileImageLocks.GetOrAdd(doctorId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            // 🔁 Delete old files with any extension
            var existingFiles = Directory.GetFiles(uploadFolderPath, doctorId + ".*");
            foreach (var file in existingFiles)
            {
                System.IO.File.Delete(file);
            }

            using (
                var stream = new FileStream(
                    filePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None
                )
            )
            {
                await profileImageFile.CopyToAsync(stream);
            }
            doctor.ProfileImagePath = "doctor-images/" + filename;

            await _dbContext.SaveChangesAsync();
        }
        finally
        {
            semaphore.Release();
        }
        // 🔄 Save new image (overwrite if exists)


        return Ok(new { ImageUrl = _config["BaseUrl"] + doctor.ProfileImagePath });
    }

    [HttpGet("get-doctor-verification-requests")]
    public async Task<
        ActionResult<List<GetDoctorVerificationRequestResponseDto>>
    > GetDoctorVerificationRequests()
    {
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        try
        {
            List<GetDoctorVerificationRequestResponseDto> response = await _dbContext
                .DoctorsVerificationRequests.Include(d => d.Attachments)
                .Where(d => d.DoctorId == doctorId)
                .OrderByDescending(x => x.RequestedAtUtc)
                .Select(x => new GetDoctorVerificationRequestResponseDto
                {
                    Id = x.Id,
                    CreatedAtUtc = x.RequestedAtUtc,
                    Status = x.Status,
                    Title = x.Title,
                    Description = x.Description,
                    DeletedAtUtc = x.DeletedAtUtc,
                    ReviewedAtUtc = x.ReviewedAtUtc,
                    RejectionNote = x.RejectionNote,
                    Attachments = x
                        .Attachments.Select(a => new DoctorVerificationRequestAttachmentDto
                        {
                            Id = a.Id,
                            FileName = a.FileName,
                        })
                        .OrderBy(a => a.FileName)
                        .ToList(),
                })
                .ToListAsync();

            return Ok(response);
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpGet("get-doctor-verification-request")]
    public async Task<
        ActionResult<GetDoctorVerificationRequestResponseDto>
    > GetDoctorVerificationRequest([FromQuery] GetDoctorVerificationRequestRequestDto request)
    {
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        try
        {
            var verificationRequest = await _dbContext
                .DoctorsVerificationRequests.Include(d => d.Attachments)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId && d.Id == request.RequestId);

            if (verificationRequest is null)
                return BadRequest(
                    new { Errors = new List<string> { "Verification request not found." } }
                );
            var response = new GetDoctorVerificationRequestResponseDto
            {
                Id = verificationRequest.Id,
                CreatedAtUtc = verificationRequest.RequestedAtUtc,
                Status = verificationRequest.Status,
                Title = verificationRequest.Title,
                Description = verificationRequest.Description,
                DeletedAtUtc = verificationRequest.DeletedAtUtc,
                ReviewedAtUtc = verificationRequest.ReviewedAtUtc,
                RejectionNote = verificationRequest.RejectionNote,
                Attachments = verificationRequest
                    .Attachments.Select(a => new DoctorVerificationRequestAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                    })
                    .OrderBy(a => a.FileName)
                    .ToList(),
            };
            return Ok(response);
        }
        catch
        {
            return BadRequest(
                new { Errors = new List<string> { "Error getting verification request." } }
            );
        }
    }

    [HttpPost("send-doctor-verification-request")]
    [RequestSizeLimit(20_000_000)] // Slightly above 15MB to account for metadata overhead
    public async Task<IActionResult> SendVerificationRequest(
        [FromForm] SendVerificationRequestRequestDto request
    )
    {
        var nowUtc = DateTime.UtcNow;
        var result = await _sendVerificationRequestDtoValidator.ValidateAsync(request);

        if (!result.IsValid)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.ErrorMessage) });
        }

        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        // Prevent multiple pending requests
        var pendingExists = await _dbContext.DoctorsVerificationRequests.AnyAsync(r =>
            r.DoctorId == doctorId
            && (r.Status == RequestStatus.Pending || r.Status == RequestStatus.Approved)
        );

        if (pendingExists)
            return BadRequest(
                new
                {
                    Errors = new List<string>
                    {
                        "You already have a pending verification request.",
                    },
                }
            );
        Console.WriteLine($"doctorId: {request.Description}");
        var verificationRequest = new DoctorVerificationRequest
        {
            DoctorId = doctorId,
            Title = request.Title,
            Description = request.Description,
            Status = RequestStatus.Pending,
            RequestedAtUtc = nowUtc,
            SessionPrice = request.SessionPrice,
        };

        try
        {
            await _dbContext.DoctorsVerificationRequests.AddAsync(verificationRequest);
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(
                new
                {
                    Errors = new List<string>
                    {
                        "An error occurred while saving the verification request.",
                    },
                }
            );
        }
        using (var readLock = await _lockingManager.EnterReadAsync(verificationRequest.Id))
        {
            try
            {
                var savedAttachments = new List<DoctorVerificationRequestAttachment>();
                var uploadsFolder = Path.Combine(
                    "Uploads",
                    "DoctorsVerificationRequests",
                    doctorId.ToString(),
                    verificationRequest.Id.ToString()
                );
                Directory.CreateDirectory(uploadsFolder);
                foreach (var file in request.Attachments)
                {
                    var attachmentId = Guid.NewGuid();
                    var uniqueFileName = $"{attachmentId}_{Path.GetFileName(file.FileName)}";
                    var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

                    Console.WriteLine($"file name: {file.FileName}");
                    Console.WriteLine($"file size: {file.Length} bytes");

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    savedAttachments.Add(
                        new DoctorVerificationRequestAttachment
                        {
                            Id = attachmentId,
                            DoctorVerificationRequestId = verificationRequest.Id,
                            UploadedAtUtc = nowUtc,
                            FileName = file.FileName,
                            FilePath = fullPath,
                            ContentType = file.ContentType,
                            FileSize = file.Length,
                        }
                    );
                }

                await _dbContext.DoctorsVerificationRequestsAttachments.AddRangeAsync(
                    savedAttachments
                );
                await _dbContext.SaveChangesAsync();
                return Ok(
                    new SendVerificationRequestResponseDto
                    {
                        VerificationRequestId = verificationRequest.Id,
                    }
                );
            }
            catch
            {
                try
                {
                    _dbContext.DoctorsVerificationRequests.Remove(verificationRequest);

                    await _dbContext.SaveChangesAsync();
                    //clean up the uploads and in db
                    var uploadsFolder = Path.Combine(
                        "Uploads",
                        "DoctorsVerificationRequests",
                        doctorId.ToString(),
                        verificationRequest.Id.ToString()
                    );
                    Directory.Delete(uploadsFolder, true);
                }
                catch (System.Exception)
                {
                    return BadRequest(
                        new
                        {
                            Errors = new List<string>
                            {
                                "An error occurred while saving the attachments.",
                            },
                        }
                    );
                }
                return BadRequest(
                    new
                    {
                        Errors = new List<string>
                        {
                            "An error occurred while saving the attachments.",
                        },
                    }
                );
            }
        }
    }

    [HttpDelete("delete-doctor-verification-request")]
    public async Task<ActionResult<DoctorProfileResponseDto>> DeleteDoctorVerificationRequest(
        [FromQuery] DeleteDoctorVerificationRequestRequestDto request
    )
    {
        var doctorId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

        try
        {
            var doctorVerificationRequest = await _dbContext
                .DoctorsVerificationRequests.Include(d => d.Attachments)
                .FirstOrDefaultAsync(d =>
                    d.DoctorId == doctorId
                    && d.Status == RequestStatus.Pending
                    && d.Id == request.RequestId
                    && d.DeletedAtUtc == null
                    && d.RequestedAtUtc.AddHours(1) > DateTime.UtcNow
                );

            if (doctorVerificationRequest is null)
                return BadRequest("Doctor verification request not found.");

            doctorVerificationRequest.DeletedAtUtc = DateTime.UtcNow;
            doctorVerificationRequest.Status = RequestStatus.Deleted;
            await _dbContext.SaveChangesAsync();

            #region  remove attachment from disk

            var uploadsFolder = Path.Combine(
                "Uploads",
                "DoctorsVerificationRequests",
                doctorId.ToString(),
                doctorVerificationRequest.Id.ToString()
            );

            if (Directory.Exists(uploadsFolder))
            {
                Directory.Delete(uploadsFolder, recursive: true);
            }

            #endregion

            return NoContent();
        }
        catch
        {
            return BadRequest(
                new
                {
                    Errors = new List<string> { "An error occurred while deleting the request." },
                }
            );
        }
    }
}
