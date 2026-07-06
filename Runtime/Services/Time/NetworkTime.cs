using System;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


public enum NetworkTimeStatus
{
    Success,
    Failure,
    Loading
}

public static class NetworkTime
{
    private const string TimeAPI = "https://timeapi.io/api/v1/time/current/utc";
    private const string TimeKey = "NetworkTime_UTC";

    [Serializable]
    private class WorldTimeResponse
    {
        public string utc_time;
    }

    public static async void GetNetworkTime(
        Action<DateTime> onSuccess,
        Action<Exception> onFailure = null)
    {
        try
        {
            using var request = UnityWebRequest.Get(TimeAPI);
            await SendWebRequestAsync(request);

            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new Exception(
                    $"Could not get network time: {request.error}");
            }

            var response =
                JsonUtility.FromJson<WorldTimeResponse>(
                    request.downloadHandler.text);

            if (response == null ||
                string.IsNullOrWhiteSpace(response.utc_time))
            {
                throw new FormatException(
                    "Network time response does not contain utc_time.");
            }

            if (!DateTimeOffset.TryParse(
                    response.utc_time,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal |
                    DateTimeStyles.AdjustToUniversal,
                    out var utcTime))
            {
                throw new FormatException(
                    $"Invalid utc_time value: {response.utc_time}");
            }

            onSuccess?.Invoke(ConvertUtcToLocalTime(utcTime.UtcDateTime));
        }
        catch (Exception exception)
        {
            onFailure?.Invoke(exception);
        }
    }

    public static async Task<DateTime> GetTimeAsync()
    {
        try
        {
            using var request = UnityWebRequest.Get(TimeAPI);
            await SendWebRequestAsync(request);

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception(
                    $"Could not get network time: {request.error}");

            var response =
                JsonUtility.FromJson<WorldTimeResponse>(
                    request.downloadHandler.text);

            if (response == null ||
                string.IsNullOrWhiteSpace(response.utc_time))
            {
                throw new FormatException(
                    "Network time response does not contain utc_time.");
            }

            if (!DateTimeOffset.TryParse(
                    response.utc_time,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal |
                    DateTimeStyles.AdjustToUniversal,
                    out var utcTime))
            {
                throw new FormatException(
                    $"Invalid utc_time value: {response.utc_time}");
            }

            DateTime result = utcTime.UtcDateTime;
            SetCachedUtcTime(result);
            return ConvertUtcToLocalTime(result);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"Could not get network time. Using cached time. " +
                exception.Message);
            return ConvertUtcToLocalTime(GetCachedUtcTime());
        }
    }

    private static DateTime GetCachedUtcTime()
    {
        DateTime currentUtcTime = DateTime.UtcNow;

        try
        {
            string cachedDateTimeString =
                RuntimeStorageData.PLAYER.ExtensionData.Get(TimeKey);

            if (DateTime.TryParse(
                    cachedDateTimeString,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var cachedUtcTime))
            {
                return cachedUtcTime.ToUniversalTime();
            }

            SetCachedUtcTime(currentUtcTime);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                $"Could not read or save cached UTC time. " +
                exception.Message);
        }

        return currentUtcTime;
    }

    private static void SetCachedUtcTime(DateTime utcTime)
    {
        string dateTimeString =
            utcTime.ToUniversalTime().ToString(
                "o",
                CultureInfo.InvariantCulture);

        RuntimeStorageData.PLAYER.ExtensionData.Set(
            TimeKey,
            dateTimeString);
    }

    private static DateTime ConvertUtcToLocalTime(DateTime utcTime)
    {
        DateTime normalizedUtcTime =
            DateTime.SpecifyKind(utcTime, DateTimeKind.Utc);

        TimeSpan gmtOffset =
            TimeZoneInfo.Local.GetUtcOffset(normalizedUtcTime);

        return DateTime.SpecifyKind(
            normalizedUtcTime + gmtOffset,
            DateTimeKind.Unspecified);
    }

    private static Task SendWebRequestAsync(UnityWebRequest request)
    {
        var completionSource = new TaskCompletionSource<bool>();
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        operation.completed += _ => completionSource.TrySetResult(true);
        return completionSource.Task;
    }
}
