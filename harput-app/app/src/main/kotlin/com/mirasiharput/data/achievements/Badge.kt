package com.mirasiharput.data.achievements

data class Badge(
    val id: String,
    val title: String,
    val description: String,
    val emblem: String,
)

/**
 * Miras'ı Harput rozet kataloğu.
 * Rozetler, Harput'un tarihî kimliğine (kale muhafızlığı, sarnıç bekçiliği,
 * âlimlik) atıfta bulunan bir "miras yolculuğu" hiyerarşisi kurar.
 */
object BadgeCatalog {

    val KESIF_YOLCUSU = Badge(
        id = "kesif_yolcusu",
        title = "Keşif Yolcusu",
        description = "İlk bilgi yarışmasını tamamlayarak Harput yolculuğuna adım attın.",
        emblem = "🧭",
    )

    val KALE_MUHAFIZI = Badge(
        id = "kale_muhafizi",
        title = "Kale Muhafızı",
        description = "Harput Kalesi yarışmasında 70 ve üzeri puan aldın; surların bekçisisin.",
        emblem = "🏰",
    )

    val SARNIC_BEKCISI = Badge(
        id = "sarnic_bekcisi",
        title = "Sarnıç Bekçisi",
        description = "Urartu Sarnıcı yarışmasında 70 ve üzeri puan aldın; yer altının sırlarını biliyorsun.",
        emblem = "🗝️",
    )

    val KUSURSUZ_HAFIZA = Badge(
        id = "kusursuz_hafiza",
        title = "Kusursuz Hafıza",
        description = "Bir yarışmadaki 10 sorunun tamamını doğru yanıtladın.",
        emblem = "📜",
    )

    val HARPUT_ALIMI = Badge(
        id = "harput_alimi",
        title = "Harput Âlimi",
        description = "Her iki mekanın yarışmasında da 90 ve üzeri puan alarak Harput'un âlimi oldun.",
        emblem = "🪶",
    )

    val all: List<Badge> = listOf(
        KESIF_YOLCUSU,
        KALE_MUHAFIZI,
        SARNIC_BEKCISI,
        KUSURSUZ_HAFIZA,
        HARPUT_ALIMI,
    )

    fun findById(id: String): Badge? = all.firstOrNull { it.id == id }
}
