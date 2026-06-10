package com.mirasiharput.features.quiz

/**
 * Mekan başına 10 soruluk bilgi yarışması içeriği.
 * Sorular mekan anlatım metinlerindeki bilgilere dayanır.
 */
object QuizRepository {

    const val QUESTIONS_PER_QUIZ = 10
    const val POINTS_PER_CORRECT_ANSWER = 10
    const val MAX_SCORE = QUESTIONS_PER_QUIZ * POINTS_PER_CORRECT_ANSWER

    private val questionsByLocation: Map<String, List<QuizQuestion>> = mapOf(
        "harput_kalesi" to listOf(
            QuizQuestion(
                text = "Harput Kalesi hangi uygarlık döneminde kurulmuştur?",
                options = listOf("Hititler", "Urartular", "Romalılar", "Osmanlılar"),
                correctIndex = 1,
                explanation = "Kale, Urartu Krallığı döneminde kurulmuştur.",
            ),
            QuizQuestion(
                text = "Tarihsel kaynaklara göre kale ne zaman kurulmuştur?",
                options = listOf("MÖ 8. yüzyıl", "MÖ 3. yüzyıl", "MS 5. yüzyıl", "MS 11. yüzyıl"),
                correctIndex = 0,
                explanation = "Kalenin kuruluşu MÖ 8. yüzyıla, Urartu dönemine tarihlenir.",
            ),
            QuizQuestion(
                text = "Harput Kalesi halk arasında hangi isimle bilinir?",
                options = listOf("Taş Kale", "Yıldız Kalesi", "Süt Kalesi", "Kartal Yuvası"),
                correctIndex = 2,
                explanation = "Halk arasında \"Süt Kalesi\" olarak bilinir.",
            ),
            QuizQuestion(
                text = "Efsaneye göre \"Süt Kalesi\" adı nereden gelir?",
                options = listOf(
                    "Surların harcına süt katıldığı rivayetinden",
                    "Kalenin beyaz taşlarından",
                    "Çevredeki süt pazarından",
                    "Bir kraliçenin adından",
                ),
                correctIndex = 0,
                explanation = "Rivayete göre surların harcına su yerine süt katılmıştır.",
            ),
            QuizQuestion(
                text = "Harput Kalesi nerede yükselir?",
                options = listOf(
                    "Geniş bir ovanın ortasında",
                    "Sarp kayalıklar üzerinde",
                    "Bir göl kıyısında",
                    "Nehir yatağının içinde",
                ),
                correctIndex = 1,
                explanation = "Kale, Harput'un sarp kayalıkları üzerinde yükselir.",
            ),
            QuizQuestion(
                text = "Kale, konumu sayesinde yüzyıllar boyunca hangi açılardan stratejik bir merkez olmuştur?",
                options = listOf(
                    "Ticaret ve tarım",
                    "Savunma, gözetleme ve yerleşim",
                    "Madencilik ve zanaat",
                    "Yalnızca dini törenler",
                ),
                correctIndex = 1,
                explanation = "Konumu; savunma, gözetleme ve yerleşim açısından stratejik olmuştur.",
            ),
            QuizQuestion(
                text = "Aşağıdakilerden hangisi Harput'ta iz bırakan medeniyetlerden biri DEĞİLDİR?",
                options = listOf("Artuklular", "Bizanslılar", "Vikingler", "Selçuklular"),
                correctIndex = 2,
                explanation = "Harput; Urartu, Pers, Roma, Bizans, Artuklu, Selçuklu ve Osmanlı izleri taşır.",
            ),
            QuizQuestion(
                text = "Harput hangi şehrimizin sınırları içindedir?",
                options = listOf("Malatya", "Diyarbakır", "Elazığ", "Tunceli"),
                correctIndex = 2,
                explanation = "Harput, Elazığ ilimizdedir.",
            ),
            QuizQuestion(
                text = "Harput Kalesi yalnızca bir savunma yapısı değil, aynı zamanda nedir?",
                options = listOf(
                    "Bir ticaret merkezi",
                    "Harput'un binlerce yıllık hafızasını temsil eden bir sembol",
                    "Bir su deposu",
                    "Bir saray kompleksi",
                ),
                correctIndex = 1,
                explanation = "Kale, Harput'un binlerce yıllık hafızasını temsil eden güçlü bir semboldür.",
            ),
            QuizQuestion(
                text = "Bugün kalede ziyaretçiye geçmişin izlerini hissettiren unsurlar nelerdir?",
                options = listOf(
                    "Surları, taş dokusu ve yüksek konumu",
                    "Modern restorasyon ekleri",
                    "Çevresindeki çarşılar",
                    "İçindeki müze vitrinleri",
                ),
                correctIndex = 0,
                explanation = "Surlar, taş dokusu ve yüksek konum geçmişin izlerini hissettirir.",
            ),
        ),
        "urartu_sarnici_zindani" to listOf(
            QuizQuestion(
                text = "Urartu Sarnıcı ilk yapıldığında hangi amaçla kullanılıyordu?",
                options = listOf(
                    "Tahıl deposu olarak",
                    "Kalenin su ihtiyacını karşılamak için",
                    "Hazine odası olarak",
                    "İbadethane olarak",
                ),
                correctIndex = 1,
                explanation = "Sarnıç, kalenin su ihtiyacını karşılamak için yapılmıştır.",
            ),
            QuizQuestion(
                text = "Sarnıç hangi uygarlığın eseridir?",
                options = listOf("Urartular", "Romalılar", "Selçuklular", "Osmanlılar"),
                correctIndex = 0,
                explanation = "Yapı, Urartu dönemine aittir.",
            ),
            QuizQuestion(
                text = "Kuşatma dönemlerinde su neden bu kadar önemliydi?",
                options = listOf(
                    "Ticarette değerli olduğu için",
                    "Kalenin hayatta kalması anlamına geldiği için",
                    "Yalnızca hayvanlar için gerektiğinden",
                    "Sur yapımında kullanıldığı için",
                ),
                correctIndex = 1,
                explanation = "Kuşatmada su, bir kalenin hayatta kalması demekti.",
            ),
            QuizQuestion(
                text = "Bu yapı zamanla başka hangi amaçla kullanılmıştır?",
                options = listOf("Kütüphane", "Zindan", "Han", "Hamam"),
                correctIndex = 1,
                explanation = "Sarnıç zamanla zindan olarak da kullanılmıştır.",
            ),
            QuizQuestion(
                text = "Yapının merdivenleri nasıl inşa edilmiştir?",
                options = listOf(
                    "Ahşaptan çakılarak",
                    "Mermer bloklarla örülerek",
                    "Kayaya oyularak",
                    "Tuğladan örülerek",
                ),
                correctIndex = 2,
                explanation = "Merdivenler kayaya oyulmuştur.",
            ),
            QuizQuestion(
                text = "Sarnıçlar neden yalnızca mimari yapılar olarak görülmez?",
                options = listOf(
                    "Yaşamın devamını sağlayan stratejik alanlar oldukları için",
                    "Altın saklandığı için",
                    "Yalnızca süsleme amaçlı oldukları için",
                    "Tapınak olarak kullanıldıkları için",
                ),
                correctIndex = 0,
                explanation = "Sarnıçlar yaşamın devamını sağlayan stratejik alanlardı.",
            ),
            QuizQuestion(
                text = "Urartu Sarnıcı / Zindanı, Harput'un hangi yönünü ziyaretçiye hissettirir?",
                options = listOf(
                    "Yalnızca dışarıdan görünen ihtişamını",
                    "Yer altında saklı kalan tarihini",
                    "Modern şehir yaşamını",
                    "Ticari gücünü",
                ),
                correctIndex = 1,
                explanation = "Yapı, Harput'un yer altında saklı kalan tarihini hissettirir.",
            ),
            QuizQuestion(
                text = "Yapının atmosferini tanımlayan özellikler hangileridir?",
                options = listOf(
                    "Geniş pencereleri ve aydınlık salonları",
                    "Derin yapısı ve kapalı atmosferi",
                    "Renkli çinileri",
                    "Yüksek kubbesi",
                ),
                correctIndex = 1,
                explanation = "Derin yapısı ve kapalı atmosferi yapıyı özel kılar.",
            ),
            QuizQuestion(
                text = "Sarnıç hangi yapının su ihtiyacını karşılamak için inşa edilmiştir?",
                options = listOf(
                    "Harput Kalesi'nin",
                    "Bir sarayın",
                    "Bir hamamın",
                    "Bir kervansarayın",
                ),
                correctIndex = 0,
                explanation = "Sarnıç, Harput Kalesi'nin su ihtiyacı için yapılmıştır.",
            ),
            QuizQuestion(
                text = "Ziyaretçi burada Harput'un hangi iki yönünü birlikte keşfeder?",
                options = listOf(
                    "Savunma gücünü ve yer altında gizlenen tarihini",
                    "El sanatlarını ve mutfağını",
                    "Müziğini ve edebiyatını",
                    "Tarımını ve hayvancılığını",
                ),
                correctIndex = 0,
                explanation = "Hem savunma gücü hem de yer altında gizlenen tarih burada keşfedilir.",
            ),
        ),
    )

    fun getQuestions(locationId: String): List<QuizQuestion> =
        questionsByLocation[locationId].orEmpty()
}
