using UnityEngine;

/// <summary>
/// Dünya uzayındaki sprite/mesh NPC'nin kameraya dönmesini sağlar.
/// </summary>
public class NpcWorldBillboard : MonoBehaviour
{
    Camera targetCamera;

    void LateUpdate()
    {
        ResolveCamera();
        if (targetCamera == null)
            return;

        var toCamera = targetCamera.transform.position - transform.position;
        if (toCamera.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(toCamera, Vector3.up);
    }

    void ResolveCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();
    }
}
