using ClockApp.Domain.Entities;

namespace ClockApp.Domain.Repositories;

public interface IWorkLocationRepository
{
    IReadOnlyList<WorkLocation> ListForOrganisation(int organisationId);

    WorkLocation? GetById(string locationId);
}
