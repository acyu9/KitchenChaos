using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{

    // Enumeration with options
    private enum Mode {
        LookAt,
        LookAtInverted,
        CameraForward,
        CameraForwardInverted,
    }

    [SerializeField] private Mode mode;
    
    // LateUpdate is after all the updates
    private void LateUpdate() {
        switch (mode) {
            // LookAt is slanted and backwards towards the camera
            case Mode.LookAt:
                // Look straight at the camera
                transform.LookAt(Camera.main.transform);
                break;
            // LookAtInverted is slanted towards the camera
            case Mode.LookAtInverted:
                Vector3 dirFromCamera = transform.position - Camera.main.transform.position;
                transform.LookAt(transform.position + dirFromCamera);
                break;
            // CameraForward is straight towards the camera
            case Mode.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            // CameraForwardInverted is straight and backwards towards the camera
            case Mode.CameraForwardInverted:
                transform.forward = -Camera.main.transform.forward;
                break;
        }
    }

}
