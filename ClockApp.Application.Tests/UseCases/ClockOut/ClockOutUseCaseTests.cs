using ClockApp.Application.Interfaces;
using ClockApp.Application.UseCases.ClockOut;
using ClockApp.Application.UseCases.Location;
using ClockApp.Application.UseCases.Sync;
using ClockApp.Domain.Aggregates;
using ClockApp.Domain.Entities;
using ClockApp.Domain.Enums;
using ClockApp.Domain.Repositories;
using ClockApp.Domain.ValueObjects;
using NSubstitute;

namespace ClockApp.Application.Tests.UseCases.ClockOut;

public class ClockOutUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhileOnBreak_EndsBreakThenChecksOutAndClearsPresence()
    {
        const int organisationId = 1;
        var timesheet = new Timesheet();
        var checkInAt = new DateTime(2025, 6, 18, 9, 0, 0, DateTimeKind.Utc);
        var breakStartAt = new DateTime(2025, 6, 18, 12, 0, 0, DateTimeKind.Utc);

        timesheet.CheckIn(checkInAt, isOffline: false);
        timesheet.StartBreak(breakStartAt, isOffline: false);

        var timesheetRepository = Substitute.For<ITimesheetRepository>();
        timesheetRepository.GetCurrentAsync(Arg.Any<CancellationToken>()).Returns(timesheet);

        var savedEntries = new List<TimeEntry>();
        var timeEntryRepository = Substitute.For<ITimeEntryRepository>();
        await timeEntryRepository.SaveEntryAsync(Arg.Do<TimeEntry>(savedEntries.Add), Arg.Any<CancellationToken>());

        var connectivity = Substitute.For<IConnectivityService>();
        connectivity.IsOnline.Returns(true);

        var hourRepository = Substitute.For<IHourRepository>();
        hourRepository.SaveAsync(Arg.Any<HourRecord>(), Arg.Any<CancellationToken>()).Returns(true);

        var syncHourSession = new SyncHourSessionUseCase(timeEntryRepository, hourRepository);

        var qrCodeRepository = Substitute.For<IQrCodeRepository>();
        qrCodeRepository.GetMode(organisationId).Returns(CheckInMode.Self);

        var userContext = Substitute.For<IUserContext>();
        userContext.Current.Returns(new UserInfo { UserId = 10, OrganisationId = organisationId, DisplayName = "Test User" });

        var presence = Substitute.For<IClockedInPresenceService>();
        var checkWorkLocation = new CheckWorkLocationUseCase(Substitute.For<IGeolocationService>(), Substitute.For<IWorkLocationRepository>(), qrCodeRepository, userContext);

        var sut = new ClockOutUseCase(timesheetRepository, timeEntryRepository, connectivity, syncHourSession, qrCodeRepository, userContext, presence, checkWorkLocation);

        var result = await sut.ExecuteAsync(new ClockOutRequest());

        Assert.Equal(ClockOutStatus.Synced, result.Status);
        Assert.False(timesheet.IsSessionActive());
        Assert.Equal(2, savedEntries.Count);
        Assert.Equal(TimeEntryType.BreakEnd, savedEntries[0].Type);
        Assert.Equal(TimeEntryType.CheckOut, savedEntries[1].Type);
        await presence.Received(1).RegisterCheckOutAsync(Arg.Any<CancellationToken>());
    }
}
