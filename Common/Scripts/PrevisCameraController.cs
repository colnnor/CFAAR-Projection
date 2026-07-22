using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PrevisCameraController : MonoBehaviour
{
    [SerializeField] private List<CinemachineCamera> cameras = new List<CinemachineCamera>();
    
    private int currentCameraIndex = 0;
    
    void Start()
    {
        ActivateCamera(currentCameraIndex);
    }

    private void Update()
    {
        if(Keyboard.current.shiftKey.isPressed && Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            NextCamera();
        }
    }

    void NextCamera()
    {
        currentCameraIndex = (currentCameraIndex + 1) % cameras.Count;
        ActivateCamera(currentCameraIndex);
    }
    void ActivateCamera(int index)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].gameObject.SetActive(i == index);
        }
    }
}