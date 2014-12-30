using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;
using JrDevAssets;

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
        public Animator animator;
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
        public LayerMask environment;
        public Color highlightColor;
        public float highlightRayLength;
        public float highlightOffset;
    }

    public AttackSetup attackSeutp;
    [System.Serializable]
    public struct AttackSetup
    {
        public float attackRange;
        public float attackDuration;
        public GAC attackModule;
    }

    public AnimationState animationState;
    public enum AnimationState { Idle, Walk, Run, Attack, Dead }

    private NavMeshAgent _agent;
    private Highlighter _highlight;
    private AttachCamera _camera;
    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _highlight = GetComponent<Highlighter>();
        _camera = Camera.main.GetComponent<AttachCamera>();
    }

    void Start()
    {

    }

    void Update()
    {
        InputCheck();
        GroundCheck();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
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
            if (_hits[e].collider.gameObject.tag == "Enemy")
            {
                Cursor.SetCursor(mouseSetup.attack, Vector2.zero, CursorMode.Auto);
                enemy = _hits[e].collider.gameObject;
                enemy.SendMessage("On", Color.red);
                break;
            }
            else
            {
                Cursor.SetCursor(mouseSetup.normal, Vector2.zero, CursorMode.Auto);
                if (enemy != null)
                    enemy.SendMessage("Off");
                enemy = null;
            }
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


            //_agent.SetDestination(mouseSetup.targetPosition);
            if (enemy != null)
            {
                Debug.DrawLine(transform.position, enemy.transform.position, Color.magenta);
                if (Vector3.Distance(transform.position, enemy.transform.position) < attackSeutp.attackRange)
                {
                    attackSeutp.attackModule.enabled = true;
                    _agent.Stop();
                    StandingRotation();
                }
            }
            else
            {
                attackSeutp.attackModule.enabled = false;
                if (Vector3.Distance(transform.position, mouseSetup.pointPosition) > 1f)
                {
                    _agent.destination = mouseSetup.pointPosition;
                }
            }
        }

        float _moveSpeed = Mathf.Max(Mathf.Abs(_agent.velocity.normalized.x), Mathf.Abs(_agent.velocity.normalized.z));

        characterSetup.animator.SetFloat("MoveSpeed", _moveSpeed);

        //if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0)
        //{
        //    if (_camera.offset.y >= 4f && _camera.offset.y <= 11f)
        //        _camera.offset.y += (Input.GetAxis("Mouse ScrollWheel") * 100f) * Time.deltaTime;
        //    if (_camera.offset.z >= -4.5f && _camera.offset.z <= -8f)
        //        _camera.offset.z += (Input.GetAxis("Mouse ScrollWheel") * -50f) * Time.deltaTime;
        //}


        Ray _topRay = new Ray(transform.position + new Vector3(0f, mouseSetup.highlightOffset, 0f), new Vector3(0, 1, -1));
        Debug.DrawRay(_topRay.origin, _topRay.direction * mouseSetup.highlightRayLength, Color.red);
        if (Physics.Raycast(_topRay, mouseSetup.highlightRayLength, mouseSetup.environment))
            _highlight.On(mouseSetup.highlightColor);
        else
            _highlight.Off();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _agent.Stop();
            StandingRotation();
        }

        Debug.DrawRay(_mouseRay.origin, _mouseRay.direction * mouseSetup.rayLength, Color.magenta);
        //Debug.DrawLine(_mouseRay.origin, mouseSetup.edgePosition, Color.blue);
    }

    void StandingRotation()
    {
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
