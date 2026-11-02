using System;

public static class DateUtils
{
    private static readonly string[] Months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
    public static string GetMonthAbbreviation(int Month) =>
        Month >= 1 && Month <= Months.Length ? Months[Month - 1] : "";
    public static DayOfWeek GetDayOfWeek(DateTime dateTime) => dateTime.DayOfWeek;
    public static DayOfWeek GetDayOfWeek(int year, int month, int day) => new DateTime(year, month, day).DayOfWeek;
    public static string GetMonthAbbreviation(string Month) =>
        int.TryParse(Month, out var month) ? GetMonthAbbreviation(month) : "";
}
