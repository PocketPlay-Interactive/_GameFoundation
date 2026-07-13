using System;
using System.Collections;
using UnityEngine;

public static class UtilsTimerHelper
{
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1);

    public static int CurrentTimeInSeconds() => (int)(DateTime.Now - Epoch).TotalSeconds;
    public static int CurrentTimeInSecondsUtc() => (int)(DateTime.UtcNow - Epoch).TotalSeconds;

    public static int TimeInSecondsAfterMinutes(int minutes)
        => (int)(DateTime.Now.AddMinutes(minutes) - Epoch).TotalSeconds;

    public static int EndOfDayInSeconds()
        => (int)(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59) - Epoch).TotalSeconds;

    public static int CurrentDaySinceEpoch() => (int)(DateTime.Now - Epoch).TotalDays;

    public static int CurrentDayAddDays(int days)
    {
        var today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        return (int)(today.AddDays(days) - Epoch).TotalDays;
    }

    public static int CurrentSecondInDay()
        => DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second;

    public static int SecondsToTarget(DateTime target)
        => (int)(target - DateTime.Now).TotalSeconds;

    public static int SecondsToTargetUtc(DateTime target)
        => (int)(target - DateTime.UtcNow).TotalSeconds;

    public static DateTime ConvertSecondsToDateTime(int seconds)
        => Epoch.AddSeconds(seconds);

    public static TimeSpan ConvertSecondsToTimeSpan(int seconds)
        => TimeSpan.FromSeconds(seconds);

    public static string FormatCompactDuration(TimeSpan remaining)
    {
        if (remaining.TotalDays >= 1d)
        {
            return $"{(int)remaining.TotalDays}d {remaining.Hours}h";
        }

        if (remaining.TotalHours >= 1d)
        {
            return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
        }

        return $"{remaining.Minutes}m {remaining.Seconds}s";
    }

    public static TimeSpan GetTimeUntilWeekdayEnd(
        DateTime trustedToday,
        TimeSpan currentTimeOfDay,
        DayOfWeek endDay = DayOfWeek.Sunday)
    {
        DateTime now = trustedToday.Date + currentTimeOfDay;
        int daysUntilEndDay = ((int)endDay - (int)trustedToday.DayOfWeek + 7) % 7;
        DateTime end = trustedToday.Date.AddDays(daysUntilEndDay + 1);
        return end > now ? end - now : TimeSpan.Zero;
    }

    public static string ToMinuteSecondString(int totalSeconds)
        => $"{totalSeconds / 60}m {totalSeconds % 60}s";

    public static string ToTimeString(int totalSeconds)
        => $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";

    public static string ToHourMinuteSecondString(int totalSeconds)
    {
        int h = totalSeconds / 3600;
        int m = (totalSeconds % 3600) / 60;
        int s = totalSeconds % 60;
        return $"{h:00}:{m:00}:{s:00}";
    }
}
