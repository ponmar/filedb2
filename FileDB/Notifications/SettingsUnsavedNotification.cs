﻿using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class SettingsUnsavedNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message => Strings.SettingsUnsavedSettingsNotification;
    public DateTime DateTime { get; } = DateTime.Now;
}