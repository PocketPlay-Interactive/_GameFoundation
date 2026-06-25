using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public static class NetworkTime
{
    [Serializable]
    private class WorldTimeResponse
    {
        public string utc_time;
    }

    public static async UniTask<DateTime> GetTimeAsync()
    {
        using var request =
            UnityWebRequest.Get(
                "https://timeapi.io/api/v1/time/current/utc");

        await request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            throw new Exception(
                $"Could not get network time: {request.error}");

        var response =
            UnityEngine.JsonUtility.FromJson<WorldTimeResponse>(
                request.downloadHandler.text);

        if (response == null || string.IsNullOrWhiteSpace(response.utc_time))
            throw new FormatException(
                "Network time response does not contain utc_time.");

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

        return utcTime.UtcDateTime;
    }
}
