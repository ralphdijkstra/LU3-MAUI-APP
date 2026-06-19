using ClockApp.Application.Interfaces;
using System.Net.Http.Headers;

namespace ClockApp.Infrastructure.Http;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    public BearerTokenHandler(IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authService.GetAccessTokenAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
