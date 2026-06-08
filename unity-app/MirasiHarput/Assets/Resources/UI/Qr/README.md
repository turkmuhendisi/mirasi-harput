# QR akış arayüzü (kendi tasarımınız)

## Hızlı başlangıç

1. Unity menüsü: **Mirasi Harput → UI → Create QR Flow UI Prefabs**
2. Oluşan prefab: `Assets/Resources/UI/Qr/QrFlowUI.prefab`
3. Prefab'ı açın, renk/font/layout'u istediğiniz gibi düzenleyin
4. Kaydedin — Play modunda otomatik yüklenir

## Sahneye elle yerleştirme (isteğe bağlı)

`QrFlowUI` prefab'ını `MainScene` içine sürükleyebilirsiniz. Sahnedeki örnek, Resources'tan yüklemeden önceliklidir.

## Bileşenler

| Bileşen | Görev |
|---------|--------|
| `QrFlowUIView` | Tüm ekran, metin ve buton referansları (Inspector) |
| `QrFlowController` | Akış mantığı (ekran geçişi, QR, rota) |
| `QrRouteListItem` | Rota listesi satır prefab'ı (`QrRouteListItem.prefab`) |

## Hierarchy isimleri (Auto Wire)

Yeniden düzenlerseniz bu isimleri koruyun veya `QrFlowUIView` → sağ tık **Auto Wire (standart isimler)** kullanın:

```
QrFlowUI
└── Screens
    ├── WelcomeScreen / ContinueButton
    ├── RouteSelectScreen / Hint, BackButton, RouteScroll/Viewport/Content
    ├── StartConfirmModal / Card / RouteName, Body, Cancel, Start
    ├── QrHubScreen / RouteName, Checkpoint, Target, Next, Status, ScanButton, ChangeRoute
    ├── QrScanScreen / TopBar/Close, TopBar/Hint, BottomBar/Status, BottomBar/BackToHub
    └── ArExperienceScreen / TopBar/Route, BottomBar/Status, QuestButton, HubButton
```

## Kodun güncellediği metinler

- Hub: rota adı, durak, hedef, sonraki durak, durum
- Onay modalı: rota adı, durak sayısı
- Tarama: kamera durumu, QR sonuç mesajı
- AR: rota özeti, deneyim durumu
- Rota listesi: `route_order.json` içinden dinamik satırlar

Buton metinlerini ve görselleri özgürce değiştirebilirsiniz.

## Harici panel

Görev paneli sahnedeki `QuestInteractionPanel` adlı objeyi açar (`QuestInteractionPanelName` alanından değiştirilebilir).
