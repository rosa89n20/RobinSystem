using UnityEngine;
using System.Collections;

public class MouseControlModule : MonoBehaviour
{
    public Camera handleCamera;
    public LayerMask groundLayer;
    public LayerMask mouseRayIgnore;
    public float mouseRayLength = 10f;
    public Vector3 mousePosition;
    public GameObject[] mouseHits;
    public string enemyTag;
    public GameObject hoverEnemy;
    public bool isLockOn;
    public GameObject lockOnEnemy;
    public enum MouseCursorType { Normal, Hover, Active }
    static Texture2D normalCursor;
    static Texture2D hoverCursor;
    static Texture2D activeCursor;
    public Texture2D NormalCursor { get { return MouseControlModule.normalCursor; } set { MouseControlModule.normalCursor = value; } }
    public Texture2D HoverCursor { get { return MouseControlModule.hoverCursor; } set { MouseControlModule.hoverCursor = value; } }
    public Texture2D ActiveCursor { get { return MouseControlModule.activeCursor; } set { MouseControlModule.activeCursor = value; } }

    void Update()
    {
        MouseAction();
    }

    private RaycastHit[] GetMouseHits(Ray ray, float length)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(ray, length);
        return hits;
    }

    public static void SetCursor(MouseCursorType type)
    {
        Texture2D cursor;
        switch (type)
        {
            case MouseCursorType.Normal:
                cursor = normalCursor;
                break;
            case MouseCursorType.Hover:
                cursor = hoverCursor;
                break;
            case MouseCursorType.Active:
                cursor = activeCursor;
                break;
            default:
                cursor = normalCursor;
                break;
        }
        Cursor.SetCursor(cursor, Vector2.zero, CursorMode.Auto);
    }

    void MouseAction()
    {
        Ray mouseRay = handleCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = GetMouseHits(mouseRay, mouseRayLength);

        //Check all objects in raycasthits
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider.gameObject.tag == enemyTag)
            {
                SetCursor(MouseCursorType.Hover);
                hoverEnemy = hits[i].collider.gameObject;
                //hoverEnemy.SendMessage("On", Color.red, SendMessageOptions.DontRequireReceiver);
                //Vector2 _v = hoverEnemy.GetComponent<Oracle>().GetHealthBar();
                //SetHealthBar(hoverEnemyHealthBar, true, _v);
                break;
            }
            else
            {
                SetCursor(MouseCursorType.Normal);
                if (hoverEnemy)
                {
                    //hoverEnemy.SendMessage("Off", SendMessageOptions.DontRequireReceiver);
                    hoverEnemy = null;
                    //SetHealthBar(hoverEnemyHealthBar, false, Vector2.zero);
                }
            }
        }

        //When pressing mouse left-button
        if (Input.GetMouseButton(0))
        {
            //Check all raycasthit object again, and get pointer
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    mousePosition = hits[i].point;
                    if (lockOnEnemy)
                        lockOnEnemy = null;
                    break;
                }
            }

            //If mouse currently point at hoverEnemy
            if (hoverEnemy != null)
            {
                //if (Vector3.Distance(transform.position, hoverEnemy.transform.position) < attackRange)
                //{
                //    attack.GacSyatem.enabled = true;
                //    _agent.Stop();
                //    transform.rotation = LookRotation(output.hoverEnemy.transform.position, character.rotateSpeed);
                //}
                //else
                //{
                //    output.lockOnEnemy = output.hoverEnemy;
                //}
            }
            else
            {
                //attack.GacSyatem.enabled = false;
                //if (!character.mecanim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                //    if (Vector3.Distance(transform.position, output.pointerPosition) > 1f)
                //        _agent.destination = output.pointerPosition;
            }
        }

        float moveSpeed = Mathf.Max(Mathf.Abs(_agent.velocity.normalized.x), Mathf.Abs(_agent.velocity.normalized.z));

        //character.mecanim.SetFloat("MoveSpeed", _moveSpeed);

        //Ray _topRay = new Ray(transform.position + new Vector3(0f, checker.highlightRayOffset, 0f), new Vector3(0, 1, -1));
        //Debug.DrawRay(_topRay.origin, _topRay.direction * checker.highlightRayLength, Color.red);
        //if (Physics.Raycast(_topRay, checker.highlightRayLength, checker.mouseRayIgnore))
        //    _highlight.On(checker.highlightColor);
        //else
        //    _highlight.Off();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //_agent.Stop();
            transform.rotation = LookRotation(output.pointerPosition, character.rotateSpeed);
        }

        //Debug.DrawRay(mouse..origin, _mouseRay.direction * checker.mouseRayLength, Color.magenta);

        //if (output.lockOnEnemy != null)
        //    LockOnEnemy();
    }

    Quaternion LookRotation(Vector3 target, float speed)
    {
        float step = speed * Time.deltaTime;
        Vector3 direction = (target - transform.position).normalized;
        Vector3 tween = Vector3.RotateTowards(transform.forward, new Vector3(direction.x, 0, direction.z), step, 0.0f);
        return Quaternion.LookRotation(tween);
    }

    void LockOnEnemy()
    {
        //if (attack.GacSyatem.enabled)
        //    attack.GacSyatem.enabled = false;
        //_agent.destination = output.lockOnEnemy.transform.position;
        //output.lockOnEnemy.SendMessage("On", Color.red, SendMessageOptions.DontRequireReceiver);
        //Vector2 _v = output.lockOnEnemy.GetComponent<Oracle>().GetHealthBar();
        //SetHealthBar(true, _v);
        //if (Vector3.Distance(transform.position, lockOnEnemy.transform.position) < attack.attackRange)
        //{
        //    attack.GacSyatem.enabled = true;
        //    _agent.Stop();
        //    character.mecanim.Play("Combo1");
        //    return;
        //}
    }
}
