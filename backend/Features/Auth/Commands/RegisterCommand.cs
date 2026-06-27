using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Auth.Commands;

public record RegisterCommand(string Email, string Password, string? Name) : IRequest;

public class RegisterCommandHandler(AppDbContext db) : IRequestHandler<RegisterCommand>
{
    public async Task Handle(RegisterCommand cmd, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Email == cmd.Email, ct))
            throw new ConflictException("Email already in use.");

        db.Users.Add(new User
        {
            Email = cmd.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(cmd.Password),
            Name = cmd.Name,
        });

        await db.SaveChangesAsync(ct);
    }
}
