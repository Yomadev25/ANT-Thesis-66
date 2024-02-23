using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class EnemyCombatState : EnemyBaseState
{
    Enemy enemy;
    public EnemyCombatState(EnemyStateMachine ctx) : base(ctx) { }

    public override void Enter()
    {
        MessagingCenter.Subscribe<EnemyManager>(this, EnemyManager.MessageOnEnemyDead, (sender) =>
        {
            if (sender.stateMachine == _context)
            {
                ChangeState(_context.State.Idle());
            }
        });

        enemy = _context.Enemy;
        _context.Anim.SetFloat("Speed", 0);
        enemy.combos[_context.ComboCount].Execute(_context, this);
    }

    public override void Update()
    {
        
    }

    public override void FixedUpdate()
    {

    }

    public void OnAttacked()
    {
        if (_context.CurrentState != this) return;
        ChangeState(_context.State.Chase());
    }

    public override void Exit()
    {
        _context.Anim.applyRootMotion = false;
        _context.ResetCombatCooldown(enemy.combos[_context.ComboCount].cooldown);
        enemy = null;

        MessagingCenter.Unsubscribe<EnemyManager>(this, EnemyManager.MessageOnEnemyDead);
    }
}
