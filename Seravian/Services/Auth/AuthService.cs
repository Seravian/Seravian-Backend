using Microsoft.EntityFrameworkCore;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    private readonly IPasswordHasher _passwordHasher;
    private readonly IOtpService _otpService;

    public AuthService(
        ApplicationDbContext context,
        ITokenService tokenService,
        IEmailSender emailSender,
        IPasswordHasher passwordHasher,
        IOtpService otpService
    )
    {
        _context = context;
        _tokenService = tokenService;

        _passwordHasher = passwordHasher;
        _otpService = otpService;
    }

    // Register a new user
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto registerDto)
    {
        // Check if the email is already registered
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
        {
            throw new Exception("Email is already taken");
        }

        // Generate and send OTP to the user
        var user = new User
        {
            Email = registerDto.Email,
            CreatedAtUtc = DateTime.UtcNow,
            PasswordHash = _passwordHasher.HashPassword(registerDto.Password), // Temporary, will be hashed later
        };
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        catch
        {
            throw new Exception("Email is already taken");
        }
        // Send OTP and verify the code
        await _otpService.GenerateAndSendOtpAsync(user);
        return new RegisterResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc,
        };
    }

    // Refresh tokens (generate new ones)

    public async Task<bool> VerifyOtpAsync(string userEmail, string otpCode)
    {
        return await _otpService.ValidateOtpAsync(userEmail, otpCode);
    }

    // Revoke the refresh token
    public async Task RevokeRefreshTokenAsync(string refreshToken)
    {
        var token = await _tokenService.GetRefreshTokenAsync(refreshToken);

        if (token == null)
        {
            throw new Exception("Invalid refresh token.");
        }

        // Mark the refresh token as revoked
        await _tokenService.RevokeRefreshTokenAsync(refreshToken);
    }

    public async Task<LoginResponseDto> CompleteProfileSetupAsync(
        Guid userId,
        CompleteProfileSetupRequestDto profileSetupDto
    )
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }
        if (user.IsProfileSetupComplete)
        {
            throw new Exception("Profile setup is already completed.");
        }
        user.FullName = profileSetupDto.FullName;
        user.DateOfBirth = profileSetupDto.DateOfBirth;
        user.Gender = profileSetupDto.Gender;
        user.Role = profileSetupDto.Role;
        user.IsProfileSetupComplete = true;
        await _context.SaveChangesAsync();

        if (user.Role == UserRole.Doctor)
        {
            Doctor doctor = new Doctor { UserId = user.Id, VerifiedAtUtc = null };
            await _context.Doctors.AddAsync(doctor);
        }
        else if (user.Role == UserRole.Patient)
        {
            Patient patient = new Patient { UserId = user.Id };
            await _context.Patients.AddAsync(patient);
        }
        else if (user.Role == UserRole.Admin)
        {
            Admin admin = new Admin { UserId = user.Id };
            await _context.Admins.AddAsync(admin);
        }
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new Exception("The profile setup has been completed by another request.");
        }

        return new LoginResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth ?? default,
            Gender = user.Gender,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified,
            IsProfileSetupComplete = user.IsProfileSetupComplete,
            CreatedAtUtc = user.CreatedAtUtc,
            IsDoctorVerified = user.Role == UserRole.Doctor ? false : null,
            Tokens = await _tokenService.GenerateTokensAsync(user),
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto)
    {
        var passwordHash = _passwordHasher.HashPassword(loginDto.Password);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user is null || !_passwordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
        {
            throw new InvalidCredentialsException();
        }
        if (!user.IsEmailVerified)
        {
            await _otpService.GenerateAndSendOtpAsync(user);
            return new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = null,
                DateOfBirth = null,
                Gender = null,
                FullName = null,
                IsDoctorVerified = null,
                IsEmailVerified = user.IsEmailVerified,
                IsProfileSetupComplete = user.IsProfileSetupComplete,
                CreatedAtUtc = user.CreatedAtUtc,
                Tokens = null,
            };
        }

        if (!user.IsProfileSetupComplete)
        {
            return new LoginResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Role = null,
                DateOfBirth = null,
                Gender = null,
                FullName = null,
                IsDoctorVerified = null,
                IsEmailVerified = user.IsEmailVerified,
                IsProfileSetupComplete = user.IsProfileSetupComplete,
                CreatedAtUtc = user.CreatedAtUtc,
                Tokens = _tokenService.GenerateAccessTokenWithoutRole(user),
            };
        }

        // Generate tokens
        var authTokens = await _tokenService.GenerateTokensAsync(user);
        bool? isDoctorVerified = null;
        Console.WriteLine(isDoctorVerified);
        if (user.Role == UserRole.Doctor)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);

            isDoctorVerified = doctor.VerifiedAtUtc is not null;
        }

        Console.WriteLine(isDoctorVerified);
        // Return the tokens and user information
        return new LoginResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            DateOfBirth = user.DateOfBirth ?? default,
            Gender = user.Gender ?? default,
            Role = user.Role ?? default,
            IsEmailVerified = user.IsEmailVerified,
            IsDoctorVerified = isDoctorVerified,
            IsProfileSetupComplete = user.IsProfileSetupComplete,
            CreatedAtUtc = user.CreatedAtUtc,
            Tokens = authTokens,
        };
    }

    public async Task<AuthTokens> RefreshTokensAsync(string refreshToken)
    {
        RefreshToken? token = await _tokenService.GetRefreshTokenAsync(refreshToken);

        if (token == null || token.IsRevoked)
        {
            throw new Exception("Invalid or revoked refresh token.");
        }
        if (token.ExpiresAtUtc < DateTime.UtcNow)
        {
            throw new Exception("Refresh token has expired.");
        }
        var user = token.User;

        await _tokenService.RevokeRefreshTokenAsync(token.Token);

        return await _tokenService.GenerateTokensAsync(user);
    }

    public async Task ResendOtpAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null)
            throw new Exception("User not found.");
        if (user.IsEmailVerified)
            throw new Exception("Email already verified.");

        await _otpService.GenerateAndSendOtpAsync(user);
    }
}
