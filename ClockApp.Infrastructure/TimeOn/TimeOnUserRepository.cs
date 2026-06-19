using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;
using ClockApp.Infrastructure.TimeOn.Client;

namespace ClockApp.Infrastructure.TimeOn;

public sealed class TimeOnUserRepository : IUserRepository
{
    private readonly TimeOnUserClient _userClient;

    public TimeOnUserRepository(TimeOnUserClient userClient)
    {
        _userClient = userClient;
    }

    public async Task<UserInfo?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var response = await _userClient.GetUserInfoAsync(cancellationToken);

        if (!response.IsSuccess || response.Data == null || !response.Data.Success)
            return null;

        var dto = response.Data.ResultObject;

        if (dto == null)
            return null;

        return new UserInfo
        {
            UserId = dto.UserId,
            OrganisationId = dto.OrganisationId,
            DisplayName = dto.Name ?? dto.Username ?? string.Empty,
            Email = dto.Email ?? string.Empty,
            IsManager = dto.IsManager || dto.IsSystemAdmin || dto.AllowSettings
        };
    }
}
