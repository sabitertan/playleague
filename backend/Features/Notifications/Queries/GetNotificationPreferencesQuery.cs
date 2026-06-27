using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Notifications.Queries;

public record NotificationPreferenceDto(
    bool LeagueMessages,
    bool LeagueAnnouncements,
    bool EventNotifications,
    bool RsvpReminders,
    bool TeamInvitations,
    bool PracticePlanNotifications,
    bool EmailEnabled,
    bool UrgentOnly,
    bool BatchDelivery
);

public record GetNotificationPreferencesQuery(Guid UserId, Guid? LeagueId) : IRequest<NotificationPreferenceDto>;

public class GetNotificationPreferencesQueryHandler(AppDbContext db)
    : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferenceDto>
{
    public async Task<NotificationPreferenceDto> Handle(GetNotificationPreferencesQuery query, CancellationToken ct)
    {
        var preference = await db.NotificationPreferences
            .FirstOrDefaultAsync(
                np => np.UserId == query.UserId && np.LeagueId == query.LeagueId,
                ct);

        if (preference is null)
        {
            return new NotificationPreferenceDto(
                LeagueMessages: true,
                LeagueAnnouncements: true,
                EventNotifications: true,
                RsvpReminders: true,
                TeamInvitations: true,
                PracticePlanNotifications: true,
                EmailEnabled: true,
                UrgentOnly: false,
                BatchDelivery: false
            );
        }

        return new NotificationPreferenceDto(
            preference.LeagueMessages,
            preference.LeagueAnnouncements,
            preference.EventNotifications,
            preference.RsvpReminders,
            preference.TeamInvitations,
            preference.PracticePlanNotifications,
            preference.EmailEnabled,
            preference.UrgentOnly,
            preference.BatchDelivery
        );
    }
}
