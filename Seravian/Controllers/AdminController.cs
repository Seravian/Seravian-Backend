using System.IO.Compression;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Seravian.DTOs.Admin;

[Route("[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AdminController> _logger;
    private readonly DoctorsVerificationRequestsAttachmentFilesAccessLockingManager _lockingManager;
    private readonly IConfiguration _config;

    public AdminController(
        ApplicationDbContext dbContext,
        ILogger<AdminController> logger,
        DoctorsVerificationRequestsAttachmentFilesAccessLockingManager lockingManager,
        IConfiguration config
    )
    {
        _dbContext = dbContext;
        _logger = logger;
        _lockingManager = lockingManager;
        _config = config;
    }

    [HttpGet("get-doctors-verification-requests")]
    public async Task<
        ActionResult<List<GetDoctorVerificationRequestResponseDto>>
    > GetDoctorsVerificationRequests([FromQuery] GetDoctorVerificationRequestsRequestDto request)
    {
        try
        {
            var verificationRequests = _dbContext
                .DoctorsVerificationRequests.AsNoTracking()
                .AsSplitQuery()
                .Include(d => d.Doctor)
                .ThenInclude(d => d.User)
                .Include(d => d.Attachments)
                .Where(d => d.Status != RequestStatus.Deleted);

            if (request.StatusFilter is not null)
            {
                verificationRequests = verificationRequests.Where(d =>
                    d.Status == request.StatusFilter
                );
            }

            var response = await verificationRequests
                .Select(x => new GetDoctorVerificationRequestResponseDto
                {
                    Id = x.Id,
                    DoctorId = x.DoctorId,
                    DoctorFullName = x.Doctor.User.FullName,
                    DoctorEmail = x.Doctor.User.Email,
                    DateOfBirth = x.Doctor.User.DateOfBirth!.Value,
                    DoctorGender = x.Doctor.User.Gender!.Value,
                    RequestedAtUtc = x.RequestedAtUtc,
                    Status = x.Status,
                    Title = x.Title,
                    Description = x.Description,
                    DeletedAtUtc = x.DeletedAtUtc,
                    ReviewedAtUtc = x.ReviewedAtUtc,
                    DoctorImageUrl =
                        x.Doctor.ProfileImagePath != null
                            ? _config["BaseUrl"] + x.Doctor.ProfileImagePath
                            : null,
                    Attachments = x
                        .Attachments.Select(a => new DoctorVerificationRequestAttachmentDto
                        {
                            Id = a.Id,
                            FileName = a.FileName,
                            SizeInBytes = a.FileSize,
                        })
                        .OrderBy(a => a.FileName)
                        .ToList(),
                    RejectionNotes = x.RejectionNote,
                    ReviewerId = x.ReviewerId,
                })
                .OrderByDescending(x => x.RequestedAtUtc)
                .ToListAsync();

            return Ok(response);
        }
        catch
        {
            return BadRequest(
                new { Errors = new List<string> { "Failed to get doctor verification requests." } }
            );
        }
    }

    [HttpGet("get-doctor-verification-request")]
    public async Task<
        ActionResult<GetDoctorVerificationRequestResponseDto>
    > GetDoctorVerificationRequest([FromQuery] GetDoctorVerificationRequestRequestDto request)
    {
        try
        {
            var verificationRequest = await _dbContext
                .DoctorsVerificationRequests.AsNoTracking()
                .AsSplitQuery()
                .Include(d => d.Doctor)
                .ThenInclude(d => d.User)
                .Include(d => d.Attachments)
                .FirstOrDefaultAsync(d =>
                    d.Id == request.RequestId && d.Status != RequestStatus.Deleted
                );

            if (verificationRequest is null)
                return BadRequest(
                    new { Errors = new List<string> { "Verification request not found." } }
                );

            var response = new GetDoctorVerificationRequestResponseDto
            {
                Id = verificationRequest.Id,
                DoctorId = verificationRequest.DoctorId,
                DoctorFullName = verificationRequest.Doctor.User.FullName!,
                DoctorEmail = verificationRequest.Doctor.User.Email,
                DateOfBirth = verificationRequest.Doctor.User.DateOfBirth!.Value,
                DoctorGender = verificationRequest.Doctor.User.Gender!.Value,
                RequestedAtUtc = verificationRequest.RequestedAtUtc,
                Status = verificationRequest.Status,
                Title = verificationRequest.Title,
                Description = verificationRequest.Description,
                DeletedAtUtc = verificationRequest.DeletedAtUtc,
                ReviewedAtUtc = verificationRequest.ReviewedAtUtc,
                DoctorImageUrl =
                    verificationRequest.Doctor.ProfileImagePath != null
                        ? _config["BaseUrl"] + verificationRequest.Doctor.ProfileImagePath
                        : null,
                Attachments = verificationRequest
                    .Attachments.Select(a => new DoctorVerificationRequestAttachmentDto
                    {
                        Id = a.Id,
                        FileName = a.FileName,
                        SizeInBytes = a.FileSize,
                    })
                    .OrderBy(a => a.FileName)
                    .ToList(),
                RejectionNotes = verificationRequest.RejectionNote,
                ReviewerId = verificationRequest.ReviewerId,
            };

            return Ok(response);
        }
        catch
        {
            return BadRequest(
                new { Errors = new List<string> { "Failed to get doctor verification request." } }
            );
        }
    }

    [HttpPut("review-doctor-verification-request")]
    public async Task<ActionResult> ReviewDoctorVerificationRequest(
        ReviewDoctorVerificationRequestRequestDto request
    )
    { // rejection notes is only provided when rejected
        if (request.IsApproved && !string.IsNullOrEmpty(request.RejectionNotes))
        {
            return BadRequest(
                new
                {
                    Errors = new List<string> { "Rejection notes is only provided when rejected." },
                }
            );
        }
        try
        {
            var verificationRequest = await _dbContext
                .DoctorsVerificationRequests.Include(r => r.Doctor)
                .FirstOrDefaultAsync(d => d.Id == request.RequestId);

            if (verificationRequest is null)
                return BadRequest(
                    new { Errors = new List<string> { "Verification request not found." } }
                );
            if (verificationRequest.Doctor.VerifiedAtUtc is not null)
            {
                return BadRequest(
                    new { Errors = new List<string> { "Doctor is already verified." } }
                );
            }

            if (verificationRequest.Status != RequestStatus.Pending)
                return BadRequest(
                    new { Errors = new List<string> { "Verification request is not pending." } }
                );

            if (!(verificationRequest.RequestedAtUtc.AddHours(1) < DateTime.UtcNow))
                return Conflict(
                    new { Errors = new List<string> { "Wait for 1 hour before review." } }
                );

            verificationRequest.ReviewedAtUtc = DateTime.UtcNow;
            verificationRequest.ReviewerId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            );

            if (request.IsApproved)
            {
                verificationRequest.Status = RequestStatus.Approved;
                verificationRequest.Doctor.VerifiedAtUtc = DateTime.UtcNow;
                verificationRequest.Doctor.Description = verificationRequest.Description;
                verificationRequest.Doctor.Title = verificationRequest.Title;
                verificationRequest.Doctor.SessionPrice = verificationRequest.SessionPrice;
            }
            else
            {
                verificationRequest.Status = RequestStatus.Rejected;
                verificationRequest.RejectionNote = request.RejectionNotes;
            }
            await _dbContext.SaveChangesAsync();

            // Delete Request files

            if (!request.IsApproved)
            {
                var uploadsFolder = Path.Combine(
                    "Uploads",
                    "DoctorsVerificationRequests",
                    verificationRequest.DoctorId.ToString(),
                    verificationRequest.Id.ToString()
                );

                if (Directory.Exists(uploadsFolder))
                {
                    try
                    {
                        using (
                            var readLock = await _lockingManager.EnterWriteAsync(
                                verificationRequest.Id
                            )
                        )
                        {
                            Directory.Delete(uploadsFolder, recursive: true);
                        }
                    }
                    catch
                    {
                        return BadRequest(
                            new { Errors = new List<string> { "Failed to delete request files." } }
                        );
                    }
                }
            }

            return NoContent();
        }
        catch
        {
            return BadRequest(
                new
                {
                    Errors = new List<string> { "Failed to review doctor verification request." },
                }
            );
        }
    }

    [HttpGet("get-doctor-info")]
    public async Task<ActionResult<GetDoctorVerificationRequestResponseDto>> GetDoctorInfo(
        [FromQuery] GetDoctorInfoRequestDto request
    )
    {
        if (request.DoctorId == null || request.DoctorId == Guid.Empty)
        {
            return BadRequest(new { Errors = new List<string> { "Request ID is required." } });
        }

        var doctor = await _dbContext
            .Doctors.AsNoTracking()
            .AsSplitQuery()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == request.DoctorId);
        if (doctor is null)
            return BadRequest(new { Errors = new List<string> { "Doctor not found." } });

        var response = new GetDoctorInfoResponseDto
        {
            Id = doctor.UserId,
            Title = doctor.Title,
            Description = doctor.Description,
            DoctorFullName = doctor.User.FullName,
            DoctorEmail = doctor.User.Email,
            DateOfBirth = doctor.User.DateOfBirth!.Value,
            DoctorGender = doctor.User.Gender!.Value,
            VerifiedAtUtc = doctor.VerifiedAtUtc,
            SessionPrice = doctor.SessionPrice,
            ProfileImageUrl = doctor.ProfileImagePath is not null
                ? _config["BaseUrl"] + doctor.ProfileImagePath
                : null,
        };

        return Ok(response);
    }

    [HttpGet("download-doctor-verification-attachment")]
    public async Task<IActionResult> DownloadAttachment(
        [FromQuery] DownloadAttachmentRequestDto request
    )
    {
        var attachment = await _dbContext
            .DoctorsVerificationRequestsAttachments.AsNoTracking()
            .Include(a => a.DoctorVerificationRequest)
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId);

        if (attachment is null)
            return NotFound("Attachment not found.");
        if (
            !(
                attachment.DoctorVerificationRequest.Status == RequestStatus.Pending
                || attachment.DoctorVerificationRequest.Status == RequestStatus.Approved
            )
        )
        {
            return BadRequest(new { Errors = new List<string> { "file is deleted" } });
        }

        using (
            var readLock = await _lockingManager.EnterReadAsync(
                attachment.DoctorVerificationRequestId
            )
        )
        {
            if (!System.IO.File.Exists(attachment.FilePath))
                return NotFound("File does not exist on server.");
            try
            {
                var stream = System.IO.File.OpenRead(attachment.FilePath);
                return File(stream, "application/octet-stream", attachment.FileName);
            }
            catch
            {
                return BadRequest(new { Errors = new List<string> { "Failed to download file." } });
            }
        }
    }

    [HttpGet("download-all-doctor-verification-attachments")]
    public async Task<IActionResult> DownloadAllAttachments(
        [FromQuery] DownloadAllAttachmentsRequestDto request
    )
    {
        var verificationRequest = await _dbContext
            .DoctorsVerificationRequests.AsNoTracking()
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId);

        if (verificationRequest is null)
            return NotFound("Verification request not found.");
        if (
            !(
                verificationRequest.Status == RequestStatus.Pending
                || verificationRequest.Status == RequestStatus.Approved
            )
        )
        {
            return BadRequest(new { Errors = new List<string> { "files are deleted" } });
        }

        var attachments = verificationRequest.Attachments;

        if (!attachments.Any())
            return NotFound("No attachments found for this request.");

        using (var readLock = await _lockingManager.EnterReadAsync(verificationRequest.Id))
        {
            var zipStream = new MemoryStream();
            try
            {
                using (
                    var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true)
                )
                {
                    foreach (var attachment in attachments)
                    {
                        if (!System.IO.File.Exists(attachment.FilePath))
                            return NotFound(
                                $"File '{attachment.FileName}' does not exist on server."
                            );

                        var fileBytes = await System.IO.File.ReadAllBytesAsync(attachment.FilePath);
                        var entry = archive.CreateEntry(
                            attachment.FileName,
                            CompressionLevel.Fastest
                        );
                        await using var entryStream = entry.Open();
                        await entryStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                    }
                }

                zipStream.Position = 0;

                return File(
                    zipStream,
                    "application/zip",
                    $"VerificationFiles_{request.RequestId}.zip"
                );
            }
            catch
            {
                return BadRequest(
                    new { Errors = new List<string> { "Failed to download attachments." } }
                );
            }
        }
    }
}
