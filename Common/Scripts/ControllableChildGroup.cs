using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class ControllableChildGroup
{
    public Transform parent;

    private List<Transform> children = new List<Transform>();
    [ShowInInspector] public List<Transform> Children => children;
    
    public Transform this[int index] => children[index];
    public int Count => children.Count;
    public bool Initialized => children.Count > 0 && parent != null;

    public void Initialize(Transform parent)
    {
        this.parent = parent;
    }
    public void Update(int newCount)
    {
        if (newCount < 0) newCount = 0;
        if (newCount > 100) newCount = 100;

        children = parent.GetChildren();

        if (children.Count > newCount)
        {
            RemoveChildren(newCount);
        }
        else if (children.Count < newCount)
        {
            AddChildren(newCount);
        }
    }

    private void AddChildren(int newCount)
    {
        Debug.Log($"Adding {newCount} children");
        int childrenCount = children.Count;
        for (int i = childrenCount; i < newCount; i++)
        {
            bool empty = childrenCount == 0;
            Transform childObj;

            if (empty)
            {
                childObj = new GameObject($"Child_{i}").transform;
            }
            else
            {
                // use the first child as a template for instantiation
                var template = children[0];
                childObj = Object.Instantiate(template);
                childObj.localScale = template.localScale; // ensure the scale is the same as the template
                childObj.name = $"{template.name}_{i}"; // use template's name instead of children[i]
            }

            childObj.SetParent(parent);
            childObj.localPosition = Vector3.zero;

            children.Add(childObj);
        }
    }

    private void RemoveChildren(int newCount)
    {
        for (int i = children.Count - 1; i >= newCount; i--)
        {
            if (children[i] == null) continue;
            Object.DestroyImmediate(children[i].gameObject);
            children.RemoveAt(i);
        }
    }
}