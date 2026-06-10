package com.mirasiharput.data

import com.mirasiharput.features.qr.QRPayloadParser

object LocationRepository {

    const val SHARED_MODEL_PATH = "models/mirasiharput-model.glb"

    private val locations: List<LocationModel> = listOf(
        LocationModel(
            id = "harput_kalesi",
            title = "Harput Kalesi",
            qrPayload = "MIRASI_HARPUT|LOCATION|harput_kalesi|v1",
            modelPath = SHARED_MODEL_PATH,
            audioPath = "audio/harput_kalesi.mp3",
            description =
                "Harput Kalesi, Harput'un sarp kayalıkları üzerinde yükselen en önemli tarihî yapılardan biridir. " +
                    "Tarihsel kaynaklara göre kale, MÖ 8. yüzyılda Urartu Krallığı döneminde kurulmuştur. Konumu sayesinde yüzyıllar boyunca savunma, gözetleme ve yerleşim açısından stratejik bir merkez olmuştur.\n\n" +
                    "Harput; Urartular, Persler, Romalılar, Bizanslılar, Artuklular, Selçuklular ve Osmanlılar gibi birçok medeniyetin izlerini taşır. Halk arasında \"Süt Kalesi\" olarak da bilinen Harput Kalesi, yalnızca bir savunma yapısı değil, aynı zamanda Harput'un binlerce yıllık hafızasını temsil eden güçlü bir semboldür.\n\n" +
                    "Bugün kalenin surları, taş dokusu ve yüksek konumu ziyaretçiye geçmişin izlerini hissettirir. Burada görülen her taş, Harput'un uzun tarih yolculuğundan bir parça taşır.",
            voiceText =
                "Harput Kalesi'ne hoş geldiniz. Şu anda Harput'un en güçlü sembollerinden birinin önündesiniz.\n\n" +
                    "Bu kale, binlerce yıldır bölgenin hafızasını taşır. Urartular döneminde kurulan yapı, zaman içinde birçok medeniyetin izlerini üzerinde toplamıştır.\n\n" +
                    "Yüksek kayalıklar üzerine kurulu olması, onu yalnızca bir kale değil, aynı zamanda Harput'un koruyucu gözü haline getirmiştir.\n\n" +
                    "Bugün burada gördüğünüz taşlar, geçmişten bugüne uzanan uzun bir hikâyenin sessiz tanıklarıdır.",
            modelTransform = ModelTransform(
                scale = 0.8f,
                rotationY = 0f,
                positionZ = -2.0f,
            ),
        ),
        LocationModel(
            id = "urartu_sarnici_zindani",
            title = "Urartu Sarnıcı / Zindanı",
            qrPayload = "MIRASI_HARPUT|LOCATION|urartu_sarnici_zindani|v1",
            modelPath = SHARED_MODEL_PATH,
            audioPath = "audio/urartu_sarnici_zindani.mp3",
            description =
                "Şimdi Harput'un yer altında saklı kalan tarihine yaklaşıyorsunuz. Burası Urartu Sarnıcı ve Zindanı. İlk yapıldığında kalenin su ihtiyacını karşılamak için kullanılıyordu.\n\n" +
                    "Kuşatma dönemlerinde su, bir kalenin hayatta kalması demekti. Bu yüzden sarnıçlar yalnızca mimari yapılar değil, aynı zamanda yaşamın devamını sağlayan stratejik alanlardı.\n\n" +
                    "Zamanla bu yapı zindan olarak da kullanıldı. Kayaya oyulmuş merdivenleri, derin yapısı ve kapalı atmosferiyle Urartu Sarnıcı / Zindanı, Harput'un yalnızca dışarıdan görünen ihtişamını değil, yer altında saklı kalan tarihini de ziyaretçiye hissettirir.\n\n" +
                    "Bugün burada, Harput'un hem savunma gücünü hem de yer altında gizlenen tarihini keşfediyorsunuz.",
            voiceText =
                "Şimdi Harput'un yer altında saklı kalan tarihine yaklaşıyorsunuz.\n\n" +
                    "Burası Urartu Sarnıcı ve Zindanı. İlk yapıldığında kalenin su ihtiyacını karşılamak için kullanılıyordu.\n\n" +
                    "Kuşatma dönemlerinde su, bir kalenin hayatta kalması demekti. Bu yüzden sarnıçlar yalnızca mimari yapılar değil, aynı zamanda yaşamın devamını sağlayan stratejik alanlardı.\n\n" +
                    "Zamanla bu yapı zindan olarak da kullanıldı.\n\n" +
                    "Bugün burada, Harput'un hem savunma gücünü hem de yer altında gizlenen tarihini keşfediyorsunuz.",
            modelTransform = ModelTransform(
                scale = 0.8f,
                rotationY = 0f,
                positionZ = -2.0f,
            ),
        ),
    )

    private val byId: Map<String, LocationModel> = locations.associateBy { it.id }
    private val byQrPayload: Map<String, LocationModel> = locations.associateBy { it.qrPayload }

    fun getAll(): List<LocationModel> = locations

    fun findById(locationId: String): LocationModel? = byId[locationId]

    fun findByQrPayload(rawPayload: String): LocationModel? {
        val normalized = rawPayload.trim()
        byQrPayload[normalized]?.let { return it }

        val locationId = QRPayloadParser.parse(normalized) ?: return null
        return findById(locationId)
    }
}
