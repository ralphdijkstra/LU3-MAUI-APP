using ClockApp.Infrastructure.Http;
using ClockApp.Infrastructure.TimeOn.Dtos;

namespace ClockApp.Infrastructure.TimeOn.Client;

public class AuthClient : BaseHttpClient
{
    public AuthClient(HttpClient httpClient) : base(httpClient) { }

    public Task<ApiResponse<TokenResponseDto>> LoginWithPasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var url = $"token?grant_type=password&username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}";

        return RequestTokenAsync(url, cancellationToken);
    }

    public Task<ApiResponse<TokenResponseDto>> LoginWithApiTokenAsync(string apiToken, CancellationToken cancellationToken = default)
    {
        var url = $"token?grant_type=apitoken&token={Uri.EscapeDataString(apiToken)}";

        return RequestTokenAsync(url, cancellationToken);
    }

    public Task<ApiResponse<object>> LogoutAsync(string accessToken, CancellationToken cancellationToken = default) =>
        PostWithBearerAsync("token/logout", accessToken, cancellationToken);

    private async Task<ApiResponse<TokenResponseDto>> RequestTokenAsync(string url, CancellationToken cancellationToken)
    {
        var result = await PostWithoutBodyAsync<TokenResponseDto>(url, cancellationToken);

        if (!result.IsSuccess)
            return result;

        var data = result.Data;

        if (data == null || !data.Success || string.IsNullOrWhiteSpace(data.AccessToken))
        {
            return new ApiResponse<TokenResponseDto>
            {
                IsSuccess = false,
                Data = data,
                ErrorMessage = data?.ErrorMessage ?? "Authentication failed.",
                StatusCode = result.StatusCode
            };
        }

        return result;
    }
}
