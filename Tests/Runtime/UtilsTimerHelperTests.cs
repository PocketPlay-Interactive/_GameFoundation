using System;
using NUnit.Framework;

public sealed class UtilsTimerHelperTests
{
    [Test]
    public void FormatsMinuteAndHourValues()
    {
        Assert.That(UtilsTimerHelper.ToMinuteSecondString(125), Is.EqualTo("2m 5s"));
        Assert.That(UtilsTimerHelper.ToTimeString(125), Is.EqualTo("02:05"));
        Assert.That(UtilsTimerHelper.ToHourMinuteSecondString(3723), Is.EqualTo("01:02:03"));
    }

    [Test]
    public void ConvertsEpochSeconds()
    {
        Assert.That(UtilsTimerHelper.ConvertSecondsToDateTime(0), Is.EqualTo(new DateTime(1970, 1, 1)));
        Assert.That(UtilsTimerHelper.ConvertSecondsToTimeSpan(125), Is.EqualTo(TimeSpan.FromSeconds(125)));
    }

    [Test]
    public void FormatsCompactDurationByLargestUnit()
    {
        Assert.That(UtilsTimerHelper.FormatCompactDuration(TimeSpan.FromDays(2) + TimeSpan.FromHours(3)), Is.EqualTo("2d 3h"));
        Assert.That(UtilsTimerHelper.FormatCompactDuration(TimeSpan.FromHours(2) + TimeSpan.FromMinutes(4)), Is.EqualTo("2h 4m"));
        Assert.That(UtilsTimerHelper.FormatCompactDuration(TimeSpan.FromMinutes(4) + TimeSpan.FromSeconds(8)), Is.EqualTo("4m 8s"));
    }
}
