using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    Enemy enemy;

    public EnemyIdleState(EnemyStateMachine ctx) : base(ctx) { }

    public override void Enter()
    {
        enemy = _context.Enemy;
    }

    public override void Update()
    {
        _context.Anim.SetFloat("Speed", _context.NavMesh.velocity.magnitude);
        CheckChangeState();
    }

    public override void FixedUpdate()
    {
        
    }

    private void CheckChangeState()
    {
        if (_context.GetVisibleTarget() != null)
        {
            ChangeState(_context.State.Chase());
        }
    }

    public override void Exit()
    {
        enemy = null;
    }
}
