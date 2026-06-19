using ClockApp.Application.Interfaces;
using ClockApp.Application.Models;
using ClockApp.Application.UseCases.ClockIn;
using ClockApp.Application.UseCases.Location;
using ClockApp.Domain.Entities;
using ClockApp.Domain.Enums;
using ClockApp.Domain.Repositories;
using NSubstitute;

namespace ClockApp.Application.Tests.UseCases.ClockIn;

public class ClockInUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_GpsModeNotAtLocation_DoesNotPersistCheckInOrPresence()
    {
        const int organisationId = 1;

        var timesheetRepository = Substitute.For<ITimesheetRepository>();
        timesheetRepository.GetCurrentAsync(Arg.Any<CancellationToken>()).Returns((Domain.Aggregates.Timesheet?)null);

        var timeEntryRepository = Substitute.For<ITimeEntryRepository>();
        var connectivity = Substitute.For<IConnectivityService>();
        connectivity.IsOnline.Returns(true);

        var qrCodeRepository = Substitute.For<IQrCodeRepository>();
        qrCodeRepository.GetMode(organisationId).Returns(CheckInMode.Gps);
        qrCodeRepository.GetLocationId(organisationId).Returns("eindhoven");

        var userContext = Substitute.For<IUserContext>();
        userContext.Current.Returns(new UserInfo { UserId = 10, OrganisationId = organisationId, DisplayName = "Test User" });

        var presence = Substitute.For<IClockedInPresenceService>();

        // Amsterdam
        var geolocation = Substitute.For<IGeolocationService>();
        geolocation.GetCurrentPositionAsync(Arg.Any<CancellationToken>()).Returns(new GeolocationReading { Latitude = 52.37, Longitude = 4.89 });

        var workLocationRepository = Substitute.For<IWorkLocationRepository>();
        workLocationRepository.GetById("eindhoven").Returns(new WorkLocation
        {
            Id = "eindhoven",
            Name = "Eindhoven",
            Latitude = 51.4419,
            Longitude = 5.4728,
            RadiusMeters = 150
        });

        var checkWorkLocation = new CheckWorkLocationUseCase(geolocation, workLocationRepository, qrCodeRepository, userContext);
        var sut = new ClockInUseCase(timesheetRepository, timeEntryRepository, connectivity, qrCodeRepository, userContext, presence, checkWorkLocation);

        var result = await sut.ExecuteAsync(new ClockInRequest());

        Assert.Equal(ClockInStatus.Failed, result.Status);
        Assert.Contains("not on site", result.Message!, StringComparison.OrdinalIgnoreCase);
        await timeEntryRepository.DidNotReceive().SaveEntryAsync(Arg.Any<TimeEntry>(), Arg.Any<CancellationToken>());
        await presence.DidNotReceive().RegisterCheckInAsync(Arg.Any<DateTime>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
