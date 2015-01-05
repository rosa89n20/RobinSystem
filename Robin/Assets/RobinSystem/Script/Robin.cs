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

    public UiBarManager playerHealthBar;
    public UiBarManager playerManaBar;

    public CharacterSetup characterSetup;
    //[System.Serializable]
    //public struct CharacterSetup
    //{
    //    public float healthPoint;
    //    public float manaPoint;
    //    public Animator animator;
    //    public MovementInput movementInput;
    //    public float rotateSpeed;
    //    public bool isGrounded;
    //    public float groundLength;
    //    public float groundOffset;
    //}
    
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
        public float damage;
        public GameObject enemy;
        public GameObject lockOnEnemy;
        public float attackRange;
        public GAC attackModule;
        public GameObject attackEffect;
    }

    public AnimationState animationState;
    public enum AnimationState { Idle, Walk, Run, Attack, Dead }

    public UiBarManager enemyHealthBar;

    private NavMeshAgent _agent;
    private Highlighter _highlight;
    private AttachCamera _camera;

    private float _maxHp;
    private float _maxMana;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _highlight = GetComponent<Highlighter>();
        _camera = Camera.main.GetComponent<AttachCamera>();
        _maxHp = characterSetup.healthPoint;
        _maxMana = characterSetup.manaPoint;
        SetHealthBar(enemyHealthBar, false, Vector2.zero);
    }

    void Start()
    {

    }

    void Update()
    {
        MouseAction();
        GroundCheck();
        CharacterUI();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    void FixedUpdate()
    {

    }

    private RaycastHit[] GetMouseHit(Ray ray, float length)
    {
        RaycastHit[] _hit;
        _hit = Physics.RaycastAll(ray, length);
        return _hit;
    }

    void CharacterUI()
    {
        SetHealthBar(playerHealthBar, true, new Vector2(characterSetup.healthPoint, _maxHp));
        SetHealthBar(playerManaBar, true, new Vector2(characterSetup.manaPoint, _maxMana));
    }

    private void SetHealthBar(UiBarManager ui, bool show, Vector2 value)
    {
        if (ui != null)
        {
            if (show)
            {
                ui.gameObject.SetActive(true);
                ui.SetValue(value);
            }
            else
                ui.gameObject.SetActive(false);
        }
    }

    void GetHit(float damage)
    {
        if (characterSetup.healthPoint - damage > 0)
            characterSetup.healthPoint -= damage;
        else
            characterSetup.healthPoint = 0;
        //characterSetup.animator.Play("GetHit" + Random.Range(1, 3), 1);
        characterSetup.animator.SetTrigger("GetHit");
    }

    void MouseAction()
    {
        Ray _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] _hits = GetMouseHit(_mouseRay, mouseSetup.rayLength);
        //Check all object on raycasthit
        for (int e = 0; e < _hits.Length; e++)
        {
            if (_hits[e].collider.gameObject.tag == "Enemy")
            {
                Cursor.SetCursor(mouseSetup.attack, Vector2.zero, CursorMode.Auto);
                attackSeutp.enemy = _hits[e].collider.gameObject;
                attackSeutp.enemy.SendMessage("On", Color.red, SendMessageOptions.DontRequireReceiver);
                Vector2 _v = attackSeutp.enemy.GetComponent<Oracle>().GetHealthBar();
                SetHealthBar(enemyHealthBar, true, _v);
                break;
            }
            else
            {
                Cursor.SetCursor(mouseSetup.normal, Vector2.zero, CursorMode.Auto);
                if (attackSeutp.enemy != null)
                {
                    attackSeutp.enemy.SendMessage("Off", SendMessageOptions.DontRequireReceiver);
                    attackSeutp.enemy = null;
                    SetHealthBar(enemyHealthBar, false, Vector2.zero);
                }
            }
        }

        //When pressing mouse left-button
        if (Input.GetMouseButton(0))
        {
            //Check all raycasthit object again, and get pointer
            for (int i = 0; i < _hits.Length; i++)
            {
                if (_hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    mouseSetup.pointPosition = _hits[i].point;
                    if (attackSeutp.lockOnEnemy != null)
                        attackSeutp.lockOnEnemy = null;
                    break;
                }
            }

            //If mouse currently point at enemy
            if (attackSeutp.enemy != null)
            {
                if (Vector3.Distance(transform.position, attackSeutp.enemy.transform.position) < attackSeutp.attackRange)
                {
                    attackSeutp.attackModule.enabled = true;
                    _agent.Stop();
                    transform.rotation = LookRotation(attackSeutp.enemy.transform.position, characterSetup.rotateSpeed);
                }
                else
                {
                    attackSeutp.lockOnEnemy = attackSeutp.enemy;
                }
            }
            else
            {
                attackSeutp.attackModule.enabled = false;
                if (!characterSetup.animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                    if (Vector3.Distance(transform.position, mouseSetup.pointPosition) > 1f)
                        _agent.destination = mouseSetup.pointPosition;
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
            transform.rotation = LookRotation(mouseSetup.pointPosition, characterSetup.rotateSpeed);
        }

        Debug.DrawRay(_mouseRay.origin, _mouseRay.direction * mouseSetup.rayLength, Color.magenta);

        if (attackSeutp.lockOnEnemy != null)
            LockOnEnemy();
    }

    void InputCheck()
    {
        //This's for keyboard control
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 || Mathf.Abs(Input.GetAxis("Vertical")) > 0)
        {
            inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        }
    }

    public void Attack()
    {
        GameObject target = null;
        if (attackSeutp.lockOnEnemy != null)
            target = attackSeutp.lockOnEnemy;
        else if (attackSeutp.enemy != null)
            target = attackSeutp.enemy;
        if (attackSeutp.attackModule.enabled && target != null)
        {
            transform.rotation = LookRotation(target.transform.position, characterSetup.rotateSpeed);
            target.GetComponent<GAC_TargetTracker>().playDamage = true;
            target.GetComponent<GAC_TargetTracker>().DamageMovement(gameObject, target);
            float _dmg = Mathf.RoundToInt(Random.Range(-attackSeutp.damage / 10, attackSeutp.damage / 10)) + attackSeutp.damage;
            target.SendMessage("GetHit", _dmg);
            GameManager.CreateDamageText(target, _dmg.ToString(), Color.white, new Vector3(Random.Range(-5, 6), 5, 0));
            if (attackSeutp.attackEffect)
                Instantiate(attackSeutp.attackEffect, transform.position, Quaternion.identity);
        }
        if (attackSeutp.lockOnEnemy != null)
            attackSeutp.lockOnEnemy = null;
    }

    void LockOnEnemy()
    {
        //Debug.Log("LockOnEnemy");
        if (attackSeutp.attackModule.enabled)
            attackSeutp.attackModule.enabled = false;
        _agent.destination = attackSeutp.lockOnEnemy.transform.position;
        attackSeutp.lockOnEnemy.SendMessage("On", Color.red, SendMessageOptions.DontRequireReceiver);
        //Vector2 _v = attackSeutp.lockOnEnemy.GetComponent<Oracle>().GetHealthBar();
        //SetHealthBar(true, _v);
        if (Vector3.Distance(transform.position, attackSeutp.lockOnEnemy.transform.position) < attackSeutp.attackRange)
        {
            attackSeutp.attackModule.enabled = true;
            _agent.Stop();
            characterSetup.animator.Play("Combo1");
            return;
        }
    }

    Quaternion LookRotation(Vector3 target, float speed)
    {
        float step = speed * Time.deltaTime;
        Vector3 direction = (target - transform.position).normalized;
        Vector3 tween = Vector3.RotateTowards(transform.forward, new Vector3(direction.x, 0, direction.z), step, 0.0f);
        return Quaternion.LookRotation(tween);
    }

    void OnDrawGizmos()
    {
        //Mouse position
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawSphere(mouseSetup.pointPosition, 0.25f);
        Gizmos.DrawRay(transform.position + new Vector3(0, characterSetup.groundOffset, 0), transform.up * -characterSetup.groundLength);
        //Attack range
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawSphere(transform.position, attackSeutp.attackRange);
        //Front check
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 10f);
    }

    private void GroundCheck()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, characterSetup.groundOffset, 0), -transform.up, characterSetup.groundLength, mouseSetup.ground))
            characterSetup.isGrounded = true;
        else
            characterSetup.isGrounded = false;
    }
}
