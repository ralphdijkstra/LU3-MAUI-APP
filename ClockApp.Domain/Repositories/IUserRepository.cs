using ClockApp.Domain.Entities;

namespace ClockApp.Domain.Repositories;

public interface IUserRepository
{
    Task<UserInfo?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
