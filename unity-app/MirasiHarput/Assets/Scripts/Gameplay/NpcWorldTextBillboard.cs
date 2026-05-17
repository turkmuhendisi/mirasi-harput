using UnityEngine;

/// <summary>
/// Dünya uzayında TextMesh etiketinin kameraya dönmesini sağlar.
/// </summary>
public class NpcWorldTextBillboard : MonoBehaviour
{
    Camera targetCamera;

    void LateUpdate()
    {
        ResolveCamera();
        if (targetCamera == null)
            return;

        var toCamera = targetCamera.transform.position - transform.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(toCamera.normalized, Vector3.up);
    }

    void ResolveCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }
}
