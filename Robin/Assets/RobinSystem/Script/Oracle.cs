using UnityEngine;
using System.Collections;

public class Oracle : MonoBehaviour
{
    private NavMeshAgent _agent;
    private CharacterController _controller;
    private Animator _animator;
    public float idleTime;
    public float _time;
    public float patrolRange = 2f;
    public Vector3 patrolPoint;
    public Vector3 originalPosition;
    public float repathTime = 1f;
    public float patrolTime;
    public float maxPatrolTime;
    //public float chaseRange = 4f;
    public float attackRange = .5f;
    public float damage = 20f;

    public AiState aiState;
    public enum AiState { Idle, Patrol, Chase, Dead }
    public GameObject target;
    public float health = 100f;
    private float maxHealth;
    public bool isDead;
    //public bool hasPath;
    public GameObject[] deadEffect;

    public GameObject[] drops;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        maxHealth = health;
        if (Random.value > 0.5f)
            aiState = AiState.Idle;
        else
            aiState = AiState.Patrol;
    }

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if (aiState == AiState.Patrol)
        {
            if (_agent.hasPath)
            {
                patrolTime += Time.deltaTime;
                if (patrolTime > maxPatrolTime)
                {
                    SetPatrolPoint();
                    patrolTime = 0f;
                }
            }
            else
            {
                if (patrolTime == 0f)
                {
                    SetPatrolPoint();
                }
                else
                {
                    aiState = AiState.Idle;
                    patrolTime = 0f;
                    _time = 0f;
                }
            }
        }
        else if (aiState == AiState.Idle)
        {
            if (_time > idleTime)
            {
                aiState = AiState.Patrol;
            }
            else
                _time += Time.deltaTime;
        }
        else if (aiState == AiState.Chase)
        {
            if (target != null)
            {
                if (Vector3.Distance(transform.position, target.transform.position) < attackRange)
                {
                    _agent.Stop();
                    if (_animator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"))
                    {
                        _animator.Play("Attack");
                        Attack();
                        transform.LookAt(target.transform);
                    }
                }
            }
        }
        else if (aiState == AiState.Dead)
        {
            if (!isDead)
            {
                if (deadEffect != null)
                    Instantiate(deadEffect[Random.Range(0, deadEffect.Length)], transform.position, Quaternion.identity);
                target = null;
                _agent.Stop();
                _animator.Play("Dead");
                GetComponent<CharacterController>().enabled = false;
                GetComponent<SphereCollider>().enabled = false;
                //DropTreasure();
                Destroy(gameObject, 5f);
                isDead = true;
            }
        }

        if (health <= 0)
        {
            aiState = AiState.Dead;
        }

        //if (target != null)
        //{
        //    if (Vector3.Distance(transform.position, target.transform.position) > chaseRange)
        //    {
        //        target = null;
        //        aiState = AiState.Idle;
        //    }
        //    else
        //        aiState = AiState.Chase;
        //}
    }

    void DropTreasure()
    {
        foreach (GameObject t in drops)
        {
            if (Random.value > 0.5f)
            {
                //
                return;
            }
        }
    }

    public Vector2 GetHealthBar()
    {
        Vector2 _hp = new Vector2(health, maxHealth);
        return _hp;
    }

    void GetHit(float damage)
    {
        if (aiState != AiState.Dead)
        {
            //Debug.Log("Getting hit");
            if (health - damage > 0)
                health -= damage;
            else
                health = 0;
        }
    }

    void FixedUpdate()
    {
        if (aiState == AiState.Chase)
        {
            if (target != null)
            {
                _agent.destination = target.transform.position;
            }
        }
    }

    void Attack()
    {
        //Debug.Log("Enemy attacking");
        if (target != null)
        {
            if (Vector3.Distance(transform.position, target.transform.position) < attackRange)
            {
                float _dmg = Mathf.RoundToInt(Random.Range(-damage / 10, damage / 10)) + damage;
                target.SendMessage("GetHit", _dmg);
                GameManager.CreateDamageText(target, _dmg.ToString(), Color.red, Vector3.up * 8f);
            }
        }
    }

    void SetTarget(GameObject player)
    {
        if (aiState != AiState.Dead)
        {
            if (!target)
            {
                target = player;
                aiState = AiState.Chase;
            }
        }
    }

    void RemoveTarget()
    {
        if (aiState != AiState.Dead)
        {
            if (target)
            {
                target = null;
                aiState = AiState.Idle;
            }
        }
    }

    void SetPatrolPoint()
    {
        Vector3 _point = originalPosition + new Vector3(Random.Range(-patrolRange, patrolRange), 0, Random.Range(-patrolRange, patrolRange));
        patrolPoint = _point;
        _agent.destination = _point;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(patrolPoint, 0.5f);
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        if (Application.isPlaying)
            Gizmos.DrawSphere(originalPosition, patrolRange);
        else
            Gizmos.DrawSphere(transform.position, patrolRange);
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        //Gizmos.DrawSphere(transform.position, chaseRange);
    }
}
