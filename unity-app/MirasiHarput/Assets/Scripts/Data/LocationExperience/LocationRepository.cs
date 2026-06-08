using System.Collections.Generic;

public static class LocationRepository
{
    public const string SharedModelPath = "Locations/Models/mirasiharput-model";

    static readonly List<LocationModel> Locations = BuildLocations();
    static readonly Dictionary<string, LocationModel> ById = BuildIdIndex();
    static readonly Dictionary<string, LocationModel> ByQrPayload = BuildPayloadIndex();

    public static IReadOnlyList<LocationModel> All
    {
        get { return Locations; }
    }

    public static bool TryGetById(string locationId, out LocationModel location)
    {
        location = null;
        if (string.IsNullOrEmpty(locationId))
            return false;

        return ById.TryGetValue(locationId, out location);
    }

    public static bool TryGetByQrPayload(string qrPayload, out LocationModel location)
    {
        location = null;
        if (string.IsNullOrEmpty(qrPayload))
            return false;

        var normalized = QrPayloadNormalizer.Normalize(qrPayload);
        if (ByQrPayload.TryGetValue(normalized, out location))
            return true;

        if (QRPayloadParser.TryParse(normalized, out var locationId, out _) &&
            TryGetById(locationId, out location))
            return true;

        return false;
    }

    static List<LocationModel> BuildLocations()
    {
        return new List<LocationModel>
        {
            new LocationModel
            {
                id = "harput_kalesi",
                title = "Harput Kalesi",
                qrPayload = "MIRASI_HARPUT|LOCATION|harput_kalesi|v1",
                modelPath = SharedModelPath,
                audioPath = "Locations/Audio/harput_kalesi",
                description =
                    "Harput Kalesi, Harput'un sarp kayalıkları üzerinde yükselen en önemli tarihî yapılardan biridir. " +
                    "Tarihsel kaynaklara göre kale, MÖ 8. yüzyılda Urartu Krallığı döneminde kurulmuştur. Konumu sayesinde yüzyıllar boyunca savunma, gözetleme ve yerleşim açısından stratejik bir merkez olmuştur." +
                    "Harput; Urartular, Persler, Romalılar, Bizanslılar, Artuklular, Selçuklular ve Osmanlılar gibi birçok medeniyetin izlerini taşır. Halk arasında \"Süt Kalesi\" olarak da bilinen Harput Kalesi, yalnızca bir savunma yapısı değil, aynı zamanda Harput'un binlerce yıllık hafızasını temsil eden güçlü bir semboldür." +
                    "Bugün kalenin surları, taş dokusu ve yüksek konumu ziyaretçiye geçmişin izlerini hissettirir. Burada görülen her taş, Harput'un uzun tarih yolculuğundan bir parça taşır.",
                voiceText =
                    "Harput Kalesi'ne hoş geldiniz. Şu anda Harput'un en güçlü sembollerinden birinin önündesiniz." +
                    "Bu kale, binlerce yıldır bölgenin hafızasını taşır. Urartular döneminde kurulan yapı, zaman içinde birçok medeniyetin izlerini üzerinde toplamıştır." +
                    "Yüksek kayalıklar üzerine kurulu olması, onu yalnızca bir kale değil, aynı zamanda Harput'un koruyucu gözü haline getirmiştir." +
                    "Bugün burada gördüğünüz taşlar, geçmişten bugüne uzanan uzun bir hikâyenin sessiz tanıklarıdır."
            },
            new LocationModel
            {
                id = "urartu_sarnici_zindani",
                title = "Urartu Sarnıcı / Zindanı",
                qrPayload = "MIRASI_HARPUT|LOCATION|urartu_sarnici_zindani|v1",
                modelPath = SharedModelPath,
                audioPath = "Locations/Audio/urartu_sarnici_zindani",
                description =
                    "Şimdi Harput'un yer altında saklı kalan tarihine yaklaşıyorsunuz. Burası Urartu Sarnıcı ve Zindanı. İlk yapıldığında kalenin su ihtiyacını karşılamak için kullanılıyordu." +
                    "Kuşatma dönemlerinde su, bir kalenin hayatta kalması demekti. Bu yüzden sarnıçlar yalnızca mimari yapılar değil, aynı zamanda yaşamın devamını sağlayan stratejik alanlardı." +
                    "Zamanla bu yapı zindan olarak da kullanıldı. Kayaya oyulmuş merdivenleri, derin yapısı ve kapalı atmosferiyle Urartu Sarnıcı / Zindanı, Harput'un yalnızca dışarıdan görünen ihtişamını değil, yer altında saklı kalan tarihini de ziyaretçiye hissettirir." +
                    "Bugün burada, Harput'un hem savunma gücünü hem de yer altında gizlenen tarihini keşfediyorsunuz.",
                voiceText =
                    "Şimdi Harput'un yer altında saklı kalan tarihine yaklaşıyorsunuz." +
                    "Burası Urartu Sarnıcı ve Zindanı. İlk yapıldığında kalenin su ihtiyacını karşılamak için kullanılıyordu." +
                    "Kuşatma dönemlerinde su, bir kalenin hayatta kalması demekti. Bu yüzden sarnıçlar yalnızca mimari yapılar değil, aynı zamanda yaşamın devamını sağlayan stratejik alanlardı." +
                    "Zamanla bu yapı zindan olarak da kullanıldı." +
                    "Bugün burada, Harput'un hem savunma gücünü hem de yer altında gizlenen tarihini keşfediyorsunuz."
            }
        };
    }

    static Dictionary<string, LocationModel> BuildIdIndex()
    {
        var map = new Dictionary<string, LocationModel>();
        for (var i = 0; i < Locations.Count; i++)
        {
            var loc = Locations[i];
            if (loc != null && !string.IsNullOrEmpty(loc.id))
                map[loc.id] = loc;
        }

        return map;
    }

    static Dictionary<string, LocationModel> BuildPayloadIndex()
    {
        var map = new Dictionary<string, LocationModel>();
        for (var i = 0; i < Locations.Count; i++)
        {
            var loc = Locations[i];
            if (loc != null && !string.IsNullOrEmpty(loc.qrPayload))
                map[QrPayloadNormalizer.Normalize(loc.qrPayload)] = loc;
        }

        return map;
    }
}
