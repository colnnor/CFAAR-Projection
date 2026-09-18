using System;
using System.Collections;
using Klak.Spout;
using UnityEngine;

public class StreamingSetup : MonoBehaviour
{
    [SerializeField] private float setupDelay = 1f;
    [SerializeField] private string streamingPrefix = "Unity_";
    [SerializeField] private StreamingMode streamingMode;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(setupDelay);
        SetupStreaming();
    }
    
    void SetupStreaming()
    {
        var cameras = FindObjectsByType<Camera>();
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
                    spout.captureMethod = tex == null ? CaptureMethod.Camera : CaptureMethod.Texture;
                    spout.sourceTexture = tex;
                    spout.spoutName = streamName;

                    spout.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

}