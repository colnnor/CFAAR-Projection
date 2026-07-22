using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public enum RotationMapping
{
    None,
    Uniform,
    MatchY,
    Individual
}
public enum PositionMapping
{
    None,
    Uniform,
    XZ,
    XY,
    XZTrackedY
}
public class MappedObject : MonoBehaviour
{
    [SerializeField] private TrackedObject trackedObject;
    [BoxGroup("Position")]
    [SerializeField] private PositionMapping positionMapping = PositionMapping.Uniform;
    
    [BoxGroup("Rotation")]
    [SerializeField] private RotationMapping rotationMapping = RotationMapping.None;

    [BoxGroup("Rotation")]
    [ShowIf("rotationMapping", RotationMapping.Individual)] [SerializeField]
    private bool matchX;

    [BoxGroup("Rotation")]
    [ShowIf("rotationMapping", RotationMapping.Individual)] [SerializeField]
    private bool matchY = false;

    [BoxGroup("Rotation")]
    [ShowIf("rotationMapping", RotationMapping.Individual)] [SerializeField]
    private bool matchZ;


    private void LateUpdate()
    {
        MapPosition();
        MapRotation();
    }

    private void MapRotation()
    {
        switch (rotationMapping)
        {
            case RotationMapping.None:
                return;
            case RotationMapping.Uniform:
                transform.rotation = trackedObject.transform.rotation;
                break;
            case RotationMapping.MatchY:
                Vector3 euler = transform.rotation.eulerAngles;
                euler.y = trackedObject.transform.rotation.eulerAngles.y;
                transform.rotation = Quaternion.Euler(euler);
                break;
            case RotationMapping.Individual:
                Vector3 newEuler = transform.rotation.eulerAngles;
                Vector3 trackedEuler = trackedObject.transform.rotation.eulerAngles;
                if (matchX) newEuler.x = trackedEuler.x;
                if (matchY) newEuler.y = trackedEuler.y;
                if (matchZ) newEuler.z = trackedEuler.z;
                transform.rotation = Quaternion.Euler(newEuler);
                break;
        }
    }

    private void MapPosition()
    {
        if (positionMapping == PositionMapping.None) return;
        FloorMapper mapper = FloorMapper.Instance;
        Vector3 mappedPosition = mapper.RemapPosition(trackedObject.MappedPosition());
        Vector3 newPosition = positionMapping switch
        {
            PositionMapping.Uniform => mappedPosition,
            PositionMapping.XY => transform.position.With(x: mappedPosition.x, y: mappedPosition.y),
            PositionMapping.XZ => mappedPosition.With(y: mapper.transform.position.y),
            PositionMapping.XZTrackedY => mappedPosition.With(y: mapper.Bounds().min.y + trackedObject.localPosition.y),
            _ => transform.position
        };
        transform.position = newPosition;
    }
}