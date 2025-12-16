using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    PlayerMovment pm;

    //public Transform Ggun;
    //Vector3 gunStartPos; 

    [Header("Sens")]
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

    [Header("Cam Effects")]
    public Camera cam;
    public float baseFov;
    public float highFov;
    public float fovSmoothSpeed;
    public float bobAmount;
    public float bobSpeed;
    public float bobTimer;

    private void Start()
    {
        //gunStartPos = Ggun.localPosition;


        pm = GetComponentInParent<PlayerMovment>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cam.fieldOfView = baseFov;
    }

    private void LateUpdate()
    {
        MouseLook();
        CameraFov();
        HeadBob();

        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRot += mouseX;
        xRot -= mouseY;

        xRot = Mathf.Clamp(xRot, -90, 90);

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        transform.localRotation = Quaternion.Euler(xRot, yRot, currentTilt); 
        looking.rotation = Quaternion.Euler(0, yRot, 0); 
    }

    private void MouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRot += mouseX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90, 90);

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);

        transform.localRotation = Quaternion.Euler(xRot, yRot, currentTilt);
        looking.rotation = Quaternion.Euler(0, yRot, 0);
    }

    private void  CameraFov()
    {
        float targetFov = baseFov;

        if (pm.isRunning || pm.grapplning)
        {
            targetFov = highFov;
        }

        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFov, Time.deltaTime * fovSmoothSpeed);
    }

    private void HeadBob()
    {
        if (!pm.isRunning || pm.grapplning)
        {
            cam.transform.localPosition = Vector3.Lerp(
                cam.transform.localPosition,
                Vector3.zero,
                Time.deltaTime * 6f
                );
            return;
        }
        bobTimer += Time.deltaTime * bobSpeed;

        float x = Mathf.Sin(bobTimer) * bobAmount;
        float y = Mathf.Cos(bobTimer * 2f) * bobAmount * 0.5f;

        cam.transform.localPosition = new Vector3(x, y, 0f);
    }

    public void SetWallrunTilt(int direction)
    {
        targetTilt = tiltDegress * direction;
    }
}
