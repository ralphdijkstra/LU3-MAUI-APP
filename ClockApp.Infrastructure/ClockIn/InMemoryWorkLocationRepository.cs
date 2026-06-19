using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;

namespace ClockApp.Infrastructure.ClockIn;

public sealed class InMemoryWorkLocationRepository : IWorkLocationRepository
{
    private static readonly WorkLocation[] Locations =
    [
        new()
        {
            Id = "breda-hogeschool",
            Name = "Hogeschoollaan 1, Breda",
            Latitude = 51.5835256,
            Longitude = 4.7986851,
            RadiusMeters = 150
        },
        new()
        {
            Id = "sint-oedenrode",
            Name = "Sint-Oedenrode",
            Latitude = 51.5675,
            Longitude = 5.4597,
            RadiusMeters = 2500 // 2.5 KM
        },
        new()
        {
            Id = "eindhoven",
            Name = "Eindhoven",
            Latitude = 51.4419,
            Longitude = 5.4728,
            RadiusMeters = 2500 // 2.5 KM
        }
    ];

    public IReadOnlyList<WorkLocation> ListForOrganisation(int organisationId) => Locations;

    public WorkLocation? GetById(string locationId) =>
        Locations.FirstOrDefault(l => string.Equals(l.Id, locationId, StringComparison.OrdinalIgnoreCase));
}
