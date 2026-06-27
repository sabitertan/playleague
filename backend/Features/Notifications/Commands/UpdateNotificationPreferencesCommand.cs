using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Notifications.Commands;

public record UpdateNotificationPreferencesCommand(
    Guid UserId,
    Guid? LeagueId,
    bool LeagueMessages,
    bool LeagueAnnouncements,
    bool EventNotifications,
    bool RsvpReminders,
    bool TeamInvitations,
    bool PracticePlanNotifications,
    bool EmailEnabled,
    bool UrgentOnly,
    bool BatchDelivery
) : IRequest;

public class UpdateNotificationPreferencesCommandHandler(AppDbContext db)
    : IRequestHandler<UpdateNotificationPreferencesCommand>
{
    public async Task Handle(UpdateNotificationPreferencesCommand cmd, CancellationToken ct)
    {
        var preference = await db.NotificationPreferences
            .FirstOrDefaultAsync(
                np => np.UserId == cmd.UserId && np.LeagueId == cmd.LeagueId,
                ct);

        if (preference is null)
        {
            preference = new NotificationPreference
            {
                UserId = cmd.UserId,
                LeagueId = cmd.LeagueId,
                UnsubscribeToken = Guid.NewGuid().ToString("N"),
            };
            db.NotificationPreferences.Add(preference);
        }

        preference.LeagueMessages = cmd.LeagueMessages;
        preference.LeagueAnnouncements = cmd.LeagueAnnouncements;
        preference.EventNotifications = cmd.EventNotifications;
        preference.RsvpReminders = cmd.RsvpReminders;
        preference.TeamInvitations = cmd.TeamInvitations;
        preference.PracticePlanNotifications = cmd.PracticePlanNotifications;
        preference.EmailEnabled = cmd.EmailEnabled;
        preference.UrgentOnly = cmd.UrgentOnly;
        preference.BatchDelivery = cmd.BatchDelivery;
        preference.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
