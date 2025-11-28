using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    public LineRenderer Lr;
    public LayerMask canGrapple;
    public Transform gunTip, cam, player;
    public PlayerMovmentGrappling pm;

    [Header("Gear")]
    public Transform looking;
    public Rigidbody rb;
    public float horizontalForce;
    public float forwardForce;
    public float extendCableSpeed;

    [Header("Prediction")]
    public RaycastHit predictHit;
    public float predictBallRadius;
    public Transform predictPoint;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse0;

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }
        if (Input.GetKeyUp(grappleKey))
        {
            StopGrapple();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void CheckForGrapplePoins()
    {
        if (joint != null)
        {
            return;
        }
    }
}
