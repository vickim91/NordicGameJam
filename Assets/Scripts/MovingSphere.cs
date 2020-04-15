using System;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{

    public enum AccelerationType
    {
        noAcc, constAcc, LinAccBoost, QuadAccBoost
    }

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 500f)]
    float acceleration = 10f;

    [SerializeField]
    AccelerationType accType = default;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;

    [SerializeField, Min(0f)]
    float probeDistance = 1f;

    [SerializeField, Range(0,1000)]
    int gravityPercentage = 100;

    [SerializeField, Range(0, 100)]
    int airControlPercentage = 10;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0f, 90f)]
    float groundedAngle = 20f;

    Vector3 velocity, desiredVelocity;
    Vector3 contactNormal;

    float startAcceleration;

    int stepsSinceLastGrounded, stepsSinceLastJump;

    Rigidbody body;

    bool desiredJump, onGround;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        startAcceleration = acceleration;
        Physics.gravity = -Vector3.up * 9.81f*gravityPercentage/100;
    }

    void Update()
    {
        Vector2 playerInput;
        playerInput.x = Input.GetAxisRaw("Horizontal");
        playerInput.y = Input.GetAxisRaw("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        desiredJump |= Input.GetButtonDown("Jump");
    }

    private void FixedUpdate()
    {
        MoveSphere();


        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        UpdateState();

        if (onGround)
        {
            GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);
        }
        else
        {
            GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
        }

        ClearState();
        
    }

    private void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;

        body.velocity = velocity;

        if (onGround)
        {
            stepsSinceLastGrounded = 0;
        }
        else
        {
            SnapToGround();
            contactNormal = Vector3.up;
        }
    }

    private void MoveSphere()
    {
        velocity = body.velocity;

        float accelerationScaled = 0;

        acceleration = onGround ? startAcceleration : startAcceleration * airControlPercentage / 100;

        switch (accType)
        {
            case AccelerationType.constAcc:
                accelerationScaled = acceleration * Time.deltaTime;
                break;
            case AccelerationType.LinAccBoost:
                accelerationScaled = acceleration * (0.1f + 5 * Vector3.Distance(desiredVelocity, velocity) / (maxSpeed * 2)) * Time.deltaTime;
                break;
            case AccelerationType.QuadAccBoost:
                accelerationScaled = 5 * acceleration * (0.1f + 10 * Mathf.Pow(Vector3.Distance(desiredVelocity, velocity) / (maxSpeed * 2), 4)) * Time.deltaTime;
                break;
            case AccelerationType.noAcc:
                accelerationScaled = 0;
                break;
        }

        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentXVelocity = Vector3.Dot(velocity, xAxis);
        float currentZVelocity = Vector3.Dot(velocity, zAxis);

        if (accType == AccelerationType.noAcc)
        {
            float oldY = velocity.y;
            velocity = xAxis * desiredVelocity.x + zAxis * desiredVelocity.z;
            velocity += new Vector3(0, oldY, 0);
        }
        else
        {
            float newXVelocity = Mathf.MoveTowards(currentXVelocity, desiredVelocity.x, accelerationScaled);
            float newZVelocity = Mathf.MoveTowards(currentZVelocity, desiredVelocity.z, accelerationScaled);

            velocity += xAxis * (newXVelocity - currentXVelocity) + zAxis * (newZVelocity - currentZVelocity);
        }
    }

    private void ClearState()
    {

        onGround = false;
        contactNormal = Vector3.zero;
    }

    void Jump()
    {
        if (onGround)
        {
            stepsSinceLastJump = 0;
            velocity += contactNormal*Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        }
    }
   
    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;

            
            float cosTheta = Vector3.Dot(normal, Vector3.up) -0.00001f;
            float theta = Mathf.Acos(cosTheta);
            theta = theta * 360 / (2 * Mathf.PI);

            if (theta <= groundedAngle)
            {
                onGround = true;
                contactNormal += normal;
            }
        }
        contactNormal.Normalize();
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    void SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 10)
        {
            return;
        }
        float speed = velocity.magnitude;
        if (speed > (maxSnapSpeed+1)/100*maxSpeed)
        {
            return;
        }
        if (!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance))
        {
            return;
        }
        if (Mathf.Acos((Vector3.Dot(hit.normal, Vector3.up) - 0.00001f) * 360 / (2 * Mathf.PI)) > groundedAngle)
        {
            return;
        }

        onGround = true;
        contactNormal = hit.normal;
        
        float dot = Vector3.Dot(velocity, hit.normal);

        if (dot > 0f)
        {
            print("snap NOW!");
            body.velocity = (velocity - hit.normal * dot).normalized * speed;
        }
    }
}
