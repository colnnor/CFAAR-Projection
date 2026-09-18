using System.Collections.Generic;
using UnityEngine;

public class HorizontalArray : MonoBehaviour
{
    [SerializeField] private float spacing = 1f;
    [SerializeField] private int count;
    [SerializeField] private List<Material> materials = new List<Material>();

    [SerializeField] private ControllableChildGroup childGroup;

    private void OnValidate()
    {
        if (!childGroup.Initialized)
        {
            childGroup.Initialize(transform);
        }

        childGroup.Update(count);

        for (int i = 0; i < childGroup.Count; i++)
        {
            var child = childGroup[i];
            var ogPos = child.localPosition;
            child.localPosition = ogPos.With(x: spacing * i / 100);

            if (materials.Count <= i || !materials[i]) continue;

            if (child.TryGetComponent(out Renderer rend))
            {
                rend.sharedMaterial = materials[i];
            }
        }
    }
}