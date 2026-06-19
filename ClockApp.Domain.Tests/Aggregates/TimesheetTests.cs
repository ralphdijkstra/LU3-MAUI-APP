using ClockApp.Domain.Aggregates;
using ClockApp.Domain.Enums;

namespace ClockApp.Domain.Tests.Aggregates;

public class TimesheetTests
{
    [Fact]
    public void TryBuildHourRecord_AfterWorkSessionWithBreak_SubtractsBreakFromWorkedSeconds()
    {
        var timesheet = new Timesheet();

        var checkInAt = new DateTime(2025, 6, 18, 9, 0, 0, DateTimeKind.Utc);
        var breakStartAt = new DateTime(2025, 6, 18, 12, 0, 0, DateTimeKind.Utc);
        var breakEndAt = new DateTime(2025, 6, 18, 12, 30, 0, DateTimeKind.Utc);
        var checkOutAt = new DateTime(2025, 6, 18, 17, 0, 0, DateTimeKind.Utc);

        timesheet.CheckIn(checkInAt, isOffline: false);
        timesheet.StartBreak(breakStartAt, isOffline: false);
        timesheet.EndBreak(breakEndAt, isOffline: false, minimumBreakDuration: TimeSpan.Zero);

        var checkOut = timesheet.CheckOut(checkOutAt, isOffline: false);

        var success = timesheet.TryBuildHourRecord(checkOut, out var record);

        Assert.True(success);
        Assert.NotNull(record);
        Assert.Equal(27_000, record.Seconds);   // 7,5 uur
        Assert.Equal(1_800, record.BreakSeconds); // 30 min
        Assert.Equal(TimeEntryType.CheckOut, checkOut.Type);
    }
}
