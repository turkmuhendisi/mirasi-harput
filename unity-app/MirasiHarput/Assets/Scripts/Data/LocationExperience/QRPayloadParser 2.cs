using System;

public static class QRPayloadParser
{
    const string ExpectedPrefix = "MIRASI_HARPUT";
    const string ExpectedType = "LOCATION";
    const string ExpectedVersion = "v1";

    public static bool TryParse(string rawPayload, out string locationId, out string errorMessage)
    {
        locationId = string.Empty;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            errorMessage = "Geçersiz QR kod. Lütfen Miras'ı Harput mekan QR kodunu okutun.";
            return false;
        }

        var normalized = QrPayloadNormalizer.Normalize(rawPayload);
        if (string.IsNullOrEmpty(normalized))
        {
            errorMessage = "Geçersiz QR kod. Lütfen Miras'ı Harput mekan QR kodunu okutun.";
            return false;
        }

        var parts = normalized.Split('|');
        if (parts.Length != 4)
        {
            errorMessage = "Geçersiz QR kod. Lütfen Miras'ı Harput mekan QR kodunu okutun.";
            return false;
        }

        if (!string.Equals(parts[0].Trim(), ExpectedPrefix, StringComparison.Ordinal) ||
            !string.Equals(parts[1].Trim(), ExpectedType, StringComparison.Ordinal) ||
            !string.Equals(parts[3].Trim(), ExpectedVersion, StringComparison.Ordinal))
        {
            errorMessage = "Geçersiz QR kod. Lütfen Miras'ı Harput mekan QR kodunu okutun.";
            return false;
        }

        locationId = parts[2].Trim();
        if (string.IsNullOrEmpty(locationId))
        {
            errorMessage = "Geçersiz QR kod. Lütfen Miras'ı Harput mekan QR kodunu okutun.";
            return false;
        }

        return true;
    }
}
