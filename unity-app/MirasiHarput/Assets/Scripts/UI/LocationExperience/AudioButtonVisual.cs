using UnityEngine;

public class AudioButtonVisual : MonoBehaviour
{
    [SerializeField] GameObject barsIconRoot = null;
    [SerializeField] GameObject muteIconRoot = null;

    public void Configure(GameObject barsRoot, GameObject muteRoot)
    {
        barsIconRoot = barsRoot;
        muteIconRoot = muteRoot;
        SetPlaying(false);
    }

    public void SetPlaying(bool isPlaying)
    {
        if (barsIconRoot != null)
            barsIconRoot.SetActive(!isPlaying);

        if (muteIconRoot != null)
            muteIconRoot.SetActive(isPlaying);
    }
}
