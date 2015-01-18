using UnityEngine;
using System.Collections;

public class CameraModule : MonoBehaviour
{
    #region Public
    public Camera handleCamera;
    public float distance = 7f;
    public float maxDistance = 9f;
    public float minDistance = 5f;
    public float height = 3f;
    public float maxHeight = 10f;
    public float minHeight = 2f;
    public float angle = 0f;
    public float angularSmoothTime = 0.3f;
    public float angularMaxSpeed = 15f;
    public float heightSmoothTime = 0.3f;
    public float clampHeadPositionScreenSpace = 0.75f;
    public bool autoLookAt = false;
    #endregion

    #region Private
    private float heightVelocity = 0;
    private float angleVelocity = 0;
    #endregion

    void Awake()
    {
        if (!handleCamera)
            handleCamera = Camera.main;
    }

    private float AngleDistance(float a, float b)
    {
        a = Mathf.Repeat(a, 360f);
        b = Mathf.Repeat(b, 360f);
        return Mathf.Abs(b - a);
    }

    public void ApplyCamera(Transform target)
    {
        angle = Mathf.Repeat(angle, 360f);
        if (autoLookAt)
            LookAt(target);
        else
            handleCamera.transform.eulerAngles = new Vector3(handleCamera.transform.eulerAngles.x, angle, handleCamera.transform.eulerAngles.z);
        ////Set camera euler angle not over 360 degree
        float originalTargetAngle = transform.eulerAngles.y;
        float currentAngle = handleCamera.transform.eulerAngles.y;

        float targetAngle = originalTargetAngle;

        if (AngleDistance(currentAngle, targetAngle) > 160f)
            targetAngle += 180f;

        currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref angleVelocity, angularSmoothTime, angularMaxSpeed);
        ////
        Quaternion currentRotation = Quaternion.Euler(0f, currentAngle, 0f);
        ////Height
        float targetHeight = target.transform.position.y + height;
        float currentHeight = handleCamera.transform.position.y;
        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothTime);
        handleCamera.transform.position = new Vector3(target.transform.position.x, currentHeight, target.transform.position.z);
        handleCamera.transform.position += currentRotation * Vector3.back * distance;
        ////
    }

    void Update()
    {
        //handleCamera.transform.eulerAngles = new Vector3(handleCamera.transform.eulerAngles.x, angle, handleCamera.transform.eulerAngles.z);
        //ApplyCamera(transform);

    }

    public float GetCameraAngle()
    {
        return angle;
    }

    public float SetCameraAngle(float degree)
    {
        angle = degree;
        return angle;
    }

    public float GetCameraHeight()
    {
        return height;
    }

    public float SetCameraHeight(float unit)
    {
        height = unit;
        return height;
    }

    void LookAt(Transform target)
    {
        Vector3 offsetToCenter = target.position - handleCamera.transform.position;

        Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));

        Vector3 relativeOffset = Vector3.forward * distance + Vector3.down * height;
        handleCamera.transform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);

        Ray centerRay = handleCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
        Ray topRay = handleCamera.ViewportPointToRay(new Vector3(0.3f, clampHeadPositionScreenSpace, 1f));

        Vector3 centerRayPosition = centerRay.GetPoint(distance);
        Vector3 topRayPosition = topRay.GetPoint(distance);

        float centerToTopAngle = Vector3.Angle(centerRay.direction, topRay.direction);
        float heightToAngle = centerToTopAngle / (centerRayPosition.y - topRayPosition.y);
        float extraLookAngle = heightToAngle * (centerRayPosition.y - target.position.y);

        if (extraLookAngle < centerToTopAngle)
        {
            extraLookAngle = 0f;
        }
        else
        {
            extraLookAngle = extraLookAngle - centerToTopAngle;
            handleCamera.transform.rotation *= Quaternion.Euler(-extraLookAngle, 0, 0);
        }
    }
}
