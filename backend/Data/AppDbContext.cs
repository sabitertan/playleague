using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Data;

// Ensures every DateTime is stored as UTC, so writes to 'timestamp with time zone' never fail
// for Unspecified/Local kinds. Columns stay as timestamptz.
file sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : DateTime.SpecifyKind(v, DateTimeKind.Utc),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    { }
}

file sealed class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter() : base(
        v => v.HasValue ? (v.Value.Kind == DateTimeKind.Local ? v.Value.ToUniversalTime() : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
    { }
}

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Coach> Coaches => Set<Coach>();
    public DbSet<CoachAssignment> CoachAssignments => Set<CoachAssignment>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Rsvp> Rsvps => Set<Rsvp>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<Division> Divisions => Set<Division>();
    public DbSet<LeagueUser> LeagueUsers => Set<LeagueUser>();
    public DbSet<PlayerTransfer> PlayerTransfers => Set<PlayerTransfer>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<IceSurface> IceSurfaces => Set<IceSurface>();
    public DbSet<GameSchedule> GameSchedules => Set<GameSchedule>();
    public DbSet<ScheduleGame> ScheduleGames => Set<ScheduleGame>();
    public DbSet<LeagueMessage> LeagueMessages => Set<LeagueMessage>();
    public DbSet<MessageTargeting> MessageTargetings => Set<MessageTargeting>();
    public DbSet<MessageRecipient> MessageRecipients => Set<MessageRecipient>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<PracticeSession> PracticeSessions => Set<PracticeSession>();
    public DbSet<Play> Plays => Set<Play>();
    public DbSet<PracticeSessionPlay> PracticeSessionPlays => Set<PracticeSessionPlay>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        builder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder m)
    {
        // Store all enums as strings
        m.Entity<TeamMember>().Property(e => e.Role).HasConversion<string>();
        m.Entity<LeagueUser>().Property(e => e.Role).HasConversion<string>();
        m.Entity<Event>().Property(e => e.Type).HasConversion<string>();
        m.Entity<Invitation>().Property(e => e.Status).HasConversion<string>();
        m.Entity<GameSchedule>().Property(e => e.Status).HasConversion<string>();
        m.Entity<Venue>().Property(e => e.Visibility).HasConversion<string>();
        m.Entity<Venue>().Property(e => e.ProfileStatus).HasConversion<string>();
        m.Entity<AuditLog>().Property(e => e.Severity).HasConversion<string>();
        m.Entity<LeagueMessage>().Property(e => e.MessageType).HasConversion<string>();
        m.Entity<LeagueMessage>().Property(e => e.Priority).HasConversion<string>();
        m.Entity<MessageRecipient>().Property(e => e.DeliveryStatus).HasConversion<string>();

        m.Entity<User>().HasIndex(u => u.Email).IsUnique();
        m.Entity<Invitation>().HasIndex(i => i.Token).IsUnique();
        m.Entity<Venue>().HasIndex(v => v.Slug).IsUnique();

        // Composite keys
        m.Entity<TeamMember>().HasKey(tm => new { tm.UserId, tm.TeamId });
        m.Entity<LeagueUser>().HasKey(lu => new { lu.UserId, lu.LeagueId });
        m.Entity<Rsvp>().HasKey(r => new { r.UserId, r.EventId });

        // TeamMember
        m.Entity<TeamMember>()
            .HasOne(tm => tm.User).WithMany(u => u.TeamMembers).OnDelete(DeleteBehavior.Cascade);
        m.Entity<TeamMember>()
            .HasOne(tm => tm.Team).WithMany(t => t.TeamMembers).OnDelete(DeleteBehavior.Cascade);

        // LeagueUser
        m.Entity<LeagueUser>()
            .HasOne(lu => lu.User).WithMany(u => u.LeagueUsers).OnDelete(DeleteBehavior.Cascade);
        m.Entity<LeagueUser>()
            .HasOne(lu => lu.League).WithMany(l => l.LeagueUsers).OnDelete(DeleteBehavior.Cascade);

        // RSVP
        m.Entity<Rsvp>()
            .HasOne(r => r.Event).WithMany(e => e.Rsvps).OnDelete(DeleteBehavior.Cascade);

        // Event — home/away teams avoid cascade cycles
        m.Entity<Event>()
            .HasOne(e => e.HomeTeam).WithMany().HasForeignKey(e => e.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);
        m.Entity<Event>()
            .HasOne(e => e.AwayTeam).WithMany().HasForeignKey(e => e.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // MessageTargeting 1:1
        m.Entity<MessageTargeting>()
            .HasOne(mt => mt.Message).WithOne(msg => msg.Targeting)
            .HasForeignKey<MessageTargeting>(mt => mt.MessageId);

        // Coach — owned by a user; assignable to many teams
        m.Entity<Coach>()
            .HasOne(c => c.CreatedBy).WithMany().HasForeignKey(c => c.CreatedById)
            .OnDelete(DeleteBehavior.Cascade);

        m.Entity<CoachAssignment>().HasKey(ca => new { ca.CoachId, ca.TeamId });
        m.Entity<CoachAssignment>()
            .HasOne(ca => ca.Coach).WithMany(c => c.Assignments).OnDelete(DeleteBehavior.Cascade);
        m.Entity<CoachAssignment>()
            .HasOne(ca => ca.Team).WithMany().HasForeignKey(ca => ca.TeamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
