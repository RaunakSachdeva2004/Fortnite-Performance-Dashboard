using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using FortniteDashboard.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FortniteDashboard.Services;

/// <summary>
/// The concrete implementation of <see cref="IFortniteApiClient"/>.
/// Handles direct HTTP communication with FortniteAPI.io and manages rate limiting in-memory.
/// </summary>
public class FortniteApiClient : IFortniteApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FortniteApiClient> _logger;

    // Rate Limiting fields: 10 requests per minute
    private static readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1);
    private static readonly Queue<DateTime> _requestTimestamps = new Queue<DateTime>();
    private const int MaxRequestsPerMinute = 10;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromMinutes(1);

    public FortniteApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<FortniteApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        // Ensure keys and base URLs are retrieved from the appsettings securely, not hardcoded.
        var baseUrl = _configuration["FortniteApi:BaseUrl"] ?? "https://fortniteapi.io/";
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
    public async Task<Result<FortnitePlayerStatsResponse>> GetPlayerStatsAsync(string epicUsername)
    {
        try
        {
            // 1. Fetch the Account ID using the Epic Username.
            var accountIdResult = await GetAccountIdAsync(epicUsername);
            if (!accountIdResult.IsSuccess)
            {
                return Result<FortnitePlayerStatsResponse>.Failure(accountIdResult.ErrorMessage ?? "Failed to resolve Account ID.");
            }

            string accountId = accountIdResult.Value!;

            // 2. Fetch the actual stats using the Account ID.
            var statsResult = await FetchStatsAsync(accountId);
            return statsResult;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP Request error while attempting to retrieve stats for username: {Username}", epicUsername);
            return Result<FortnitePlayerStatsResponse>.Failure("A network or server error occurred while contacting FortniteAPI.io.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching stats for username: {Username}", epicUsername);
            return Result<FortnitePlayerStatsResponse>.Failure("An unexpected error occurred processing the player data.");
        }
    }

    /// <summary>
    /// Looks up the account ID for the provided Epic Games username from FortniteAPI.io.
    /// </summary>
    private async Task<Result<string>> GetAccountIdAsync(string epicUsername)
    {
        await EnforceRateLimitAsync();

        var response = await _httpClient.GetAsync($"/v1/lookup?username={Uri.EscapeDataString(epicUsername)}");
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Fortnite API returned status {StatusCode} for lookup of {Username}", response.StatusCode, epicUsername);
            return Result<string>.Failure($"Account lookup failed with HTTP status code {(int)response.StatusCode}.");
        }

        var lookupData = await response.Content.ReadFromJsonAsync<FortniteAccountLookupResponse>();
        
        if (lookupData == null || !lookupData.Result)
        {
            var errorMsg = lookupData?.Error ?? "The specified account could not be found or is set to private.";
            return Result<string>.Failure($"Lookup failed: {errorMsg}");
        }

        if (string.IsNullOrEmpty(lookupData.Account_Id))
        {
            return Result<string>.Failure("The API succeeded but returned a null or empty Account ID.");
        }

        return Result<string>.Success(lookupData.Account_Id);
    }

    /// <summary>
    /// Fetches the player stats for a specific account ID from FortniteAPI.io.
    /// </summary>
    private async Task<Result<FortnitePlayerStatsResponse>> FetchStatsAsync(string accountId)
    {
        await EnforceRateLimitAsync();

        var response = await _httpClient.GetAsync($"/v1/stats?account={Uri.EscapeDataString(accountId)}");
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Fortnite API returned status {StatusCode} for stats of AccountID {AccountId}", response.StatusCode, accountId);
            return Result<FortnitePlayerStatsResponse>.Failure($"Stats request failed with HTTP status code {(int)response.StatusCode}.");
        }

        var statsData = await response.Content.ReadFromJsonAsync<FortnitePlayerStatsResponse>();
        
        if (statsData == null || !statsData.Result)
        {
            var errorMsg = statsData?.Error ?? "Stats data could not be found or the profile is hidden/private.";
            return Result<FortnitePlayerStatsResponse>.Failure($"Failed to fetch stats: {errorMsg}");
        }

        return Result<FortnitePlayerStatsResponse>.Success(statsData);
    }

    /// <summary>
    /// Implements a simple in-memory sliding window rate limit to ensure we do not exceed 
    /// the free-tier limit of 10 requests per minute on FortniteAPI.io.
    /// Blocks the current asynchronous flow until a request slot opens up.
    /// </summary>
    
    private async Task EnforceRateLimitAsync()
    {
        await _rateLimitSemaphore.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;

            // Purge expired request timestamps
            while (_requestTimestamps.Count > 0 && now - _requestTimestamps.Peek() >= RateLimitWindow)
            {
                _requestTimestamps.Dequeue();
            }

            if (_requestTimestamps.Count >= MaxRequestsPerMinute)
            {
                // Finds how long we need to wait until the oldest request leaves the 1-minute window
                var oldestRequestTime = _requestTimestamps.Peek();
                var timeToWait = RateLimitWindow - (now - oldestRequestTime);
                
                if (timeToWait > TimeSpan.Zero)
                {
                    _logger.LogInformation("Fortnite API Rate limit reached. Waiting {TotalSeconds:F1} seconds before proceeding.", timeToWait.TotalSeconds);
                    await Task.Delay(timeToWait);
                }
                
                // Repurge expired timestamps just in case, after the wait
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
