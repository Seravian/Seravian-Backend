public class JwtSettings
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string AccessTokenKey { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int ProfileSetupAccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}
