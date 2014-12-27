using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;

public class Robin : MonoBehaviour
{
    public GameObject body;

    public enum MovementInput { Mouse, Keyboard };

    public Vector3 inputDirection;
    public enum State { Idle, Walk, Run }

    public GameObject enemy;

    public CharacterSetup characterSetup;
    [System.Serializable]
    public struct CharacterSetup
    {
        public MovementInput movementInput;
        public float rotateSpeed;
        public bool isGrounded;
        public float groundLength;
        public float groundOffset;
    }

    public MouseSetup mouseSetup;
    
    [System.Serializable]
    public struct MouseSetup
    {
        public Texture2D normal;
        public Texture2D attack;
        public float rayLength;
        public LayerMask ground;
        public Vector3 targetPosition;
        public Vector3 pointPosition;
        public Vector3 edgePosition;
    }

    public struct AttackSetup
    {
        public float attackRange;
        public float attackDuration;
        public MonoBehaviour AttackModule;
    }

    private NavMeshAgent _agent;
    private Highlighter _highlight;
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _highlight = GetComponent<Highlighter>();
    }

    void Start()
    {

    }

    void Update()
    {
        InputCheck();
        GroundCheck();


    }
    void FixedUpdate()
    {
        FrontCheck();
    }

    RaycastHit[] GetRaycastHit(Ray ray, float length)
    {
        RaycastHit[] _hit;
        _hit = Physics.RaycastAll(ray, length);
        
        return _hit;
    }
    
    void InputCheck()
    {
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 || Mathf.Abs(Input.GetAxis("Vertical")) > 0)
        {
            inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
            CharacterMotion();
        }

        //Vector3 _mouseOnScreen = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        Ray _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] _hits = GetRaycastHit(_mouseRay, mouseSetup.rayLength);

        for (int e = 0; e < _hits.Length; e++)
        {
            if(_hits[e].collider.gameObject.tag=="Enemy")
            {
                Cursor.SetCursor(mouseSetup.attack, Vector2.zero, CursorMode.Auto);
                enemy = _hits[e].collider.gameObject;
                break;
            }
            else
                Cursor.SetCursor(mouseSetup.normal, Vector2.zero, CursorMode.Auto);
        }

        if (Input.GetMouseButton(0))
        {
            //RaycastHit[] _rayHit;
            //_rayHit = Physics.RaycastAll(_mouseRay, mouseSetup.rayLength);

            for (int i = 0; i < _hits.Length; i++)
            {
                if (_hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    mouseSetup.pointPosition = _hits[i].point;
                    break;
                }
            }

            //RaycastHit _rayHit;
            //if (Physics.Raycast(_mouseRay, out _rayHit, mouseSetup.rayLength, mouseSetup.ground))
            //{
            //    if (_rayHit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            //        mouseSetup.worldPosition = _rayHit.point;
            //}

            _agent.destination = mouseSetup.pointPosition;
        }

        if (Physics.Linecast(transform.position, Camera.main.transform.position))
            _highlight.On(Color.green);
        else
            _highlight.Off();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _agent.Stop();
        }

        Debug.DrawRay(_mouseRay.origin, _mouseRay.direction * mouseSetup.rayLength, Color.magenta);
        //Debug.DrawLine(_mouseRay.origin, mouseSetup.edgePosition, Color.blue);

        float step = characterSetup.rotateSpeed * Time.deltaTime;
        Vector3 targetDirection = (mouseSetup.pointPosition - transform.position).normalized;
        Vector3 tweenDirection = Vector3.RotateTowards(transform.forward, new Vector3(targetDirection.x, 0, targetDirection.z), step, 0.0f);
        transform.rotation = Quaternion.LookRotation(tweenDirection);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(mouseSetup.pointPosition, 0.25f);
        Gizmos.DrawRay(transform.position + new Vector3(0, characterSetup.groundOffset, 0), transform.up * -characterSetup.groundLength);
    }

    void FrontCheck()
    {
        float _length = 10f;

        Ray _ray = new Ray(transform.position, transform.forward * _length);
        Debug.DrawRay(_ray.origin, _ray.direction, Color.red);
    }

    private void GroundCheck()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, characterSetup.groundOffset, 0), -transform.up, characterSetup.groundLength, mouseSetup.ground))
            characterSetup.isGrounded = true;
        else
            characterSetup.isGrounded = false;
    }

    void CharacterMotion()
    {
        rigidbody.velocity = inputDirection * 5f;
    }
}
