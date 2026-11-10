using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Services;

/// <summary>Outcome of an API call that returns no body.</summary>
public record ApiResult(bool Success, HttpStatusCode StatusCode, string? Error)
{
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}

/// <summary>Outcome of an API call that returns data.</summary>
public record ApiResult<T>(bool Success, HttpStatusCode StatusCode, T? Data, string? Error)
{
    public bool IsUnauthorized => StatusCode == HttpStatusCode.Unauthorized;
}

/// <summary>
/// The only place the MVC app talks to the Part 2 API. Controllers never touch
/// the database; they ask this class, and it forwards the logged-in user's
/// API session cookie so the API knows who is calling.
/// </summary>
public class RaceDayApiClient
{
    public const string ApiCookieKey = "ApiSessionCookie";
    private const string ApiCookieName = "RaceDay.Session";

    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _accessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
    {
        _factory = factory;
        _accessor = accessor;
    }

    // ------------------------------------------------------------------ login

    /// <summary>Logs in through the API and keeps the API session cookie in the MVC session.</summary>
    public async Task<ApiResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            using var response = await SendRawAsync(HttpMethod.Post, "api/auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<AuthResponse>(false, response.StatusCode, default, await ReadErrorAsync(response));
            }

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
            if (auth is null)
            {
                return new ApiResult<AuthResponse>(false, response.StatusCode, default, "The login response could not be read.");
            }

            var cookie = ExtractSessionCookie(response);
            if (!string.IsNullOrWhiteSpace(cookie))
            {
                _accessor.HttpContext?.Session.SetString(ApiCookieKey, cookie);
            }

            return new ApiResult<AuthResponse>(true, response.StatusCode, auth, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ApiResult<AuthResponse>(false, HttpStatusCode.ServiceUnavailable, default, UnavailableMessage);
        }
    }

    // ---------------------------------------------------------- generic calls

    public Task<ApiResult<T>> GetAsync<T>(string url) => SendAsync<T>(HttpMethod.Get, url);

    /// <summary>Calls the API and reads the JSON body as T.</summary>
    public async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        try
        {
            using var response = await SendRawAsync(method, url, body);

            if (!response.IsSuccessStatusCode)
            {
                return new ApiResult<T>(false, response.StatusCode, default, await ReadErrorAsync(response));
            }

            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
            return new ApiResult<T>(true, response.StatusCode, data, null);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new ApiResult<T>(false, HttpStatusCode.ServiceUnavailable, default, UnavailableMessage);
        }
    }

    /// <summary>Calls the API when no response body is needed (DELETE, logout, and so on).</summary>
    public async Task<ApiResult> SendAsync(HttpMethod method, string url, object? body = null)
    {
        try
        {
            using var response = await SendRawAsync(method, url, body);

            return response.IsSuccessStatusCode
                ? new ApiResult(true, response.StatusCode, null)
                : new ApiResult(false, response.StatusCode, await ReadErrorAsync(response));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ApiResult(false, HttpStatusCode.ServiceUnavailable, UnavailableMessage);
        }
    }

    // ---------------------------------------------------------------- helpers

    private const string UnavailableMessage = "The RaceDay service is not available right now. Please try again shortly.";

    private async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string url, object? body = null)
    {
        var client = _factory.CreateClient("RaceDayApi");
        using var request = new HttpRequestMessage(method, url);

        var apiCookie = _accessor.HttpContext?.Session.GetString(ApiCookieKey);
        if (!string.IsNullOrWhiteSpace(apiCookie))
        {
            request.Headers.TryAddWithoutValidation("Cookie", apiCookie);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType());
        }

        return await client.SendAsync(request);
    }

    private static string? ExtractSessionCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            return null;
        }

        var header = values.FirstOrDefault(v =>
            v.StartsWith(ApiCookieName + "=", StringComparison.OrdinalIgnoreCase));

        // A request Cookie header needs only name=value, not Path or HttpOnly.
        return header?.Split(';', 2)[0];
    }

    /// <summary>Turns an API error into a short message that is safe to show to a user.</summary>
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        string? message = null;

        try
        {
            var text = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                using var doc = JsonDocument.Parse(text);
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                    {
                        message = m.GetString();
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON: fall back to a generic message below.
        }

        return message ?? response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Please log in again.",
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.NotFound => "That item could not be found.",
            HttpStatusCode.Conflict => "That action conflicts with existing information.",
            HttpStatusCode.BadRequest => "Some of the information entered is not valid.",
            _ => "Something went wrong. Please try again."
        };
    }
}
