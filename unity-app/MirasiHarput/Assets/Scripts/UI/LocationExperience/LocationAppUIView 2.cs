using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LocationAppScreen
{
    QrReader = 0,
    LocationExperience = 1
}

[DisallowMultipleComponent]
public class LocationAppUIView : MonoBehaviour
{
    public const string PrefabResourcesPath = "UI/LocationExperience/LocationExperienceUI";

    [Header("Ekranlar")]
    [SerializeField] GameObject qrReaderScreen = null;
    [SerializeField] GameObject locationExperienceScreen = null;

    [Header("QR Reader")]
    [SerializeField] Button scanButton = null;
    [SerializeField] RawImage cameraPreview = null;
    [SerializeField] TMP_Text scanHintText = null;
    [SerializeField] TMP_Text scanStatusText = null;

    [Header("Mekan deneyimi")]
    [SerializeField] Button backButton = null;
    [SerializeField] Button audioToggleButton = null;
    [SerializeField] AudioButtonVisual audioButtonVisual = null;
    [SerializeField] TMP_Text descriptionText = null;
    [SerializeField] ScrollRect descriptionScrollRect = null;

    [Header("Toast")]
    [SerializeField] LocationToast toast = null;

    public GameObject QrReaderScreen
    {
        get { return qrReaderScreen; }
    }

    public GameObject LocationExperienceScreen
    {
        get { return locationExperienceScreen; }
    }

    public Button ScanButton
    {
        get { return scanButton; }
    }

    public RawImage CameraPreview
    {
        get { return cameraPreview; }
    }

    public Button BackButton
    {
        get { return backButton; }
    }

    public Button AudioToggleButton
    {
        get { return audioToggleButton; }
    }

    public TMP_Text ScanHintText
    {
        get { return scanHintText; }
    }

    public TMP_Text ScanStatusText
    {
        get { return scanStatusText; }
    }

    public AudioButtonVisual AudioButtonVisual
    {
        get { return audioButtonVisual; }
    }

    public TMP_Text DescriptionText
    {
        get { return descriptionText; }
    }

    public ScrollRect DescriptionScrollRect
    {
        get { return descriptionScrollRect; }
    }

    public LocationToast Toast
    {
        get { return toast; }
    }

    public bool HasRequiredScreens()
    {
        return qrReaderScreen != null && locationExperienceScreen != null;
    }

    public void SetAudioButtonActiveState(bool isAudioPlaying)
    {
        if (audioButtonVisual == null && audioToggleButton != null)
            audioButtonVisual = audioToggleButton.GetComponent<AudioButtonVisual>();

        if (audioButtonVisual != null)
            audioButtonVisual.SetPlaying(isAudioPlaying);
    }

    public void ShowToast(string message)
    {
        if (toast != null)
            toast.Show(message);
    }

#if UNITY_EDITOR
    [ContextMenu("Auto Wire")]
    void AutoWireContextMenu()
    {
        AutoWireByName();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    public void AutoWireByName()
    {
        qrReaderScreen = FindChild(qrReaderScreen, "QrReaderScreen");
        locationExperienceScreen = FindChild(locationExperienceScreen, "LocationExperienceScreen");

        scanButton = FindButton(scanButton, qrReaderScreen, "ScanButton");
        cameraPreview = FindRawImage(cameraPreview, qrReaderScreen, "ScanFrame/CameraPreview");
        scanHintText = FindText(scanHintText, qrReaderScreen, "ScanFrame/Hint");
        scanStatusText = FindText(scanStatusText, qrReaderScreen, "StatusText");

        backButton = FindButton(backButton, locationExperienceScreen, "BackButton");
        audioToggleButton = FindButton(audioToggleButton, locationExperienceScreen, "InfoPanel/AudioButton");
        if (audioToggleButton != null)
            audioButtonVisual = audioToggleButton.GetComponent<AudioButtonVisual>();
        descriptionText = FindText(descriptionText, locationExperienceScreen, "InfoPanel/DescriptionScroll/Viewport/Content/Description");
        descriptionScrollRect = FindScroll(descriptionScrollRect, locationExperienceScreen, "InfoPanel/DescriptionScroll");
        toast = FindToast(toast);
    }

    GameObject FindChild(GameObject current, string screenName)
    {
        if (current != null)
            return current;

        var found = transform.Find("Screens/" + screenName);
        if (found == null)
            found = transform.Find(screenName);

        return found != null ? found.gameObject : null;
    }

    LocationToast FindToast(LocationToast current)
    {
        if (current != null)
            return current;

        return GetComponentInChildren<LocationToast>(true);
    }

    static ScrollRect FindScroll(ScrollRect current, GameObject root, string path)
    {
        if (current != null)
            return current;

        if (root == null)
            return null;

        var t = root.transform.Find(path);
        return t != null ? t.GetComponent<ScrollRect>() : null;
    }

    static TMP_Text FindText(TMP_Text current, GameObject root, string path)
    {
        if (current != null)
            return current;

        if (root == null)
            return null;

        var t = root.transform.Find(path);
        return t != null ? t.GetComponent<TMP_Text>() : null;
    }

    static Button FindButton(Button current, GameObject root, string path)
    {
        if (current != null)
            return current;

        if (root == null)
            return null;

        var t = root.transform.Find(path);
        return t != null ? t.GetComponent<Button>() : null;
    }

    static RawImage FindRawImage(RawImage current, GameObject root, string path)
    {
        if (current != null)
            return current;

        if (root == null)
            return null;

        var t = root.transform.Find(path);
        return t != null ? t.GetComponent<RawImage>() : null;
    }
}
