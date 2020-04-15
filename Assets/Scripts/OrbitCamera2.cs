using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera2 : MonoBehaviour
{
   

    public enum InnerSmoothingType
    {
        DontFollow, Exponential, Spring, DampedSpring, Instant
    }

    public enum OuterSmoothingType
    {
        Exponential, Spring, DampedSpring, Instant
    }

    [SerializeField]
    public Transform player = default;

    [SerializeField, Range(1f, 20f)]
    public float distanceToPlayer = 5f;

    [SerializeField]
    public bool useTwoSmoothZones;

    [SerializeField]
    public InnerSmoothingType smoothTypeInnerCircle = default;

    [SerializeField]
    public OuterSmoothingType smoothTypeOuterCircle = default;

    [SerializeField, Min(0f)]
    public float circleRadius = 1f;

    [SerializeField, Range(0, 10f)]
    public float innerFollowSpeed = 5f;

    [SerializeField, Range(0, 10f)]
    public float outerFollowSpeed = 5f;

    [SerializeField, Range(1f, 360f)]
    public float rotationSpeed = 90f;

    Vector3 currentLookPoint, nextLookPoint, velocity;

    float circleStartRadius;

    Vector2 orbitAngles = new Vector2(45f, 0f);

    void Awake()
    {
        currentLookPoint = player.position;
        circleStartRadius = circleRadius;

        if (!useTwoSmoothZones)
        {
            switch (smoothTypeInnerCircle)
            {
                case InnerSmoothingType.DontFollow:
                    smoothTypeOuterCircle = OuterSmoothingType.Instant;
                    break;
                case InnerSmoothingType.Exponential:
                    smoothTypeOuterCircle = OuterSmoothingType.Exponential;
                    break;
                case InnerSmoothingType.Spring:
                    smoothTypeOuterCircle = OuterSmoothingType.Spring;
                    break;
                case InnerSmoothingType.DampedSpring:
                    smoothTypeOuterCircle = OuterSmoothingType.DampedSpring;
                    break;
                case InnerSmoothingType.Instant:
                    smoothTypeOuterCircle = OuterSmoothingType.Instant;
                    break;
            }

            outerFollowSpeed = innerFollowSpeed;
            if(smoothTypeInnerCircle == InnerSmoothingType.DontFollow)
            {
                Debug.Log("CANT USE DONT FOLLOW WITH ONLY ONE SMOOTH ZONE");
            }
        }
    }

    void LateUpdate()
    {
        Vector3 playerPos = player.position;
        float dt = Time.deltaTime;

        Vector3[] twoPoints = UpdateLookPoint(playerPos, smoothTypeInnerCircle, smoothTypeOuterCircle);

        float dist = Vector3.Distance(playerPos, currentLookPoint);


        if(smoothTypeInnerCircle == InnerSmoothingType.DontFollow && smoothTypeOuterCircle == OuterSmoothingType.Instant)
        {
            if(dist <= circleRadius)
            {
                circleRadius = circleStartRadius;
                nextLookPoint = twoPoints[0];
            }
            else
            {
                circleRadius = circleStartRadius;
                nextLookPoint = twoPoints[1];
            }

        }
        else if(smoothTypeInnerCircle == InnerSmoothingType.DontFollow)
        {
            if (dist <= circleRadius)
            {
                circleRadius = circleStartRadius*1.3f;
                nextLookPoint = twoPoints[0];
            }
            else
            {
                circleRadius = circleStartRadius*0.8f;
                nextLookPoint = twoPoints[1];
            }

        }
        else
        {
            float frac = dist / circleRadius;

            frac = Mathf.Clamp(frac, circleRadius * 0.7f, circleRadius * 1.4f);
            frac = (frac - 0.7f * circleRadius) / (circleRadius * 0.7f);

            nextLookPoint = Vector3.Lerp(twoPoints[0], twoPoints[1], frac);
        }

       
        MoveCamera();

        currentLookPoint = nextLookPoint;

    }

    private void MoveCamera()
    {
        Quaternion lookRotation = Quaternion.Euler(orbitAngles);
        
        Vector3 lookDirection = lookRotation * Vector3.forward;

        //transform.localPosition = nextLookPoint - lookDirection * distanceToPlayer;
        Vector3 lookPosition = nextLookPoint - lookDirection * distanceToPlayer;
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    private Vector3[] UpdateLookPoint(Vector3 playerPos, InnerSmoothingType it, OuterSmoothingType ot)
    {
        float lookPointToPlayerDist = Vector3.Distance(playerPos, currentLookPoint);
        float dt = Time.deltaTime;

        Vector3[] outPut = new Vector3[2];

            switch (it)
            {
                case InnerSmoothingType.DontFollow:
                    velocity = Vector3.zero;
                    break;

                case InnerSmoothingType.Exponential:
                    velocity = playerPos - currentLookPoint;
                    break;

                case InnerSmoothingType.Spring:
                    var n1 = velocity - (currentLookPoint - playerPos) * (innerFollowSpeed * dt *innerFollowSpeed);
                    var n2 = 1 + dt;
                    velocity = n1 / (n2 * n2);
                    break;

                case InnerSmoothingType.DampedSpring:
                    var n3 = velocity - (currentLookPoint - playerPos) * (innerFollowSpeed * innerFollowSpeed * dt *innerFollowSpeed);
                    var n4 = 1 + innerFollowSpeed * dt *innerFollowSpeed;
                    velocity = n3 / (n4 * n4);
                    break;
            }

            if (smoothTypeInnerCircle != InnerSmoothingType.Instant)
            {
                if (velocity.magnitude * dt < Vector3.Distance(playerPos, currentLookPoint))
                {
                    outPut[0] = currentLookPoint + velocity * dt *innerFollowSpeed;
                }
                else
                {
                    outPut[0] = playerPos;
                }
            }
            else
            {
                outPut[0] = playerPos;
            }
        
            switch (smoothTypeOuterCircle)
            {

                case OuterSmoothingType.Exponential:
                    velocity = playerPos - currentLookPoint;
                    break;

                case OuterSmoothingType.Spring:
                    var n1 = velocity - (currentLookPoint - playerPos) * (innerFollowSpeed * dt * outerFollowSpeed);
                    var n2 = 1 + dt;
                    velocity = n1 / (n2 * n2);
                    break;

                case OuterSmoothingType.DampedSpring:
                    var n3 = velocity - (currentLookPoint - playerPos) * (innerFollowSpeed * innerFollowSpeed * dt * outerFollowSpeed);
                    var n4 = 1 + innerFollowSpeed * dt * outerFollowSpeed;
                    velocity = n3 / (n4 * n4);
                    break;
            }
            if (smoothTypeOuterCircle != OuterSmoothingType.Instant)
            {
               outPut[1] = currentLookPoint + velocity * dt * outerFollowSpeed;
            }
            else
            {
               outPut[1] = Vector3.Lerp(playerPos, currentLookPoint, circleRadius / lookPointToPlayerDist);
            }

        return outPut;
    }

}