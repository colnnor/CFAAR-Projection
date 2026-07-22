using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class FloorMapper : Singleton<FloorMapper>
{
    enum MappingMode
    {
        Camera,
        BoxCollider
    }


    [SerializeField] private MappingMode mappingMode = MappingMode.Camera;
    [SerializeField] private Transform floorPlane;
    [ShowIf("mappingMode", MappingMode.Camera)]
    [SerializeField] private Camera floorCamera;
    [ShowIf("mappingMode", MappingMode.BoxCollider)]
    [SerializeField] private BoxCollider floorCollider;

    public virtual Vector3 RemapPosition(Vector3 position)
    {
        Bounds mappedBounds = Bounds();
        float remappedX = Mathf.Lerp(mappedBounds.min.x, mappedBounds.max.x, position.x);
        float remappedY = Mathf.Lerp(mappedBounds.min.y, mappedBounds.max.y, position.y);
        float remappedZ = Mathf.Lerp(mappedBounds.min.z, mappedBounds.max.z, position.z);

        return new Vector3(remappedX, remappedY, remappedZ);
    }
    private void OnValidate()
    {
        SetupBounds();
    }

    void SetupBounds()
    {
        if (mappingMode == MappingMode.BoxCollider && !floorCollider)
        {
            MatchBoxColliderToCamera();
        }
        else if (mappingMode == MappingMode.Camera && floorCollider)
        {
            floorCollider.enabled = false;
        }
    }
    public Bounds Bounds()
    {
        return mappingMode switch
        {
            MappingMode.Camera => CalculateCameraBounds(),
            MappingMode.BoxCollider => floorCollider ? floorCollider.bounds : new Bounds(),
            _ => new Bounds()
        };
    }
    
    Bounds CalculateCameraBounds()
    {
        Bounds cameraBounds = new Bounds();
        if (!floorCamera || !floorPlane) return new Bounds();

        float zDistance = Vector3.Distance(floorCamera.transform.position, floorPlane.position);

        cameraBounds.min = floorCamera.ViewportToWorldPoint(new Vector3(0, 0, zDistance));
        cameraBounds.max = floorCamera.ViewportToWorldPoint(new Vector3(1, 1, zDistance));
        cameraBounds.size = cameraBounds.size.With(y: .1f);

        return cameraBounds;
    }

    [Button]
    void MatchBoxColliderToCamera()
    {
        floorCollider = GetComponent<BoxCollider>();
        if (floorCollider) return;

        floorCollider = gameObject.AddComponent<BoxCollider>();
        floorCollider.isTrigger = true;

        if (!floorCamera) return;
        Bounds cameraBounds = CalculateCameraBounds();
        floorCollider.center = cameraBounds.center;
        floorCollider.size = cameraBounds.size.With(y: 1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Bounds cameraBounds = Bounds();
        Gizmos.DrawWireCube(cameraBounds.center, cameraBounds.size);
        Gizmos.color = Color.mediumOrchid;
        Gizmos.DrawSphere(cameraBounds.min, 1f);
        
        Gizmos.color = Color.mediumSpringGreen;
        Gizmos.DrawSphere(cameraBounds.max, 1f);
    }
}