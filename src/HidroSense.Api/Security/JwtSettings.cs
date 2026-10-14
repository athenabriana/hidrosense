namespace HidroSense.Api.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "HidroSense";
    public string Audience { get; set; } = "HidroSense.Clients";
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
