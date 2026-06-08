package com.mirasiharput.features.qr

object QRPayloadParser {

    private const val EXPECTED_PREFIX = "MIRASI_HARPUT"
    private const val EXPECTED_TYPE = "LOCATION"
    private const val EXPECTED_VERSION = "v1"

    const val INVALID_QR_MESSAGE =
        "Geçersiz QR kod. Lütfen Miras'ı Harput mekan QR kodunu okutun."

    const val LOCATION_NOT_FOUND_MESSAGE =
        "Bu QR kod için mekan içeriği bulunamadı."

    fun parse(rawPayload: String?): String? {
        if (rawPayload.isNullOrBlank()) return null

        val normalized = rawPayload.trim()
        val parts = normalized.split("|")
        if (parts.size != 4) return null

        if (parts[0].trim() != EXPECTED_PREFIX) return null
        if (parts[1].trim() != EXPECTED_TYPE) return null
        if (parts[3].trim() != EXPECTED_VERSION) return null

        val locationId = parts[2].trim()
        return locationId.ifBlank { null }
    }
}
