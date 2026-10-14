using HidroSense.Api.Data;
using HidroSense.Api.Middleware;
using HidroSense.Api.Security;
using HidroSense.Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HidroSense.Api.Services;

public interface IAuthService
{
    Task<TokenViewModel> LoginAsync(LoginViewModel input, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokens;

    public AuthService(AppDbContext db, ITokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    public async Task<TokenViewModel> LoginAsync(LoginViewModel input, CancellationToken ct = default)
    {
        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == input.Username, ct);

        if (user is null || !PasswordHasher.Verify(input.Password, user.PasswordHash))
            throw new DomainException("Invalid username or password.", StatusCodes.Status401Unauthorized);

        var (token, expiresIn) = _tokens.CreateToken(user);
        return new TokenViewModel
        {
            AccessToken = token,
            ExpiresInSeconds = expiresIn,
            Role = user.Role
        };
    }
}
