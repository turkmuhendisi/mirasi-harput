# Miras'ı Harput – Android (Kotlin + Jetpack Compose)

QR tabanlı AR mekan deneyimi. Uygulama tarihî miras konseptli bir **Ana Ekran** ile açılır. Kullanıcı "Keşfetmeye Başla" ile QR okuyucuya geçer, geçerli mekan QR kodunu okutunca kamera + 3D model + tarih metni + seslendirme deneyimi açılır.

## Uygulama Akışı

```
Ana Ekran (Home)
  → Keşfetmeye Başla → QR Reader
      → QR okut → Mekan Deneyimi (kamera + 3D model + metin + ses)
      → Geri → QR Reader → Geri → Ana Ekran
  → Her iki mekan ziyaret edilince Ana Ekran'da "Sertifika" ödülü görünür
      → Sertifikamı Al → Ad Soyad + E-posta → backend sertifikayı e-posta ile gönderir
```

- Ziyaret durumu cihazda (DataStore) kalıcı saklanır.
- İki mekan da ziyaret edilince ana ekranda sertifika ödül kartı çıkar.
- Sertifika üretimi/e-posta/DB işlemleri `backend/` (PHP + MySQL) tarafında yapılır.

## Backend (Docker – AlmaLinux)

API sunucusu: **`http://187.127.83.10:8087/`** (port 8087, diğer projelerle çakışmaz)

Sunucuda kurulum ve güncelleme: `backend/README.md`

```bash
cd harput-app/backend
cp .env.example .env   # ilk kurulumda
./deploy.sh            # git pull + docker yeniden build
```

Android `ApiConfig.kt` içinde `BASE_URL` ve `API_KEY` backend `.env` ile eşleşmeli.

## Gereksinimler

- Android Studio Ladybug veya üzeri
- JDK 17
- Android SDK 35
- Fiziksel Android cihaz (kamera + QR test için önerilir)

## Android Studio'da Açma

1. **File → Open** ile `harput-app` klasörünü seçin.
2. Gradle sync tamamlanana kadar bekleyin.
3. `local.properties` dosyasında SDK yolu tanımlı olmalı:

```properties
sdk.dir=/Users/<kullanici>/Library/Android/sdk
```

4. Cihazı bağlayıp **Run** ile yükleyin.

## Aktif Mekanlar

| Mekan | QR Payload |
|-------|------------|
| Harput Kalesi | `MIRASI_HARPUT\|LOCATION\|harput_kalesi\|v1` |
| Urartu Sarnıcı / Zindanı | `MIRASI_HARPUT\|LOCATION\|urartu_sarnici_zindani\|v1` |

Test için bu metinleri içeren QR kodlar üretebilirsiniz ([qr-code-generator.com](https://www.qr-code-generator.com/) vb.).

## Yerel Varlıklar (Cihazda)

Tüm içerik APK içinde `assets/` altında paketlenir:

```
app/src/main/assets/
  models/mirasiharput-model.glb
  audio/harput_kalesi.mp3
  audio/urartu_sarnici_zindani.mp3
```

Yeni mekan eklemek için `LocationRepository.kt` dosyasına yeni bir kayıt eklemeniz yeterlidir.

## Mimari

```
features/qr/              → QR Reader, payload parser, ML Kit tarama
features/locationExperience/ → Mekan ekranı, 3D model, ses, bilgi paneli
features/camera/          → CameraX önizleme
data/                     → LocationModel, LocationRepository
navigation/               → AppExperienceState
```

## Devre Dışı (Bu Sürümde Yok)

GPS, NPC, görev alma/teslim, rota ilerlemesi, puan/rozet ve konum tetikleme bu Android sürümünde uygulanmamıştır. Eski Unity projesindeki bu sistemler `unity-app/MirasiHarput` altında modüler olarak durmaya devam eder.

## Komut Satırı Derleme

```bash
./gradlew assembleDebug
```

APK: `app/build/outputs/apk/debug/app-debug.apk`
