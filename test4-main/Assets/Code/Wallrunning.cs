using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallrunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private float horizantalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Referencs")]
    public Transform orientation;
    private PlayerMovment pm;
    private Rigidbody rb;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovment>(); 
    }

    private void Update()
    {
        CheckForWall(); 
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft  = Physics.Raycast(transform.position, orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround); 
    }

    private void StateMachine()
    {
        horizantalInput = Input.GetAxisRaw("Horizantal");
        verticalInput = Input.GetAxisRaw("Vertical"); 

        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            //test3

        }
    }
}
