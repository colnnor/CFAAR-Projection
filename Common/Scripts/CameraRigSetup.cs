using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Klak.Spout;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public enum StreamingMode
{
    None,
    Spout
}

public class CameraRigSetup : MonoBehaviour
{
    [FormerlySerializedAs("lockChanges")]
    [Header("Options")]
    [SerializeField] private bool lockPositionalChanges = true;
    [SerializeField] private bool useRTs;
    [Header("Streaming")]
    [SerializeField] private StreamingMode streamingMode;
    [HideIf("streamingMode", StreamingMode.None)]
    [SerializeField] private string streamingPrefix = "streaming_";
    [Header("Settings")]
    [SerializeField] private int numberOfCameras = 6;
    [SerializeField] private float camRotationOffset = 0f;
    [SerializeField] private float camFOV = 40;
    [SerializeField] private int maxDegrees = 360;
    [SerializeField] private float overlap;
    [Header("Render Texture Settings")]
    [SerializeField] private Vector2Int rtResolution = new Vector2Int(1920, 1080);
    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private float simulatedGizmosWallDistance = 5;
    [SerializeField] private float gizmosAlpha = 1f;
    [Header("References")]
    [SerializeField] List<Camera> cameras = new List<Camera>();
    [SerializeField] private List<RenderTexture> rts;

    void OnValidate()
    {
        if (numberOfCameras < 1)
        {
            numberOfCameras = 1;
        }

        if (!lockPositionalChanges)
        {
            CreateCameras();
            SetupCameras();
        }

        SetupStreaming();
    }

    [Button]
    void UpdateRenderTextureResolutions()
    {
        foreach (var renderTexture in rts)
        {
            renderTexture.width = rtResolution.x;
            renderTexture.height = rtResolution.y;
        }

        StartCoroutine(RefreshCameras());
    }

    IEnumerator RefreshCameras()
    {
        foreach (var cam in cameras)
        {
            cam.enabled = false;
            yield return null;
            cam.enabled = true;
        }
    }

    void SetupStreaming()
    {
        foreach (var cam in cameras)
        {
            if (cam.TryGetComponent(out SpoutSender spout)) spout.enabled = false;

            var source = cam;
            var tex = cam.targetTexture;
            var streamName = $"{streamingPrefix}{cam.name}";

            switch (streamingMode)
            {
                case StreamingMode.None:
                    break;
                case StreamingMode.Spout:
                    if (!spout)
                    {
                        spout = cam.gameObject.AddComponent<SpoutSender>();
                    }

                    spout.captureMethod = CaptureMethod.Texture;
                    spout.sourceCamera = source;
                    spout.sourceTexture = tex;
                    spout.spoutName = streamName;

                    spout.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    void CreateCameras()
    {
        foreach (var cam in cameras.ToList().Where(cam => cam == null))
        {
            cameras.Remove(cam);
        }

        if (cameras.Count > numberOfCameras)
        {
            for (int i = cameras.Count - 1; i >= numberOfCameras; i--)
            {
                DestroyImmediate(cameras[i].gameObject);
                cameras.RemoveAt(i);
            }
        }
        else if (cameras.Count < numberOfCameras)
        {
            for (int i = cameras.Count; i < numberOfCameras; i++)
            {
                GameObject camObj = new GameObject($"Camera_{i}");
                Camera cam = camObj.AddComponent<Camera>();
                cam.transform.SetParent(transform);
                cam.transform.localPosition = Vector3.zero;

                cameras.Add(cam);
            }
        }

        foreach (var child in transform.Children().Select(c => c.gameObject))
        {
            if (cameras.Contains(child.GetComponent<Camera>()) == false)
            {
                DestroyImmediate(child);
            }
        }
    }

    [Button("Update Cameras")]
    private void SetupCameras()
    {
        float angleStep = (float)maxDegrees / (numberOfCameras);
        float startAngle = -angleStep * (numberOfCameras - 1) / 2;

        for (int i = 0; i < numberOfCameras; i++)
        {
            Camera cam = cameras[i];
            var camName = $"Camera_{i}";
            if (cam.name != camName)
            {
                cam.name = camName;
            }

            if (!lockPositionalChanges)
            {
                float yRotation = startAngle + (angleStep * i);
                cam.transform.localRotation = Quaternion.Euler(0, yRotation, 0);
            }

            camFOV = ((float)maxDegrees / (numberOfCameras)) + overlap;
            cam.fieldOfView = camFOV;

            if (!useRTs)
            {
                cam.targetTexture = null;
                continue;
            }

            if (i < rts.Count)
            {
                cam.targetTexture = rts[i];
            }
            else
            {
                RenderTexture rt = new RenderTexture(rts[0].width, rts[0].height, rts[0].depth, rts[0].format);
                cam.targetTexture = rt;
                rts.Add(rt);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.darkOliveGreen.WithAlpha(gizmosAlpha);
        foreach (var cam in cameras)
        {
            var frustumCorners = cam.GetCameraFrustumCorners(simulatedGizmosWallDistance);

            for (int i = 0; i < frustumCorners.Length; i++)
            {
                //draw lines between the corners
                Gizmos.DrawLine(frustumCorners[i], frustumCorners[(i + 1) % frustumCorners.Length]);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || cameras == null || cameras.Count == 0) return;
        Gizmos.color = Color.red.WithAlpha(gizmosAlpha);
        foreach (var cam in cameras)
        {
            DrawCameraFrustum(cam);
        }
    }

    void DrawCameraFrustum(Camera cam)
    {
        if (cam == null) return;

        float fov = cam.fieldOfView;
        float aspect = cam.aspect;
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;

        Vector3[] frustumCorners = cam.GetCameraFrustumCorners(near);
        Vector3[] farCorners = cam.GetCameraFrustumCorners(far);

        for (int i = 0; i < frustumCorners.Length; i++)
        {
            //draw lines between the corners
            Gizmos.DrawLine(frustumCorners[i], frustumCorners[(i + 1) % frustumCorners.Length]);
            Gizmos.DrawLine(farCorners[i], farCorners[(i + 1) % farCorners.Length]);
            Gizmos.DrawLine(frustumCorners[i], farCorners[i]);
        }
    }
}