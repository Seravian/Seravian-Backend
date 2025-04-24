using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    private readonly IValidator<RegisterRequestDto> _registerRequestDtoValidator;
    private readonly IValidator<LoginRequestDto> _loginRequestDtoValidator;

    private readonly IValidator<CompleteProfileSetupRequestDto> _completeProfileSetupRequestDtoValidator;
    private readonly IValidator<VerifyOtpRequestDto> _verifyOtpRequestDtoValidator;
    private readonly IValidator<ResendOtpRequestDto> _resendOtpRequestDtoValidator;
    private readonly IValidator<RefreshTokenRequestDto> _refreshTokenRequestDtoValidator;

    public AuthController(
        IAuthService authService,
        IValidator<RegisterRequestDto> registerRequestDtoValidator,
        IValidator<LoginRequestDto> loginRequestDtoValidator,
        IValidator<CompleteProfileSetupRequestDto> completeProfileSetupRequestDtoValidator,
        IValidator<VerifyOtpRequestDto> verifyOtpRequestDtoValidator,
        IValidator<ResendOtpRequestDto> resendOtpRequestDtoValidator,
        IValidator<RefreshTokenRequestDto> refreshTokenRequestDtoValidator
    )
    {
        _authService = authService;
        _registerRequestDtoValidator = registerRequestDtoValidator;
        _loginRequestDtoValidator = loginRequestDtoValidator;
        _completeProfileSetupRequestDtoValidator = completeProfileSetupRequestDtoValidator;
        _verifyOtpRequestDtoValidator = verifyOtpRequestDtoValidator;
        _resendOtpRequestDtoValidator = resendOtpRequestDtoValidator;
        _refreshTokenRequestDtoValidator = refreshTokenRequestDtoValidator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request)
    {
        var validationResult = _loginRequestDtoValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { Errors = validationResult.Errors.Select(x => x.ErrorMessage) });
        }

        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Errors = new List<string> { ex.Message } });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDto>> Register(RegisterRequestDto request)
    {
        var validationResult = _registerRequestDtoValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { Errors = validationResult.Errors.Select(x => x.ErrorMessage) });
        }

        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtpAsync(ResendOtpRequestDto request)
    {
        var validationResult = _resendOtpRequestDtoValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { Errors = validationResult.Errors.Select(x => x.ErrorMessage) });
        }
        var userEmail = request.Email;
        try
        {
            await _authService.ResendOtpAsync(userEmail);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Errors = new List<string> { ex.Message } });
        }
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtpAsync(VerifyOtpRequestDto request)
    {
        var validationResult = _verifyOtpRequestDtoValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { Errors = validationResult.Errors.Select(x => x.ErrorMessage) });
        }
        var userEmail = request.Email;
        var otpCode = request.OtpCode;
        bool isValid;
        try
        {
            isValid = await _authService.VerifyOtpAsync(userEmail, otpCode);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Errors = new List<string> { ex.Message } });
        }

        if (isValid)
        {
            return NoContent();
        }
        return BadRequest(new { Errors = new List<string> { "Invalid OTP code." } });
    }

    [HttpPost("complete-profile-setup")]
    [Authorize]
    public async Task<ActionResult<LoginResponseDto>> CompleteProfileSetupAsync(
        CompleteProfileSetupRequestDto request
    )
    { // Retrieve the role claim (if exists)
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "UserRole");

        if (roleClaim is not null)
        {
            return BadRequest(
                new { Errors = new List<string> { "The profile setup is already completed." } }
            );
        }
        var validationResult = _completeProfileSetupRequestDtoValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { Errors = validationResult.Errors.Select(x => x.ErrorMessage) }); // validationResult.Errors.Select validationResult.Errors);
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            var result = await _authService.CompleteProfileSetupAsync(Guid.Parse(userId), request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Errors = new List<string> { ex.Message } });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshTokensAsync(RefreshTokenRequestDto request)
    {
        var validationResult = _refreshTokenRequestDtoValidator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { Errors = validationResult.Errors.Select(x => x.ErrorMessage) });
        }

        var token = await _authService.RefreshTokensAsync(request.RefreshToken);
        return Ok(token);
    }

    [HttpPost("logout")]
    [Authorize(Roles = "Doctor,Patient,Admin")]
    public async Task<IActionResult> LogoutAsync(LogoutRequestDto request)
    {
        try
        {
            await _authService.RevokeRefreshTokenAsync(request.RefreshToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { Errors = new List<string> { ex.Message } });
        }
    }
}
