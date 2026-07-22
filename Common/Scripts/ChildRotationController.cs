using UnityEngine;

public class ChildRotationController : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAmount = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 constRotationAmount = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 0, 0);
    private Vector3 rot;
    private void OnValidate()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            rot = (rotationAmount * i);
            rot += constRotationAmount;
            
            child.localRotation = Quaternion.Euler(rot);
            child.localPosition = positionOffset * i;
        }
    }
}