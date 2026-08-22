using System.Net.Http.Json;
using FortniteDashboard.Models;

namespace FortniteDashboard.Services;

/// <summary>
/// The concrete implementation of <see cref="IFortniteApiClient"/>, targeting
/// Fortnite-API.com. Handles direct HTTP communication and manages a simple
/// in-memory rate limit.
///
/// CHANGED: this used to target FortniteAPI.io via a two-step
/// lookup-then-stats flow. FortniteAPI.io shut down (confirmed August 2026),
/// so this now targets Fortnite-API.com instead, which resolves a username
/// directly in a single call.
/// </summary>
public class FortniteApiClient : IFortniteApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FortniteApiClient> _logger;

    // Rate Limiting fields: a conservative, provider-agnostic safety net.
    // Fortnite-API.com's exact published limit wasn't verified, so this
    // errs on the safe side rather than assuming a specific number.
    private static readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1);
    private static readonly Queue<DateTime> _requestTimestamps = new Queue<DateTime>();
    private const int MaxRequestsPerMinute = 10;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    public FortniteApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<FortniteApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Ensure keys and base URLs are retrieved from appsettings/User Secrets, not hardcoded.
        var baseUrl = _configuration["FortniteApi:BaseUrl"] ?? "https://fortnite-api.com/";
        var apiKey = _configuration["FortniteApi:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("FortniteApi:ApiKey is missing or empty in configuration.");
        }

        _httpClient.BaseAddress = new Uri(baseUrl);

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", apiKey);
        }
    }

    /// <inheritdoc />
    public async Task<Result<FortniteOverallStats>> GetPlayerStatsAsync(string epicUsername, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnforceRateLimitAsync(cancellationToken);

            var url = $"/v2/stats/br/v2?name={Uri.EscapeDataString(epicUsername)}&accountType=epic&timeWindow=lifetime";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Fortnite-API.com returned status {StatusCode} for username {Username}", response.StatusCode, epicUsername);

                var friendlyMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.NotFound => "That Epic username could not be found.",
                    System.Net.HttpStatusCode.Forbidden => "The Fortnite API key is missing, invalid, or the player's stats are private.",
                    System.Net.HttpStatusCode.BadRequest => "The request to Fortnite-API.com was invalid (check the username).",
                    _ => $"Fortnite-API.com request failed with HTTP status code {(int)response.StatusCode}."
                };

                return Result<FortniteOverallStats>.Failure(friendlyMessage);
            }

            var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("RAW FORTNITE-API.COM RESPONSE for {Username}: {RawJson}", epicUsername, rawJson);

            var envelope = System.Text.Json.JsonSerializer.Deserialize<FortniteApiEnvelope<FortniteStatsData>>(
                rawJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var overall = envelope?.Data?.Stats?.All?.Overall;
            if (overall is null)
            {
                return Result<FortniteOverallStats>.Failure(
                    envelope?.Error ?? "Fortnite-API.com returned no usable stats for this player.");
            }

            return Result<FortniteOverallStats>.Success(overall);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Request error while attempting to retrieve stats for username: {Username}", epicUsername);
            return Result<FortniteOverallStats>.Failure("A network or server error occurred while contacting Fortnite-API.com.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching stats for username: {Username}", epicUsername);
            return Result<FortniteOverallStats>.Failure("An unexpected error occurred processing the player data.");
        }
    }

    /// <summary>
    /// Implements a simple in-memory sliding window rate limit so we don't
    /// hammer the free tier. Blocks the current asynchronous flow until a
    /// request slot opens up.
    /// </summary>
    private async Task EnforceRateLimitAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;

            while (_requestTimestamps.Count > 0 && now - _requestTimestamps.Peek() >= RateLimitWindow)
            {
                _requestTimestamps.Dequeue();
            }

            if (_requestTimestamps.Count >= MaxRequestsPerMinute)
            {
                var oldestRequestTime = _requestTimestamps.Peek();
                var timeToWait = RateLimitWindow - (now - oldestRequestTime);

                if (timeToWait > TimeSpan.Zero)
                {
                    _logger.LogInformation("Fortnite API rate limit reached. Waiting {TotalSeconds:F1} seconds before proceeding.", timeToWait.TotalSeconds);
                    await Task.Delay(timeToWait, cancellationToken);
                }

                now = DateTime.UtcNow;
                while (_requestTimestamps.Count > 0 && now - _requestTimestamps.Peek() >= RateLimitWindow)
                {
                    _requestTimestamps.Dequeue();
                }
            }

            _requestTimestamps.Enqueue(DateTime.UtcNow);
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }
}
