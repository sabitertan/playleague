using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync(u => u.Email == "admin@test.com"))
        {
            Console.WriteLine("✓ Database already seeded.");
            return;
        }

        var admin = new User
        {
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Name = "Test Admin",
            Approved = true,
        };
        var member = new User
        {
            Email = "member@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("member123"),
            Name = "Test Member",
            Approved = true,
        };
        db.Users.AddRange(admin, member);

        var team = new Team
        {
            Name = "Northside Ice Hawks",
            Sport = "HOCKEY",
            Season = "Winter 2026-27",
        };
        db.Teams.Add(team);

        await db.SaveChangesAsync();

        db.TeamMembers.AddRange(
            new TeamMember { UserId = admin.Id, TeamId = team.Id, Role = TeamRole.ADMIN },
            new TeamMember { UserId = member.Id, TeamId = team.Id, Role = TeamRole.MEMBER }
        );

        db.Players.AddRange(
            new Player { Name = "Connor MacTavish", JerseyNumber = 97, Position = "Center", TeamId = team.Id, UserId = admin.Id },
            new Player { Name = "Jake Sullivan", JerseyNumber = 14, Position = "Left Wing", TeamId = team.Id, UserId = member.Id },
            new Player { Name = "Dylan Bouchard", JerseyNumber = 31, Position = "Goalie", TeamId = team.Id }
        );

        var gameDate = DateTime.UtcNow.AddDays(7);
        var practiceDate = DateTime.UtcNow.AddDays(3);

        db.Events.AddRange(
            new Event { Title = "vs Westside Wolves", Type = EventType.GAME, StartAt = gameDate, Location = "Northside Ice Arena - Rink A", Opponent = "Westside Wolves", TeamId = team.Id },
            new Event { Title = "Stick Handling & Power Play Drills", Type = EventType.PRACTICE, StartAt = practiceDate, Location = "Northside Ice Arena - Rink B", TeamId = team.Id }
        );

        await db.SaveChangesAsync();

        Console.WriteLine("✅ Database seeded.");
        Console.WriteLine("   Admin:  admin@test.com  / admin123");
        Console.WriteLine("   Member: member@test.com / member123");
    }
}
