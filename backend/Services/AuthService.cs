using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Responses;
using PlayLeague.Api.Requests;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Services;

public class AuthService(AppDbContext db, ITokenService tokens) : IAuthService
{
    public async Task RegisterAsync(RegisterRequest request)
    {
        var exists = await db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            throw new ConflictException("Email already in use.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.Approved)
            throw new ForbiddenException("Account is pending approval.");

        var token = tokens.Generate(user);
        return new AuthResponse(token, new UserResponse(user.Id, user.Email, user.Name));
    }
}
