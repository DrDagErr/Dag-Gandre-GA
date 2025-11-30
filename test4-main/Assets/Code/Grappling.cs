using UnityEngine;

public class Grappling : MonoBehaviour
{
    [Header("References")]
    public LineRenderer lr;
    public LayerMask canGrapple;
    public Transform gunTip, cam, player;
    public PlayerMovment pm;
    public Transform looking;
    public Rigidbody rb;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 25f;
    private Vector3 grapplePoint;
    private SpringJoint joint;

    [Header("Swinging Forces")]
    public float horizontalForce;
    public float forwardForce;
    public float autoForwardForce;
    public float extendCableSpeed;

    [Header("Prediction")]
    public float predictBallRadius = 4f;
    public Transform predictPoint;
    public RaycastHit predictHit;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse0;

    private Vector3 currentGrapplePosition;

    private void Update()
    {
        CheckForGrapplePoints();

        if (Input.GetKeyDown(grappleKey))
        {
            StartGrapple();
        }

        if (Input.GetKeyUp(grappleKey))
        {
            StopGrapple();
        }
    }

    private void FixedUpdate()
    {
        if (joint != null)
        {
            GearMovement();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void CheckForGrapplePoints()
    {
        if (joint != null)
        {
            return;
        }

        RaycastHit sphereHit;
        Physics.SphereCast(
            cam.position,
            predictBallRadius,
            cam.forward,
            out sphereHit,
            maxGrappleDistance,
            canGrapple
        );

        RaycastHit rayHit;
        Physics.Raycast(
            cam.position,
            cam.forward,
            out rayHit,
            maxGrappleDistance,
            canGrapple
        );

        Vector3 bestPoint = Vector3.zero;

        if (rayHit.point != Vector3.zero)
        {
            bestPoint = rayHit.point;
        }
        else if (sphereHit.point != Vector3.zero)
        {
            bestPoint = sphereHit.point;
        }

        if (bestPoint != Vector3.zero)
        {
            predictPoint.position = bestPoint;
            predictPoint.gameObject.SetActive(true);
        }
        else
        {
            predictPoint.gameObject.SetActive(false);
        }

        predictHit = (rayHit.point != Vector3.zero) ? rayHit : sphereHit;
    }

    private void StartGrapple()
    {
        if (predictHit.point == Vector3.zero)
        {
            return;
        }

        pm.grapplning = true;

        grapplePoint = predictHit.point;

        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;

        currentGrapplePosition = gunTip.position;
        lr.positionCount = 2;
    }

    public void StopGrapple()
    {
        pm.grapplning = false;
        lr.positionCount = 0;

        if (joint != null)
        {
            Destroy(joint);
        }
    }

    private void GearMovement()
    {
        Vector3 autoDir = (grapplePoint - transform.position).normalized;
        rb.AddForce(autoDir * autoForwardForce * Time.deltaTime);

        if (Input.GetKey(KeyCode.D))
        {
            rb.AddForce(looking.right * horizontalForce * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            rb.AddForce(-looking.right * horizontalForce * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(looking.forward * horizontalForce * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 dir = (grapplePoint - transform.position).normalized;
            rb.AddForce(dir * forwardForce * Time.deltaTime);

            float dist = Vector3.Distance(transform.position, grapplePoint);
            joint.maxDistance = dist * 0.8f;
            joint.minDistance = dist * 0.25f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            float dist = Vector3.Distance(transform.position, grapplePoint) + extendCableSpeed;
            joint.maxDistance = dist * 0.8f;
            joint.minDistance = dist * 0.25f;
        }
    }

    private void DrawRope()
    {
        if (joint == null)
        {
            return;
        }

        currentGrapplePosition = Vector3.Lerp(
            currentGrapplePosition,
            grapplePoint,
            Time.deltaTime * 8f
        );

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }
}
