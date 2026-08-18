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

// Responses from FortniteAPI.io

public class FortniteAccountLookupResponse
{
    public bool Result { get; set; }
    public string? Account_Id { get; set; }
    public string? Error { get; set; }
}

public class FortnitePlayerStatsResponse
{
    public bool Result { get; set; }
    public string? Error { get; set; }
    public string? Name { get; set; }
    public FortniteAccountData? Account { get; set; }
    public FortniteGlobalStats? Global_Stats { get; set; }
}

public class FortniteAccountData
{
    public int Level { get; set; }
}

public class FortniteGlobalStats
{
    public FortniteModeStats? Solo { get; set; }
    public FortniteModeStats? Duo { get; set; }
    public FortniteModeStats? Squad { get; set; }
}

public class FortniteModeStats
{
    public int Placetop1 { get; set; } // Wins
    public int Kills { get; set; }
    public int Matchesplayed { get; set; }
    public double Kd { get; set; }
    public double Winrate { get; set; }
    public int Score { get; set; }
}
