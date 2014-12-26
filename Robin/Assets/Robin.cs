using UnityEngine;
using System.Collections;

public class Robin : MonoBehaviour
{
    public GameObject body;
    public Mode mode;
    public enum Mode { _3D, _2D };

    public enum MovementInput { Mouse, Keyboard };

    public Vector3 inputDirection;
    public enum State { Idle, Walk, Run }

    public CharacterSetup characterSetup;
    [System.Serializable]
    public struct CharacterSetup
    {
        public MovementInput movementInput;
        public float rotateSpeed;
    }

    public MouseSetup mouseSetup;
    [System.Serializable]
    public struct MouseSetup
    {
        public float rayLength;
        public LayerMask ground;
        public Vector3 worldPosition;
    }

    void Start()
    {

    }

    void Update()
    {

    }
    void FixedUpdate()
    {
        InputCheck();
        FrontCheck();
    }

    void InputCheck()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 || Mathf.Abs(Input.GetAxis("Vertical")) > 0)
        {
            inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            CharacterMotion();
        }
        float step = characterSetup.rotateSpeed * Time.deltaTime;
        Vector3 targetDirection = new Vector3(mouseSetup.worldPosition.x, 0, mouseSetup.worldPosition.z).normalized;
        Vector3 tweenDirection = Vector3.RotateTowards(transform.forward, targetDirection, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(tweenDirection);

        Ray _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(_mouseRay.origin, _mouseRay.direction * mouseSetup.rayLength, Color.magenta);
        RaycastHit _rayHit;
        if (Physics.Raycast(_mouseRay, out _rayHit, mouseSetup.rayLength, mouseSetup.ground))
        {
            mouseSetup.worldPosition = _rayHit.point;
        }
    }

    void FrontCheck()
    {
        float _length = 10f;

        if (mode == Mode._3D)
        {
            Ray _ray = new Ray(transform.position, transform.forward * _length);
            Debug.DrawRay(_ray.origin, _ray.direction, Color.red);
        }
        else
        {

        }
    }

    void CharacterMotion()
    {
        rigidbody.velocity = inputDirection * 5f;
    }
}
