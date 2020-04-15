using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    public enum RotationType
    {
        FreeRotation, AlignRotation
    }

    public enum InnerSmoothingType
    {
        DontFollow, Linear, Exponential, Spring, DampedSpring, Instant
    }

    public enum OuterSmoothingType
    {
        Linear, Exponential, Spring, DampedSpring, Instant
    }


    [SerializeField]
    Transform testObj;

    public bool useTestObject;

    [SerializeField]
    Transform player = default;

    [SerializeField]
    InnerSmoothingType smoothTypeInnerCircle = default;

    [SerializeField]
    OuterSmoothingType smoothTypeOuterCircle = default;

    [SerializeField, Range(1f, 20f)]
    float distanceToPlayer = 5f;

    [SerializeField, Min(0f)]
    float circleRadius = 1f;

    [SerializeField, Range(0, 10f)]
    float innerFollowSpeed = 5f;

    [SerializeField, Range(0, 10f)]
    float outerFollowSpeed = 5f;

    Vector3 currentLookPoint, nextLookPoint, velocity;

    bool inInner = true;
    float circleStartRadius;

    void Awake()
    {
        currentLookPoint = player.position;
        if (!useTestObject)
        {
            testObj.gameObject.SetActive(false);
        }
        circleStartRadius = circleRadius;
    }

    void LateUpdate()
    {

        Vector3 playerPos = player.position;
        float dt = Time.deltaTime;

        UpdateLookPoint(playerPos, dt);

        if (useTestObject)
            testObj.transform.position = nextLookPoint;

        MoveCamera();

        currentLookPoint = nextLookPoint;



        //UpdateFocusPoint();
        //Vector3 lookDirection = transform.forward;
        //transform.localPosition = currentlyLookingAt - lookDirection * distanceToPlayer;
    }

    private void MoveCamera()
    {
        Vector3 lookDirection = transform.forward;

        transform.localPosition = nextLookPoint - lookDirection * distanceToPlayer;
    }

    private void UpdateLookPoint(Vector3 playerPos, float dt)
    {
        float lookPointToPlayerDist = Vector3.Distance(playerPos, currentLookPoint);

        if(lookPointToPlayerDist <= circleRadius)
        {
            //circleRadius = 1.4f* circleStartRadius;

            dt *= innerFollowSpeed;

            switch (smoothTypeInnerCircle)
            {
                case InnerSmoothingType.DontFollow:
                    velocity = Vector3.zero;
                    break;

                case InnerSmoothingType.Linear:
                    velocity = (playerPos - currentLookPoint).normalized;
                    break;

                case InnerSmoothingType.Exponential:
                    velocity = playerPos - currentLookPoint;
                    break;

                case InnerSmoothingType.Spring:
                    var n1 = velocity - (currentLookPoint - playerPos) * (innerFollowSpeed * dt);
                    var n2 = 1 + dt;
                    velocity = n1 / (n2 * n2);
                    break;

                case InnerSmoothingType.DampedSpring:
                    var n3 = velocity - (currentLookPoint - playerPos) * (innerFollowSpeed * innerFollowSpeed * dt);
                    var n4 = 1 + innerFollowSpeed * dt;
                    velocity = n3 / (n4 * n4);
                    break;
            }

            if (smoothTypeInnerCircle != InnerSmoothingType.Instant)
            {
                if (velocity.magnitude * dt < Vector3.Distance(playerPos, currentLookPoint))
                {
                    nextLookPoint = currentLookPoint + velocity * dt;
                }
                else
                {
                    nextLookPoint = playerPos;
                }
            }
            else
            {
                nextLookPoint = playerPos;
            }
        }
        else
        {
            //circleRadius = 0.7f * circleStartRadius;
            dt *= outerFollowSpeed;
            switch (smoothTypeOuterCircle)
            {
                
                case OuterSmoothingType.Linear:
                    velocity = (playerPos - currentLookPoint).normalized;
                    break;

                case OuterSmoothingType.Exponential:
                    velocity = playerPos - currentLookPoint;
                    break;

                case OuterSmoothingType.Spring:
                    var n1 = velocity - (currentLookPoint - playerPos) * (innerFollowSpeed * dt);
                    var n2 = 1 + dt;
                    velocity = n1 / (n2 * n2);
                    break;

                case OuterSmoothingType.DampedSpring:
                    var n3 = velocity - (currentLookPoint - playerPos) * (innerFollowSpeed * innerFollowSpeed * dt);
                    var n4 = 1 + innerFollowSpeed * dt;
                    velocity = n3 / (n4 * n4);
                    break;
            }
            if (smoothTypeOuterCircle != OuterSmoothingType.Instant)
            {
                nextLookPoint = currentLookPoint + velocity * dt;
            }
            else
            {
                nextLookPoint = Vector3.Lerp(playerPos, currentLookPoint, circleRadius / lookPointToPlayerDist);
            }

        }
    }

    void UpdateFocusPoint()
    {
        Vector3 playerPos = player.position;

        // exp + relaxation:
        /*
        if (relaxationRadius > 0f)
        {
            float distance = Vector3.Distance(playerPos, currentlyLookingAt);

            if (distance > relaxationRadius)
            {
                currentlyLookingAt = Vector3.Lerp(playerPos, currentlyLookingAt, relaxationRadius / distance);
            }
            if (distance > 0.01f)
            {

                currentlyLookingAt = Vector3.Lerp(playerPos, currentlyLookingAt,Mathf.Pow(0.5f, Time.deltaTime/distanceHalfLife));
            }
        }
        */
        //
        if (circleRadius > 0f)
        {
            float distance = Vector3.Distance(playerPos, currentLookPoint);

            if (distance > circleRadius)
            {
                currentLookPoint = Vector3.Lerp(playerPos, currentLookPoint, circleRadius / distance);
            }
            if (distance > 0.01f)
            {
                float distFrac = 1 - distance / circleRadius;
                

                float lerpFrac = Time.deltaTime;

                //currentlyLookingAt = Vector3.Lerp(currentlyLookingAt, playerPos + (playerPos - currentlyLookingAt)*0.8f, lerpFrac);
                currentLookPoint = Vector3.Lerp(currentLookPoint, playerPos + (playerPos - currentLookPoint) * 0.8f, lerpFrac);
            }
        }

    }
}