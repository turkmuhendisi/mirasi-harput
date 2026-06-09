using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public static class ArNpcGroundPlacer
{
    static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    public static bool TryGetGroundPoseForNpc(
        Camera arCamera,
        ARRaycastManager raycastManager,
        float npcHeightMeters,
        float preferredDistanceMeters,
        out Pose npcPose)
    {
        npcPose = default;
        if (arCamera == null)
            return false;

        var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (TryRaycastGround(screenCenter, raycastManager, out var hitPosition))
        {
            npcPose = BuildNpcPose(hitPosition, arCamera, npcHeightMeters);
            return true;
        }

        if (TryRaycastGroundAlongCameraForward(arCamera, raycastManager, preferredDistanceMeters, out hitPosition))
        {
            npcPose = BuildNpcPose(hitPosition, arCamera, npcHeightMeters);
            return true;
        }

        return false;
    }

    static bool TryRaycastGround(Vector2 screenPosition, ARRaycastManager raycastManager, out Vector3 hitPosition)
    {
        hitPosition = default;
        if (raycastManager == null)
            return false;

        Hits.Clear();
        if (!raycastManager.Raycast(screenPosition, Hits, TrackableType.PlaneWithinPolygon))
            return false;

        hitPosition = Hits[0].pose.position;
        return true;
    }

    static bool TryRaycastAlongCameraForward(
        Camera arCamera,
        ARRaycastManager raycastManager,
        float distanceMeters,
        out Vector3 hitPosition)
    {
        hitPosition = default;
        if (arCamera == null)
            return false;

        var worldPoint = arCamera.transform.position + arCamera.transform.forward * distanceMeters;
        var screenPoint = arCamera.WorldToScreenPoint(worldPoint);
        return TryRaycastGround(new Vector2(screenPoint.x, screenPoint.y), raycastManager, out hitPosition);
    }

    static bool TryRaycastGroundAlongCameraForward(
        Camera arCamera,
        ARRaycastManager raycastManager,
        float preferredDistanceMeters,
        out Vector3 hitPosition)
    {
        if (TryRaycastAlongCameraForward(arCamera, raycastManager, preferredDistanceMeters, out hitPosition))
            return true;

        return TryRaycastAlongCameraForward(arCamera, raycastManager, preferredDistanceMeters * 0.65f, out hitPosition);
    }

    static Pose BuildNpcPose(Vector3 groundContactPoint, Camera arCamera, float npcHeightMeters)
    {
        var forward = arCamera != null
            ? Vector3.ProjectOnPlane(arCamera.transform.forward, Vector3.up)
            : Vector3.forward;

        if (forward.sqrMagnitude < 0.0001f)
            forward = Vector3.forward;

        var rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        var position = groundContactPoint + Vector3.up * (npcHeightMeters * 0.5f);
        return new Pose(position, rotation);
    }
}
