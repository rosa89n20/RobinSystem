using UnityEngine;
using System.Collections;

public class CameraModule : MonoBehaviour
{
    public Camera handleCamera;
    public Transform target;
    public float distance = 7f;
    public float height = 3f;
    public float angularSmoothLag = 0.3f;
    public float angularMaxSpeed = 15f;
    public float heightSmoothLag = 0.3f;
    public float snapSmoothLag = 0.2f;
    public float snapMaxSpeed = 720f;
    public float clampHeadPositionScreenSpace = 0.75f;
    public float lockCameraTimeout = 0.2f;

    void Awake()
    {
        if (!handleCamera)
            handleCamera = Camera.main;

    }


    float AngleDistance(float a, float b)
    {
        a = Mathf.Repeat(a, 360f);
        b = Mathf.Repeat(b, 360f);
        return Mathf.Abs(b - a);
    }

    Vector3 headOffset = Vector3.zero;
    Vector3 centerOffset = Vector3.zero;
    float heightVelocity = 0;
    float angleVelocity = 0;
    bool snap = false;
    float targetHeight = 100000;

    void ApplyCamera(Transform target, Vector3 center)
    {
        Vector3 targetCenter = target.position + centerOffset;
        Vector3 targetHead = target.position + headOffset;

        float originalTargetAngle = target.eulerAngles.y;
        float currentAngle = handleCamera.transform.eulerAngles.y;

        float targetAngle = originalTargetAngle;

        if (snap)
        {
            if (AngleDistance(currentAngle, originalTargetAngle) < 3f)
                snap = false;

            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, snapSmoothLag, snapMaxSpeed);
        }
        else
        {
            if (AngleDistance(currentAngle, targetAngle) > 160f)
                targetAngle += 180f;

            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, angularSmoothLag, angularMaxSpeed);
        }

        targetHeight = targetCenter.y + height;

        float currentHeight = handleCamera.transform.position.y;
        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothLag);

        Quaternion currentRotation = Quaternion.Euler(0f, currentAngle, 0f);

        handleCamera.transform.position = targetCenter;
        handleCamera.transform.position += currentRotation * Vector3.back * distance;

        handleCamera.transform.position = new Vector3(handleCamera.transform.position.x, currentHeight, handleCamera.transform.position.z);


    }

    void LateUpdate()
    {
        ApplyCamera(transform, Vector3.zero);
    }

    void Cut(Transform target,Vector3 center)
    {
        float oldHeightSmooth = heightSmoothLag;
        float oldSnapMaxSpeed = snapMaxSpeed;
        float oldsnapSmoothLag = snapSmoothLag;

        snapMaxSpeed = 10000f;
        snapSmoothLag = 0.001f;
        heightSmoothLag = 0.001f;

        snap = true;
        ApplyCamera(transform, Vector3.zero);

        heightSmoothLag = oldHeightSmooth;
        snapMaxSpeed = oldSnapMaxSpeed;
        snapSmoothLag = oldSnapMaxSpeed;
    }

    void SetUpRotation(Vector3 center,Vector3 head)
    {
        Vector3 cameraPosition = handleCamera.transform.position;
        Vector3 offsetToCenter = center - cameraPosition;

        Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));

        Vector3 relativeOffset = Vector3.forward * distance + Vector3.down * height;
        handleCamera.transform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);

        Ray centerRay = handleCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
        Ray topRay = handleCamera.ViewportPointToRay(new Vector3(0.3f, clampHeadPositionScreenSpace, 1f));

        Vector3 centerRayPosition = centerRay.GetPoint(distance);
        Vector3 topRayPosition = topRay.GetPoint(distance);

        float centerToTopAngle = Vector3.Angle(centerRay.direction, topRay.direction);
        float heightToAngle = centerToTopAngle / (centerRayPosition.y - topRayPosition.y);
        float extraLookAngle = heightToAngle * (centerRayPosition.y - center.y);

        if(extraLookAngle<centerToTopAngle)
        {
            extraLookAngle = 0f;
        }
        else
        {
            extraLookAngle = extraLookAngle - centerToTopAngle;
            handleCamera.transform.rotation *= Quaternion.Euler(-extraLookAngle, 0, 0);
        }
    }

    Vector3 GetCenterOffset()
    {
        return centerOffset;
    }
}
