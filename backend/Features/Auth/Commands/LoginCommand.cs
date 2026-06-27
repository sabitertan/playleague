using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Responses;
using PlayLeague.Api.Requests;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Services;

namespace PlayLeague.Api.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;

public class LoginCommandHandler(AppDbContext db, ITokenService tokens) : IRequestHandler<LoginCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == cmd.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(cmd.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.Approved)
            throw new ForbiddenException("Account is pending approval.");

        return new AuthResponse(tokens.Generate(user), new UserResponse(user.Id, user.Email, user.Name));
    }
}
