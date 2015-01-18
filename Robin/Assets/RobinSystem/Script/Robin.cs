using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;
using JrDevAssets;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Highlighter))]
[RequireComponent(typeof(AttachCamera))]
[RequireComponent(typeof(GAC))]
public class Robin : MonoBehaviour
{
    #region Character
    private Animator mecanim;
    private NavMeshAgent agent;
    public float healthPoint = 100f;
    public float manaPoint = 100f;
    public float maxHp;
    public float maxMana;
    public float rotateSpeed = 10f;
    #endregion

    #region Attack
    public GAC gac;
    public float damage;
    public float attackRange = 5f;
    public float attackAngle;
    public GameObject attackEffect;
    #endregion

    #region Checker
    public LayerMask mouseRayIgnore;
    public string enemyTag;
    public GameObject lockOnEnemy;
    #endregion

    #region Third party
    public bool useHighlightingSystem;
    private Highlighter highlight;
    public Color highlightColor;
    public float highlightRayLength = 5f;
    public float highlightRayOffset;
    public MouseModule mouseModule;
    public CameraModule cameraModule;
    #endregion

    #region UI
    public UiBarManager playerHealthBar;
    public UiBarManager playerManaBar;
    public UiBarManager hoverEnemyHealthBar;
    private AttachCamera arpgCamera;
    #endregion

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        highlight = GetComponent<Highlighter>();
        arpgCamera = Camera.main.GetComponent<AttachCamera>();
        mecanim = GetComponent<Animator>();
        gac = GetComponent<GAC>();
        maxHp = healthPoint;
        maxMana = manaPoint;
        SetHealthBar(hoverEnemyHealthBar, false, Vector2.zero);
    }

    void Start()
    {

    }

    void Update()
    {
        if (mouseModule)
            MouseAction();
        if (cameraModule)
            CameraAction();
        CharacterUI();



        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void FixedUpdate()
    {

    }

    void CharacterUI()
    {
        SetHealthBar(playerHealthBar, true, new Vector2(healthPoint, maxHp));
        SetHealthBar(playerManaBar, true, new Vector2(manaPoint, maxMana));
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
        if (healthPoint - damage > 0)
            healthPoint -= damage;
        else
            healthPoint = 0;
        mecanim.Play("GetHit" + Random.Range(1, 3), 1);
        mecanim.SetTrigger("GetHit");
    }

    void CameraAction()
    {
        cameraModule.ApplyCamera(transform);

        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(mouseWheel) > 0f)
            cameraModule.SetCameraHeight(cameraModule.GetCameraHeight() + mouseWheel * 2f);

        float mouseX = Input.GetAxis("Mouse X");
        if (Input.GetMouseButton(1))
            cameraModule.SetCameraAngle(cameraModule.GetCameraAngle() + mouseX * 5f);
    }

    void MouseAction()
    {
        mouseModule.MouseInteract();

        if (mouseModule.OnHover(enemyTag))
        {
            mouseModule.SetCursor(MouseModule.MouseCursorType.Hover);
            mouseModule.hoverEnemy.SendMessage("On", Color.red, SendMessageOptions.DontRequireReceiver);
            if (mouseModule.hoverEnemy.GetComponent<Oracle>())
            {
                Vector2 v = mouseModule.hoverEnemy.GetComponent<Oracle>().GetHealthBar();
                SetHealthBar(hoverEnemyHealthBar, true, v);
            }
        }
        else
        {
            mouseModule.SetCursor(MouseModule.MouseCursorType.Normal);
            SetHealthBar(hoverEnemyHealthBar, false, Vector2.zero);
        }

        if (Input.GetMouseButton(0))
        {
            if (lockOnEnemy)
                lockOnEnemy = null;

            if (mouseModule.hoverEnemy)
            {
                GameObject pointAt = mouseModule.hoverEnemy;
                if (Vector3.Distance(transform.position, pointAt.transform.position) < attackRange)
                {
                    gac.enabled = true;
                    agent.Stop();
                    transform.rotation = LookRotation(pointAt.transform.position, rotateSpeed);
                }
                else
                {
                    lockOnEnemy = pointAt;
                }
            }
            else
            {
                gac.enabled = false;
                if (!mecanim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                    if (Vector3.Distance(transform.position, mouseModule.mousePosition) > 1f)
                        agent.destination = mouseModule.mousePosition;
            }
        }

        float moveSpeed = Mathf.Max(Mathf.Abs(agent.velocity.normalized.x), Mathf.Abs(agent.velocity.normalized.z));

        mecanim.SetFloat("MoveSpeed", moveSpeed);



        if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(0))
        {
            agent.Stop();
            transform.rotation = LookRotation(mouseModule.mousePosition, rotateSpeed);
        }

        if (lockOnEnemy)
            LockOnEnemy();

        if (useHighlightingSystem)
            HighlightCheck();
    }

    public Quaternion LookRotation(Vector3 target, float speed)
    {
        float step = speed * Time.deltaTime;
        Vector3 direction = (target - transform.position).normalized;
        Vector3 tween = Vector3.RotateTowards(transform.forward, new Vector3(direction.x, 0, direction.z), step, 0.0f);
        return Quaternion.LookRotation(tween);
    }

    void HighlightCheck()
    {
        //Ray top = new Ray(transform.position + new Vector3(0f, highlightRayOffset, 0f), new Vector3(0, 1, -1));
        //Debug.DrawLine(top.origin,top.origin+(new Vector3(0,1,-1)*highlightRayLength), Color.red);
        //if (Physics.SphereCast(top, 0.5f, highlightRayLength, mouseRayIgnore))
        //{
        //    highlight.On(highlightColor);
        //    highlight.SeeThroughOff();
        //}
        //else
        //    highlight.Off();
        if (Physics.CheckCapsule(transform.position + new Vector3(0f, highlightRayOffset, 0f), transform.position + new Vector3(0f, highlightRayOffset, 0f) + new Vector3(0, highlightRayLength, -highlightRayLength), 0.5f, mouseRayIgnore))
            highlight.On(highlightColor);
        else
            highlight.Off();
    }

    public void Attack()
    {
        GameObject target = null;
        if (lockOnEnemy)
            target = lockOnEnemy;
        else if (mouseModule.hoverEnemy != null)
            target = mouseModule.hoverEnemy;
        if (gac.enabled && target != null)
        {
            transform.rotation = LookRotation(target.transform.position, rotateSpeed);
            target.GetComponent<GAC_TargetTracker>().playDamage = true;
            target.GetComponent<GAC_TargetTracker>().DamageMovement(gameObject, target);
            float dmg = Mathf.RoundToInt(Random.Range(-damage / 10, damage / 10)) + damage;
            target.SendMessage("GetHit", dmg);
            GameManager.CreateDamageText(target, dmg.ToString(), Color.white, new Vector3(Random.Range(-5, 6), 5, 0));
            if (attackEffect)
                Instantiate(attackEffect, transform.position, Quaternion.identity);
        }
        if (lockOnEnemy)
            lockOnEnemy = null;
    }

    void LockOnEnemy()
    {
        if (gac.enabled)
            gac.enabled = false;
        agent.destination = lockOnEnemy.transform.position;
        lockOnEnemy.SendMessage("On", Color.red, SendMessageOptions.DontRequireReceiver);
        Vector2 v = lockOnEnemy.GetComponent<Oracle>().GetHealthBar();
        SetHealthBar(hoverEnemyHealthBar, true, v);
        if (Vector3.Distance(transform.position, lockOnEnemy.transform.position) < attackRange)
        {
            gac.enabled = true;
            agent.Stop();
            mecanim.Play("Combo1");
            return;
        }
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
        Gizmos.DrawWireSphere(transform.position + new Vector3(0f, highlightRayOffset, 0f), 0.5f);
        Gizmos.DrawWireSphere(transform.position + new Vector3(0f, highlightRayOffset, 0f) + new Vector3(0, highlightRayLength, -highlightRayLength), 0.5f);
    }

    //=================================================
    public static bool CheckLayer(GameObject target, LayerMask layer)
    {
        if (((1 << target.layer) & layer) != 0)
            return true;
        else
            return false;
    }
}
