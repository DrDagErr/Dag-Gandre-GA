using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform looking;

    float xRot;
    float yRot;

    [Header("Wallrun tilt")]
    public float tiltDegress = 15f;
    public float tiltSpeed = 5f;
    private float currentTilt;
    private float targetTilt; 

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
    }

    private void LateUpdate()
    {
        //mouse input

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRot += mouseX;
        xRot -= mouseY;

        xRot = Mathf.Clamp(xRot, -90, 90);

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        //rotate cam
        transform.localRotation = Quaternion.Euler(xRot, yRot, currentTilt); 
        looking.rotation = Quaternion.Euler(0, yRot, 0); 
    }

    public void SetWallrunTilt(int direction)
    {
        targetTilt = tiltDegress * direction;
    }
}
