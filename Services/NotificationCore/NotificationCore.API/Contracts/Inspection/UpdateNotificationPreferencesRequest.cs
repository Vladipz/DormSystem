using System.Collections.ObjectModel;

using NotificationCore.API.Entities;

namespace NotificationCore.API.Contracts.Inspection
{
    public record UpdateNotificationPreferencesRequest(
        Collection<UpdateTypeSetting> Types,
        Collection<UpdateChannelSetting> Channels);

    public record UpdateTypeSetting(NotificationType Type, bool Enabled);

    public record UpdateChannelSetting(NotificationChannel Channel, bool Enabled);
}
