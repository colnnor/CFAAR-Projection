using System;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ProjectionTrackingArea : Singleton<ProjectionTrackingArea>
{
    [SerializeField] private Collider trackingArea;
    [FoldoutGroup("Debug")] 
    [SerializeField] private bool drawGizmos = true;
    [FoldoutGroup("Debug")] 
    [SerializeField] private float sphereRadius = .25f;
    private void Reset()
    {
        PopulateCollider();
    }

    protected override void Awake()
    {
        base.Awake();
        if (!trackingArea)
        {
            PopulateCollider();
        }
    }

    public Bounds TrackingBounds => trackingArea.bounds;

    private void OnDrawGizmos()
    {
        if (trackingArea == null)
        {
            PopulateCollider();
        }

        Gizmos.color = Color.mediumOrchid;
        Gizmos.DrawSphere(TrackingBounds.min, sphereRadius);

        Gizmos.color = Color.mediumSpringGreen;
        Gizmos.DrawSphere(TrackingBounds.max, sphereRadius);
    }

    private void PopulateCollider()
    {
        trackingArea = GetComponent<Collider>();
    }
}