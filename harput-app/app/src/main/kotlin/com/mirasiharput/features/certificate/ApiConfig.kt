package com.mirasiharput.features.certificate

/**
 * Sertifika backend bağlantı ayarları.
 *
 * Kendi sunucu bilgilerinizle güncelleyin:
 *  - BASE_URL: backend'in adresi. HTTPS önerilir. Sondaki "/" zorunludur.
 *  - API_KEY: backend .env içindeki API_KEY ile aynı değer (boş bırakılabilir).
 *  - ENABLE_LOGGING: ağ isteklerini Logcat'e yazmak için (yalnızca geliştirme).
 */
object ApiConfig {
    // AlmaLinux Docker backend (port 8087)
    const val BASE_URL = "http://187.127.83.10:8087/"
    // backend/.env içindeki API_KEY ile aynı değer
    const val API_KEY = ""
    const val ENABLE_LOGGING = true
}
