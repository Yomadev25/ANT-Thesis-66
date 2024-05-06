using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour, IDamageable
{
    public const string MessageOnEnemyAppeared = "On Enemy Appeared";
    public const string MessageOnUpdateHp = "On Update Hp";
    public const string MessageOnEnemyDead = "On Enemy Dead";

    [Header("Enemy Profile")]
    [SerializeField]
    private Enemy _enemy;
    [SerializeField]
    private EnemyStateMachine _enemyStateMachine;

    [Header("Properties")]
    [SerializeField]
    private float _maxHp;
    [SerializeField]
    private float _hp;

    [Header("References")]
    [SerializeField]
    private Animator _anim;
    private Collider _collider;

    [Header("Sound Effects")]
    [SerializeField]
    private string _hitSfx;

    [Header("Events")]
    [SerializeField]
    private UnityEvent onTakeDamage;
    [SerializeField]
    private UnityEvent onHeal;
    [SerializeField]
    private UnityEvent onDead;

    bool isDie;

    #region PUBLIC VARIABLES
    public Enemy Enemy => _enemy;
    public EnemyStateMachine stateMachine => _enemyStateMachine;
    public float hp => _hp;
    public float maxHp => _maxHp;
    #endregion

    private void Start()
    {
        _hp = _maxHp;
        _collider = GetComponent<Collider>();
        MessagingCenter.Send(this, MessageOnUpdateHp);
        MessagingCenter.Send(this, MessageOnEnemyAppeared);
    }

    private void Update()
    {
        if (_hp <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage, GameObject effect = null, bool impact = false)
    {
        if (isDie) return;

        if (impact)
        {
            TimeStop.Instance.StopTime(0.05f, 10, 0.1f);
        }
        else
        {
            if (damage >= _hp)
            {
                TimeStop.Instance.StopTime(0.05f, 10, 0.1f);
                AudioManager.Instance.PlaySFX("Kill");

                Destroy(EffectManager.Instance.Spawn("Kill Impact", transform.position + Vector3.up, Quaternion.identity), 1f);

                if (_enemy.name != "Scarecrow")
                {
                    GameObject blood = EffectManager.Instance.Spawn("Kill", transform.position, Quaternion.identity);
                    blood.transform.parent = transform;
                    blood.transform.localPosition = Vector3.zero + Vector3.up;
                    blood.transform.localEulerAngles = Vector3.zero;
                    Destroy(blood, 1.5f);
                }
                
            }
        }

        _hp -= damage;
        _anim.SetTrigger("Hit");
        AudioManager.Instance.PlaySFX(_hitSfx);

        if (effect != null)
        {
            Instantiate(effect, transform.position, Quaternion.identity);
        }
        

        onTakeDamage?.Invoke();
        MessagingCenter.Send(this, MessageOnUpdateHp);
    }

    private void Die()
    {
        if (isDie) return;
        isDie = true;
 
        _enemyStateMachine.enabled = false;
        _anim.applyRootMotion = false;
        _anim.SetLayerWeight(1, 0);
        _anim.SetTrigger("Die");
        _collider.enabled = false;

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.Spawn("Soul", this.transform.position, Quaternion.identity);
        }

        onDead?.Invoke();
        MessagingCenter.Send(this, MessageOnEnemyDead);
        Destroy(this.gameObject, 4f);
    }
}
