using System.Text.Json;
using ClockApp.Infrastructure.Http;
using ClockApp.Infrastructure.TimeOn.Dtos;

namespace ClockApp.Infrastructure.TimeOn.Client;

public class TimeOnHourClient : BaseHttpClient
{
    public TimeOnHourClient(HttpClient httpClient) : base(httpClient) { }

    public Task<ApiResponse<TimeOnResultDto<HourDto>>> SaveHourAsync(HourSaveDto dto, CancellationToken cancellationToken = default) =>
        PostAsync<HourSaveDto, TimeOnResultDto<HourDto>>("api/hour/save", dto, cancellationToken);

    public Task<ApiResponse<TimeOnResultDto<JsonElement>>> ListHoursAsync(HourListRequestDto dto, CancellationToken cancellationToken = default) =>
        PostAsync<HourListRequestDto, TimeOnResultDto<JsonElement>>("api/hour/list", dto, cancellationToken);

    public Task<ApiResponse<TimeOnResultDto<JsonElement>>> LoadHourGroupAsync(HourGroupRequestDto dto, CancellationToken cancellationToken = default) =>
        PostAsync<HourGroupRequestDto, TimeOnResultDto<JsonElement>>("api/hour/group", dto, cancellationToken);
}
