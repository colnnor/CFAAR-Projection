using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Klak.Spout;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class CameraRigSetup : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private bool lockChanges = true;
    [SerializeField] private bool useRTs;
    [SerializeField] private bool useSpout;
    [Header("Physical Walls")]
    [SerializeField] private bool createPhysicalWalls = false;
    [SerializeField] private Material physicalWallMaterial;
    [ShowIf("useSpout")]
    [SerializeField] private string spoutPrefix = "spout_";
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

    private const string physicalWallParentName = "WallParent";
    private List<GameObject> physicalWalls = new();
    private Transform physicalWallParent;

    void OnValidate()
    {
        if (lockChanges) return;
        if (numberOfCameras < 1)
        {
            numberOfCameras = 1;
        }

        CreateCameras();
        SetupCameras();
        SetupSpout();
        CreatePhysicalWalls();
        AlignPhysicalWalls();
    }

    void AlignPhysicalWalls()
    {
        if(!createPhysicalWalls) return;
        for (int i = 0; i < numberOfCameras; i++)
        {
            var camera = cameras[i];
            if (camera == null)
            {
                Debug.LogWarning($"Camera at index {i} is null. Skipping alignment for this camera.");
                continue;
            }

            var rend = physicalWalls[i].GetOrAddComponent<Renderer>();
            var meshFilter = physicalWalls[i].GetOrAddComponent<MeshFilter>();
            
            var localCorners = camera.GetCameraFrustumCorners(simulatedGizmosWallDistance);

            Mesh planeMesh = new Mesh
            {
                vertices = new Vector3[]
                {
                    transform.InverseTransformPoint(localCorners[0]),
                    transform.InverseTransformPoint(localCorners[1]),
                    transform.InverseTransformPoint(localCorners[2]),
                    transform.InverseTransformPoint(localCorners[3])
                },
                triangles = new int[]
                {
                    0, 1, 2,
                    0, 2, 3
                }
            };

            planeMesh.RecalculateNormals();
            planeMesh.RecalculateBounds();
            planeMesh.RecalculateTangents();
            
            rend.material = physicalWallMaterial;
            meshFilter.mesh = planeMesh;
            
            var meshCollider = physicalWalls[i].GetOrAddComponent<MeshCollider>();
        }

    }

    void CreatePhysicalWalls()
    {
        if(!createPhysicalWalls)
        {
            foreach (var wall in physicalWalls)
            {
                if (wall != null)
                {
                    DestroyImmediate(wall);
                }
            }
            physicalWallParent.DestroyChildren();
            physicalWalls.Clear();
            return;
        }
        
        if (!physicalWallParent && !transform.TryGetSiblingByName(physicalWallParentName, out physicalWallParent))
        {
            GameObject wallParentObj = new GameObject(physicalWallParentName);
            physicalWallParent = wallParentObj.transform;
            physicalWallParent.SetParent(transform.parent);
            physicalWallParent.localPosition = transform.localPosition;
            physicalWallParent.localRotation = transform.localRotation;
        }
        
        if(physicalWallParent == null) return;
        
        if(physicalWalls.Count > numberOfCameras)
        {
            for (int i = physicalWalls.Count - 1; i >= numberOfCameras; i--)
            {
                DestroyImmediate(physicalWalls[i]);
                physicalWalls.RemoveAt(i);
            }
        }
        else if(physicalWalls.Count < numberOfCameras)
        {
            for (int i = physicalWalls.Count; i < numberOfCameras; i++)
            {
                GameObject wallObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                wallObj.name = $"Wall_{i}";
                wallObj.transform.SetParent(physicalWallParent);
                wallObj.transform.localPosition = Vector3.zero;
                wallObj.transform.localRotation = Quaternion.identity;
                physicalWalls.Add(wallObj);
            }
        }
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

    void SetupSpout()
    {
        foreach (var cam in cameras)
        {
            if (useSpout)
            {
                var sender = cam.gameObject.GetOrAddComponent<SpoutSender>();
                sender.enabled = true;
                sender.captureMethod = CaptureMethod.Texture;
                sender.sourceCamera = cam;
                sender.sourceTexture = cam.targetTexture;
                sender.spoutName = $"{spoutPrefix}{cam.name}";
            }
            else
            {
                if (cam.TryGetComponent(out SpoutSender sender))
                {
                    sender.enabled = false;
                }
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
        if (lockChanges) return;

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

            float yRotation = startAngle + (angleStep * i);
            cam.transform.localRotation = Quaternion.Euler(0, yRotation, 0);

            /*
            cam.fieldOfView = camFOV;
            C = total coverage
            A = angle of the camera
            N = number of cameras
            A = C / (N - 1)
            FOV = C / N + overlap
            */

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