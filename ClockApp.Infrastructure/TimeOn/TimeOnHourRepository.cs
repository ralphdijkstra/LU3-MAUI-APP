using System.Text.Json;
using ClockApp.Domain.Entities;
using ClockApp.Domain.Repositories;
using ClockApp.Domain.ValueObjects;
using ClockApp.Infrastructure.TimeOn.Client;
using ClockApp.Infrastructure.TimeOn.Dtos;

namespace ClockApp.Infrastructure.TimeOn;

public sealed class TimeOnHourRepository : IHourRepository
{
    private readonly TimeOnHourClient _hourClient;

    public TimeOnHourRepository(TimeOnHourClient hourClient)
    {
        _hourClient = hourClient;
    }

    public async Task<bool> SaveAsync(HourRecord hour, CancellationToken cancellationToken = default)
    {
        var dto = new HourSaveDto
        {
            HourId = hour.HourId,
            Date = $"{hour.Date:yyyy-MM-dd}T00:00:00",
            FromSeconds = hour.FromSeconds,
            Seconds = hour.Seconds,
            BreakSeconds = hour.BreakSeconds > 0 ? hour.BreakSeconds : null
        };

        var result = await _hourClient.SaveHourAsync(dto, cancellationToken);

        return result.IsSuccess && result.Data?.Success == true;
    }

    public Task<IReadOnlyList<HourEntry>> ListUserDayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var filter = CreateUserDayFilter(date);

        return ListDayHoursAsync(filter, date, cancellationToken);
    }

    public Task<IReadOnlyList<HourEntry>> ListAllDayAsync(DateOnly date, int organisationId, CancellationToken cancellationToken = default)
    {
        var filter = CreateManagerDayFilter(date, organisationId);

        return ListDayHoursAsync(filter, date, cancellationToken);
    }

    private async Task<IReadOnlyList<HourEntry>> ListDayHoursAsync(HourListFilterDto filter, DateOnly date, CancellationToken cancellationToken)
    {
        var listResult = await _hourClient.ListHoursAsync(new HourListRequestDto { Filter = filter }, cancellationToken);

        if (!listResult.IsSuccess || listResult.Data?.Success != true || listResult.Data.ResultObject.ValueKind == JsonValueKind.Undefined)
            return Array.Empty<HourEntry>();

        var parsed = HourListParser.ParseForDate(listResult.Data.ResultObject, date);

        foreach (var path in parsed.PathsToLoad)
        {
            var groupResult = await _hourClient.LoadHourGroupAsync(new HourGroupRequestDto { Filter = filter, HourGroupKey = path }, cancellationToken);

            if (!groupResult.IsSuccess || groupResult.Data?.Success != true || groupResult.Data.ResultObject.ValueKind == JsonValueKind.Undefined)
                continue;

            foreach (var hour in HourListParser.ParseHourItems(groupResult.Data.ResultObject, date))
                AddUniqueHour(parsed.Hours, hour);
        }

        return parsed.Hours.OrderBy(h => h.FromSeconds).ThenBy(h => h.UserName).ToList();
    }

    private static void AddUniqueHour(List<HourEntry> hours, HourEntry hour)
    {
        if (hours.Any(existing => existing.HourId == hour.HourId))
            return;

        hours.Add(hour);
    }

    private static HourListFilterDto CreateUserDayFilter(DateOnly date)
    {
        var dateValue = FormatDate(date);

        return new HourListFilterDto
        {
            ToStringValue = string.Empty,
            PreventLargeDataset = false,
            GroupString = "week,day",
            Period = "custom",
            From = dateValue,
            To = dateValue,
            Context = "user",
            Type = "report",
            Paged = false,
            Deleted = false
        };
    }

    private static HourListFilterDto CreateManagerDayFilter(DateOnly date, int organisationId)
    {
        var dateValue = FormatDate(date);

        return new HourListFilterDto
        {
            Context = "all",
            GroupString = "week,day",
            Period = "custom",
            From = dateValue,
            To = dateValue,
            Deleted = false,
            PreventLargeDataset = false,
            OrganisationId = organisationId,
            Type = "report",
            Paged = false
        };
    }

    private static string FormatDate(DateOnly date) => $"{date:yyyy-MM-dd}T00:00:00";
}
