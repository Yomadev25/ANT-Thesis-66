using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour, IDamageable
{
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

    [Header("Events")]
    [SerializeField]
    private UnityEvent onTakeDamage;
    [SerializeField]
    private UnityEvent onHeal;

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
        MessagingCenter.Send(this, MessageOnUpdateHp);
    }

    private void Update()
    {
        if (_hp <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDie) return;

        _hp -= damage;
        _anim.SetTrigger("Hit");

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

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.Spawn("Soul", this.transform.position, Quaternion.identity);
        }

        MessagingCenter.Send(this, MessageOnEnemyDead);
        Destroy(this.gameObject, 4f);
    }
}
