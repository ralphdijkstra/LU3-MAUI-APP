using ClockApp.Infrastructure.Http;
using ClockApp.Infrastructure.TimeOn.Dtos;

namespace ClockApp.Infrastructure.TimeOn.Client;

public class TimeOnUserClient : BaseHttpClient
{
    public TimeOnUserClient(HttpClient httpClient) : base(httpClient) { }

    public Task<ApiResponse<TimeOnResultDto<UserInfoDto>>> GetUserInfoAsync(CancellationToken cancellationToken = default) =>
        PostWithoutBodyAsync<TimeOnResultDto<UserInfoDto>>("api/user/info", cancellationToken);

    public Task<ApiResponse<TimeOnResultDto<UserInfoDto>>> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default) =>
        PostWithoutBodyWithBearerAsync<TimeOnResultDto<UserInfoDto>>("api/user/info", accessToken, cancellationToken);
}
