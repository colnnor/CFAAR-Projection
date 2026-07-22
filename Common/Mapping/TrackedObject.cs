using System;
using UnityEngine;

public class TrackedObject : MonoBehaviour
{
    public Vector3 position => transform.position;
    public Vector3 localPosition => transform.localPosition;
    public Vector3 Velocity { get; private set; }
    private Vector3 lastPosition;
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    public void SetNormal(Vector3 normal)
    {
        transform.up = normal;
    }
    /// <summary>
    /// returns the position remapped to a 0-1 range based on the tracking area's bounds
    /// </summary>
    /// <returns></returns>
    public Vector3 MappedPosition()
    {
        TrackingArea trackingArea = TrackingArea.Instance;
        float remappedX = Mathf.InverseLerp(trackingArea.TrackingBounds.min.x, trackingArea.TrackingBounds.max.x, position.x);
        float remappedZ = Mathf.InverseLerp(trackingArea.TrackingBounds.min.z, trackingArea.TrackingBounds.max.z, position.z);
        
        return new Vector3(remappedX, transform.position.y, remappedZ);
    }

    private void Update()
    {
        Velocity = (position - lastPosition) / Time.deltaTime;
        lastPosition = position;
    }
}

