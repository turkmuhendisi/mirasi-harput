using System;

public static class QrPayloadNormalizer
{
    const string QrPathMarker = "/qr/";

    public static string Normalize(string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
            return string.Empty;

        var trimmed = rawPayload.Trim();
        if (trimmed.StartsWith("<", StringComparison.Ordinal))
            return string.Empty;

        var queryIndex = trimmed.IndexOf('?');
        if (queryIndex >= 0)
            trimmed = trimmed.Substring(0, queryIndex);

        trimmed = trimmed.TrimEnd('/');
        return trimmed;
    }

    public static bool IsDynamicRedirectHost(string normalizedPayload)
    {
        if (string.IsNullOrEmpty(normalizedPayload))
            return false;

        return normalizedPayload.IndexOf("me-qr.com", StringComparison.OrdinalIgnoreCase) >= 0 ||
            normalizedPayload.IndexOf("qrcode-tiger.com", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool TryExtractLocationId(string normalizedPayload, Func<string, bool> isKnownLocationId, out string locationId)
    {
        locationId = string.Empty;
        if (string.IsNullOrEmpty(normalizedPayload) || isKnownLocationId == null)
            return false;

        if (IsDynamicRedirectHost(normalizedPayload))
            return false;

        if (isKnownLocationId(normalizedPayload))
        {
            locationId = normalizedPayload;
            return true;
        }

        var qrPathIndex = normalizedPayload.IndexOf(QrPathMarker, StringComparison.OrdinalIgnoreCase);
        if (qrPathIndex >= 0)
        {
            locationId = normalizedPayload.Substring(qrPathIndex + QrPathMarker.Length).Trim('/');
            if (isKnownLocationId(locationId))
                return true;

            locationId = string.Empty;
        }

        const string locationMarker = "/location/";
        var locationIndex = normalizedPayload.IndexOf(locationMarker, StringComparison.OrdinalIgnoreCase);
        if (locationIndex >= 0)
        {
            locationId = normalizedPayload.Substring(locationIndex + locationMarker.Length).Trim('/');
            if (isKnownLocationId(locationId))
                return true;

            locationId = string.Empty;
        }

        if (normalizedPayload.Contains("://", StringComparison.Ordinal) || normalizedPayload.Contains("/", StringComparison.Ordinal))
        {
            var lastSlash = normalizedPayload.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < normalizedPayload.Length - 1)
            {
                var segment = normalizedPayload.Substring(lastSlash + 1).Trim('/');
                if (isKnownLocationId(segment))
                {
                    locationId = segment;
                    return true;
                }
            }
        }

        return false;
    }
}
