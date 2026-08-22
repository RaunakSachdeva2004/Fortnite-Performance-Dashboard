using System;

namespace FortniteDashboard.Models;

/// A simple result pattern to handle success and failure gracefully without throwing exceptions for expected errors
 
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    
    protected Result(bool isSuccess, T? value, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public static Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage);
}

// ---------------------------------------------------------------------
// Responses from Fortnite-API.com (https://fortnite-api.com/)
//
// CHANGED: the project originally targeted FortniteAPI.io, which has
// shut down (confirmed August 2026: "This API will close on March 31,
// 2026", and the domain no longer resolves). Fortnite-API.com is a
// long-running, actively maintained alternative with a documented,
// single-call BR stats endpoint.
//
// Verified against Fortnite-API.com's own docs (dash.fortnite-api.com):
//   GET https://fortnite-api.com/v2/stats/br/v2?name={username}&accountType=epic&timeWindow=lifetime
//   Header: Authorization: <api-key>
//
// ASSUMPTION (isolated here): the exact response body schema below
// (envelope -> data -> account / stats.all.overall) is reconstructed from
// third-party integration examples, since the vendor's own example
// responses are rendered client-side and weren't retrievable. Confirm
// the shape once you have a real API key -- see
// Docs/SQLite_Migration_Notes.md for how.
// ---------------------------------------------------------------------

/// <summary>Top-level envelope every Fortnite-API.com response is wrapped in.</summary>
public class FortniteApiEnvelope<T>
{
    public int Status { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

public class FortniteStatsData
{
    public FortniteAccountInfo? Account { get; set; }
    public FortniteStatsByInput? Stats { get; set; }
}

public class FortniteAccountInfo
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}

/// <summary>Stats are broken down by input device (all/keyboardMouse/gamepad/touch); we only use "all".</summary>
public class FortniteStatsByInput
{
    public FortniteModeBreakdown? All { get; set; }
}

/// <summary>Within a device breakdown, stats are further split by mode (overall/solo/duo/squad/ltm); we only use "overall".</summary>
public class FortniteModeBreakdown
{
    public FortniteOverallStats? Overall { get; set; }
}

public class FortniteOverallStats
{
    public int Wins { get; set; }
    public int Kills { get; set; }
    public int Matches { get; set; }

    // NOTE: Fortnite-API.com also reports its own "kd" and "winRate" fields,
    // but StatsService deliberately does NOT use them directly -- it computes
    // both itself via StatsCalculator from Wins/Kills/Matches, so the exact
    // same tested formula applies no matter which upstream API is behind
    // IFortniteApiClient. Only the raw counts are taken from here.
}
