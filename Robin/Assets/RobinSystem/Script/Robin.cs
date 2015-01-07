using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;
using JrDevAssets;

public class Robin : MonoBehaviour
{
    public CharacterSetup character;
    public CheckerSetup checker;
    public AttackSetup attack;
    public OutputSetup output;

    public UiBarManager playerHealthBar;
    public UiBarManager playerManaBar;
    public UiBarManager hoverEnemyHealthBar;

    public LayerMask groundLayer;
    public LayerMask mouseRayIgnore;
    
    private NavMeshAgent _agent;
    private Highlighter _highlight;
    private AttachCamera _camera;
    public float _maxHp;
    public float _maxMana;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _highlight = GetComponent<Highlighter>();
        _camera = Camera.main.GetComponent<AttachCamera>();
        character.mecanim = GetComponent<Animator>();
        _maxHp = character.healthPoint;
        _maxMana = character.manaPoint;
        SetHealthBar(hoverEnemyHealthBar, false, Vector2.zero);
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
        SetHealthBar(playerHealthBar, true, new Vector2(character.healthPoint, _maxHp));
        SetHealthBar(playerManaBar, true, new Vector2(character.manaPoint, _maxMana));
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
        if (character.healthPoint - damage > 0)
            character.healthPoint -= damage;
        else
            character.healthPoint = 0;
        //character.mecanim.Play("GetHit" + Random.Range(1, 3), 1);
        character.mecanim.SetTrigger("GetHit");
    }

    void MouseAction()
    {
        Ray _mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] _hits = GetMouseHit(_mouseRay, checker.mouseRayLength);
        //Check all object on raycasthit
        for (int e = 0; e < _hits.Length; e++)
        {
            if (_hits[e].collider.gameObject.tag == "Enemy")
            {
                GameManager.SetCursor(GameManager.MouseCursorType.Hover);
                output.hoverEnemy = _hits[e].collider.gameObject;
                output.hoverEnemy.SendMessage("On", Color.red, SendMessageOptions.DontRequireReceiver);
                Vector2 _v = output.hoverEnemy.GetComponent<Oracle>().GetHealthBar();
                SetHealthBar(hoverEnemyHealthBar, true, _v);
                break;
            }
            else
            {
                GameManager.SetCursor(GameManager.MouseCursorType.Normal);
                if (output.hoverEnemy != null)
                {
                    output.hoverEnemy.SendMessage("Off", SendMessageOptions.DontRequireReceiver);
                    output.hoverEnemy = null;
                    SetHealthBar(hoverEnemyHealthBar, false, Vector2.zero);
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
                    output.pointerPosition = _hits[i].point;
                    if (output.lockOnEnemy != null)
                        output.lockOnEnemy = null;
                    break;
                }
            }

            //If mouse currently point at hoverEnemy
            if (output.hoverEnemy != null)
            {
                if (Vector3.Distance(transform.position, output.hoverEnemy.transform.position) < attack.attackRange)
                {
                    attack.GacSyatem.enabled = true;
                    _agent.Stop();
                    transform.rotation = LookRotation(output.hoverEnemy.transform.position, character.rotateSpeed);
                }
                else
                {
                    output.lockOnEnemy = output.hoverEnemy;
                }
            }
            else
            {
                attack.GacSyatem.enabled = false;
                if (!character.mecanim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                    if (Vector3.Distance(transform.position, output.pointerPosition) > 1f)
                        _agent.destination = output.pointerPosition;
            }
        }

        float _moveSpeed = Mathf.Max(Mathf.Abs(_agent.velocity.normalized.x), Mathf.Abs(_agent.velocity.normalized.z));

        character.mecanim.SetFloat("MoveSpeed", _moveSpeed);

        Ray _topRay = new Ray(transform.position + new Vector3(0f, checker.highlightRayOffset, 0f), new Vector3(0, 1, -1));
        Debug.DrawRay(_topRay.origin, _topRay.direction * checker.highlightRayLength, Color.red);
        if (Physics.Raycast(_topRay, checker.highlightRayLength, checker.mouseRayIgnore))
            _highlight.On(checker.highlightColor);
        else
            _highlight.Off();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _agent.Stop();
            transform.rotation = LookRotation(output.pointerPosition, character.rotateSpeed);
        }

        Debug.DrawRay(_mouseRay.origin, _mouseRay.direction * checker.mouseRayLength, Color.magenta);

        if (output.lockOnEnemy != null)
            LockOnEnemy();
    }

    public void Attack()
    {
        GameObject target = null;
        if (output.lockOnEnemy != null)
            target = output.lockOnEnemy;
        else if (output.hoverEnemy != null)
            target = output.hoverEnemy;
        if (attack.GacSyatem.enabled && target != null)
        {
            transform.rotation = LookRotation(target.transform.position, character.rotateSpeed);
            target.GetComponent<GAC_TargetTracker>().playDamage = true;
            target.GetComponent<GAC_TargetTracker>().DamageMovement(gameObject, target);
            float _dmg = Mathf.RoundToInt(Random.Range(-attack.damage / 10, attack.damage / 10)) + attack.damage;
            target.SendMessage("GetHit", _dmg);
            GameManager.CreateDamageText(target, _dmg.ToString(), Color.white, new Vector3(Random.Range(-5, 6), 5, 0));
            if (attack.attackEffect)
                Instantiate(attack.attackEffect, transform.position, Quaternion.identity);
        }
        if (output.lockOnEnemy != null)
            output.lockOnEnemy = null;
    }

    void LockOnEnemy()
    {
        if (attack.GacSyatem.enabled)
            attack.GacSyatem.enabled = false;
        _agent.destination = output.lockOnEnemy.transform.position;
        output.lockOnEnemy.SendMessage("On", Color.red, SendMessageOptions.DontRequireReceiver);
        //Vector2 _v = output.lockOnEnemy.GetComponent<Oracle>().GetHealthBar();
        //SetHealthBar(true, _v);
        if (Vector3.Distance(transform.position, output.lockOnEnemy.transform.position) < attack.attackRange)
        {
            attack.GacSyatem.enabled = true;
            _agent.Stop();
            character.mecanim.Play("Combo1");
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
        ////Mouse position
        //Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        ////Gizmos.DrawSphere(checker.pointerPosition, 0.25f);
        //Gizmos.DrawRay(transform.position + new Vector3(0, checker.groundRayOffset, 0), transform.up * -checker.groundRayLength);
        ////Attack range
        //Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        //Gizmos.DrawSphere(transform.position, attack.attackRange);
        ////Front check
        //Gizmos.color = Color.red;
        //Gizmos.DrawRay(transform.position, transform.forward * 10f);
    }

    private void GroundCheck()
    {
        if (Physics.Raycast(transform.position + new Vector3(0, checker.groundRayOffset, 0), -transform.up, checker.groundRayLength, checker.ground))
            output.isGrounded = true;
        else
            output.isGrounded = false;
    }
}
