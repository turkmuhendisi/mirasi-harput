# Miras'ı Harput

## 1. Proje Özeti

Miras'ı Harput, Elazığ Harput bölgesinin tarihî ve kültürel mirasını mobil artırılmış gerçeklik teknolojileriyle destekleyen, oyunlaştırılmış bir gezi deneyimi projesidir. Projenin aktif Android sürümü; QR kod ile mekân deneyimi başlatma, kamera üzerinde 3D/avatar görüntüleme, tarihî metin ve sesli anlatım sunma, avatarla etkileşime girildiğinde bilgi yarışmasını açma, puan/rozet kazanımı ve ziyaret tamamlandığında sertifika alma akışlarını içerir.

Uygulama, kullanıcıyı pasif bir ziyaretçi olmaktan çıkarıp tarihî mekânla etkileşim kuran aktif bir katılımcıya dönüştürmeyi amaçlar. Kullanıcı Harput Kalesi ve Urartu Sarnıcı / Zindanı gibi noktalardaki QR kodları okutarak ilgili içeriklere ulaşır; mekânları tamamladığında katılım sertifikasını e-posta ile alabilir.

## 2. Problem Tanımı

Tarihî mekânlarda kullanılan geleneksel anlatım yöntemleri çoğu zaman ziyaretçiyi pasif bir izleyici konumunda bırakmaktadır. Bilgilendirme levhaları, broşürler ve klasik rehberlik yöntemleri özellikle genç kullanıcılar için yeterince dikkat çekici olmayabilmektedir. Bu durum, tarihî mekân deneyiminin yüzeysel kalmasına ve kültürel miras bilgisinin kalıcı öğrenmeye dönüşmemesine neden olabilir.

Miras'ı Harput, bu problemi QR tabanlı mobil erişim, 3D/AR içerik, sesli anlatım, bilgi yarışması, puan/rozet sistemi ve dijital sertifika mekanizması ile çözmeyi hedefler. Böylece ziyaretçi hem fiziksel sahada bulunur hem de dijital içeriklerle desteklenen daha etkileşimli bir öğrenme deneyimi yaşar.

## 3. Amaçlar

- Harput'un tarihî ve kültürel mirasını yenilikçi bir yöntemle kullanıcıya aktarmak
- Tarihî gezi deneyimini etkileşimli, öğretici ve akılda kalıcı hale getirmek
- Fiziksel saha deneyimini 3D model, AR, sesli anlatım ve quiz içerikleriyle zenginleştirmek
- Genç kullanıcılar için daha dikkat çekici bir öğrenme modeli sunmak
- Yerel turizme ve kültürel miras farkındalığına katkı sağlamak
- Ziyaret tamamlanmasını puan, rozet ve sertifika gibi oyunlaştırma unsurlarıyla teşvik etmek
- Harput bölgesi için genişletilebilir bir dijital rehberlik altyapısı oluşturmak

## 4. Temel Kavramlar

- **XR (Extended Reality):** Gerçek ve sanal ortamları bir araya getiren genişletilmiş gerçeklik kavramıdır.
- **AR (Augmented Reality):** Gerçek dünya görüntüsü üzerine dijital nesnelerin bindirilmesiyle oluşturulan artırılmış gerçeklik deneyimidir.
- **QR Tabanlı Deneyim:** Kullanıcının fiziksel mekândaki QR kodu okutarak ilgili dijital içeriğe ulaşmasını sağlayan deneyim modelidir.
- **3D Model / Avatar:** Tarihî mekân deneyimini görsel olarak desteklemek için mobil cihazda görüntülenen ve kullanıcı etkileşimiyle quiz akışını başlatabilen dijital karakter/nesnedir.
- **Oyunlaştırma:** Puan, rozet, başarı ve sertifika gibi oyun mekaniklerinin öğrenme/gezi deneyimine uygulanmasıdır.
- **Sertifika Mekanizması:** Kullanıcının gerekli mekânları tamamladıktan sonra ad soyad ve e-posta bilgileriyle dijital katılım sertifikası almasını sağlayan sistemdir.
- **NPC / Avatar Etkileşimi:** Kullanıcının dijital avatarla etkileşime girerek mekâna bağlı bilgi yarışmasını başlatmasını sağlayan etkileşim modelidir.
- **Konum Tabanlı Deneyim:** Kullanıcının fiziksel konumuna göre içerik tetiklenmesidir. Mevcut Android sürümünde GPS tabanlı tetikleme yerine QR tabanlı mekân doğrulama kullanılmaktadır.

## 5. Kullanılan Teknolojiler

Aktif Android uygulama ve backend yapısında kullanılan başlıca teknolojiler şunlardır:

- Kotlin
- Jetpack Compose
- Android SDK 35
- CameraX
- ML Kit Barcode Scanning
- SceneView / ARSceneView
- ARCore
- Android DataStore
- Retrofit / OkHttp
- PHP
- MySQL
- Docker
- PHPMailer
- GitHub
- Fiziksel Android test cihazı

Projenin eski Unity kapsamındaki Unity, AR Foundation, C#, GPS / Konum Servisleri ve rota bazlı görev sistemi gibi başlıklar bu `harput-app` Android sürümünün aktif kapsamı değildir. Mevcut Android akışı QR, 3D/avatar tabanlı AR mekân deneyimi, avatar etkileşimiyle açılan quiz, sesli anlatım, puan/rozet ve sertifika üzerine kuruludur.

## 6. Proje Kapsamı

Proje, Harput bölgesindeki belirli tarihî noktalar üzerinde çalışan mobil tabanlı bir kültürel miras deneyim sistemi olarak tasarlanmıştır. Aktif Android sürümünün kapsamı şunları içerir:

- Tarihî miras konseptli ana ekran
- QR kod ile mekân deneyimi başlatma
- Harput Kalesi ve Urartu Sarnıcı / Zindanı için tanımlı içerikler
- Kamera önizlemesi ve QR okuma
- 3D model/avatar görüntüleme
- AR deneyimi ile model yerleştirme
- Avatarla etkileşime girildiğinde bilgi yarışmasının açılması
- Mekân açıklaması ve sesli anlatım
- Mekân başına avatar etkileşimli bilgi yarışması
- Miras Puanı ve rozet sistemi
- Ziyaret ilerlemesinin cihazda kalıcı saklanması
- İki mekân tamamlandığında sertifika ödül kartının açılması
- PHP + MySQL backend üzerinden sertifika üretimi ve e-posta gönderimi

Kapsam dışında veya gelecek geliştirme olarak değerlendirilen konular:

- Tam ölçekli çok oyunculu sistemler
- Gelişmiş sosyal medya entegrasyonları
- GPS tabanlı otomatik konum tetikleme
- Eski Unity projesindeki rota bazlı GPS görev sistemi
- Tüm Harput tarihî alanları için tam içerik üretimi
- Ticari yayına hazır nihai ürün seviyesi için gerekli tüm güvenlik ve operasyon sertleştirmeleri

## 7. Beklenen Çıktılar

- Harput için çalışan mobil AR/XR destekli Android deneyimi
- QR tabanlı temel kullanıcı akışı
- Harput Kalesi ve Urartu Sarnıcı / Zindanı için mekân içerikleri
- 3D/avatar, metin ve sesli anlatım destekli mekân deneyimi
- AR avatar/model yerleştirme ve avatar etkileşimiyle açılan bilgi yarışması akışı
- Puan, rozet ve başarı mantığı
- İki mekân tamamlandığında sertifika alma süreci
- PHP + MySQL tabanlı sertifika backend'i
- Teknik ve akademik proje dokümantasyonu

## 8. Katkıda Bulunanlar

- Samet Berkant KOCA

## 9. Kaynaklar

- Harput tarihine ilişkin yerel ve akademik kaynaklar
- Android resmi geliştirici dokümantasyonu
- Jetpack Compose dokümantasyonu
- CameraX dokümantasyonu
- ML Kit Barcode Scanning dokümantasyonu
- ARCore geliştirici dokümantasyonu
- SceneView dokümantasyonu
- PHP, MySQL ve Docker dokümantasyonları
- XR, AR ve kültürel miras teknolojileri üzerine akademik çalışmalar

## 10. Anahtar Kelimeler

XR, AR, Artırılmış Gerçeklik, Karma Gerçeklik, Harput, Elazığ, Kültürel Miras, Oyunlaştırma, Mobil Uygulama, QR, 3D Model, Avatar, NPC, Sesli Anlatım, Bilgi Yarışması, Rozet, Sertifika, Tarihsel Deneyim, Konum Tabanlı Deneyim
