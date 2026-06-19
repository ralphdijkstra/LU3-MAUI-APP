using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClockApp.Infrastructure.Http
{
    public abstract class BaseHttpClient
    {
        protected readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        protected BaseHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        protected async Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<T>
                    {
                        IsSuccess = false,
                        ErrorMessage = content,
                        StatusCode = (int)response.StatusCode
                    };
                }

                var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);

                return new ApiResponse<T>
                {
                    IsSuccess = true,
                    Data = data,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }

        protected async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, body, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<TResponse>
                    {
                        IsSuccess = false,
                        ErrorMessage = content,
                        StatusCode = (int)response.StatusCode
                    };
                }

                var data = JsonSerializer.Deserialize<TResponse>(content,_jsonOptions);

                return new ApiResponse<TResponse>
                {
                    IsSuccess = true,
                    Data = data,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }

        protected async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(body, _jsonOptions);
                using var requestContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url, requestContent, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<TResponse>
                    {
                        IsSuccess = false,
                        ErrorMessage = content,
                        StatusCode = (int)response.StatusCode
                    };
                }

                var data = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);

                return new ApiResponse<TResponse>
                {
                    IsSuccess = true,
                    Data = data,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }

        protected async Task<ApiResponse<object>> PostAsync<TRequest>(string url, TRequest body, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, body, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        ErrorMessage = content,
                        StatusCode = (int)response.StatusCode
                    };
                }

                return new ApiResponse<object>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }

        protected async Task<ApiResponse<object>> GetAsync(string url, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        ErrorMessage = content,
                        StatusCode = (int)response.StatusCode
                    };
                }

                return new ApiResponse<object>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }

        protected async Task<ApiResponse<TResponse>> PostWithoutBodyAsync<TResponse>(string url, CancellationToken ct = default)
        {
            try
            {
                var response = await _httpClient.PostAsync(url, null, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<TResponse>
                    {
                        IsSuccess = false,
                        ErrorMessage = content,
                        StatusCode = (int)response.StatusCode
                    };
                }

                var data = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);

                return new ApiResponse<TResponse>
                {
                    IsSuccess = true,
                    Data = data,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }

        protected async Task<ApiResponse<TResponse>> PostWithoutBodyWithBearerAsync<TResponse>(string url, string bearerToken, CancellationToken ct = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await _httpClient.SendAsync(request, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<TResponse>
                    {
                        IsSuccess = false,
                        ErrorMessage = content,
                        StatusCode = (int)response.StatusCode
                    };
                }

                var data = JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);

                return new ApiResponse<TResponse>
                {
                    IsSuccess = true,
                    Data = data,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<TResponse>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }

        protected async Task<ApiResponse<object>> PostWithBearerAsync(string url, string bearerToken, CancellationToken ct = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await _httpClient.SendAsync(request, ct);
                var content = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResponse<object>
                    {
                        IsSuccess = false,
                        ErrorMessage = content,
                        StatusCode = (int)response.StatusCode
                    };
                }

                return new ApiResponse<object>
                {
                    IsSuccess = true,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }
    }
}
