﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]

public abstract class EnemyControllerBase : MonoBehaviour
{
    protected Rigidbody2D _enemyRB;
    protected Animator _enemyAnimator;

    [Header("Canvas")]
    [SerializeField] GameObject _canvas;
    

    [Header("HP")]
    [SerializeField] protected int _maxHP;
    [SerializeField] protected Slider _hpSlider;
    private int _currentHP;
    

    [Header("StateChenges")]
    [SerializeField] private float _maxStateTime;
    [SerializeField] private float _minStateTime;
    [SerializeField] private EnemyState[] _avaliableState;
    protected EnemyState _currentState;
    protected float _lastStateChange;
    protected float _timeToNextChange;

    [Header("Movement")]
    [SerializeField] private float _speed;
    [SerializeField] private float _range;
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private LayerMask _whatIsGround;
    protected Vector2 _startPoint;
    protected bool _faceright = true;

    [Header("DamageDealer")]
    [SerializeField] private DamageType _collisionDamageType;
    [SerializeField] protected int _collisionDamage;
    [SerializeField] protected float _collisionTimeDelay;
    private float _lastDamageTime;

    #region Unity Methods
    protected virtual void Start()
    {
        _startPoint = transform.position;
        _enemyRB = GetComponent<Rigidbody2D>();
        _enemyAnimator = GetComponent<Animator>();
        _currentHP = _maxHP;
        _hpSlider.maxValue = _maxHP;
        _hpSlider.value = _maxHP;
    }

    
    protected virtual void FixedUpdate()
    {
        if (_currentState == EnemyState.Death)
            return;

        if (IsGroundEnding())
            Flip();

        if(_currentState==EnemyState.Move)
            Move();
    }

    protected virtual void Update()
    {
        if (_currentState == EnemyState.Death)
            return;

        if (Time.time - _lastStateChange > _timeToNextChange)
            GetRandomState();
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (_currentState == EnemyState.Death)
            return;

        TryToDamage(collision.collider);
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_range * 2, 0.5f, 0));
    }
    #endregion

    #region Public Methods
    public virtual void TakeDamage(int damage, DamageType type = DamageType.Casual, Transform player = null)
    {
        if (_currentState == EnemyState.Death)
            return;

        
       
        if (_currentHP -damage > 0)
        {
            _currentHP -= damage;
            _enemyAnimator.SetBool("Hurt", true);
        }
        else 
        {
            _currentHP = 0;
            ChangeState(EnemyState.Death);
            return;
        }
        Debug.Log(String.Format("Enemy {0} take damage {1} and his currentHP = {2}", gameObject, damage, _currentHP));
        _hpSlider.value = _currentHP;
            
    }

    protected virtual void Barier()
    {
        return;
    }

    public virtual void OnDeath()
    {
        Barier();
        Destroy(gameObject);
    }
    #endregion

    #region Private Methods

    protected virtual void ChangeState(EnemyState state)
    {
        if (_currentState == EnemyState.Death)
            return;

        ResetState();
        _currentState = EnemyState.Idle;

        if (state != EnemyState.Idle)
            _enemyAnimator.SetBool(state.ToString(), true);

        _currentState = state;
        _lastStateChange = Time.time;

        switch (_currentState)
        {
            case EnemyState.Idle:
                _enemyRB.velocity = Vector2.zero;
                break;
            case EnemyState.Death:
                DisableEnemy();
                break;
        }
    }

    protected virtual void EndState()
    {
        if (_currentState == EnemyState.Death)
            OnDeath();

        ResetState();
    }

    protected void GetRandomState()
    {
        int state = Random.Range(0, _avaliableState.Length);

        if (_currentState == EnemyState.Idle && _avaliableState[state] == EnemyState.Idle)
        {
            GetRandomState();
        }


        _timeToNextChange = Random.Range(_minStateTime, _maxStateTime);
        ChangeState(_avaliableState[state]);

    }

    

    protected virtual void TryToDamage(Collider2D enemy)
    {
        if ((Time.time - _lastDamageTime) < _collisionTimeDelay)
            return;

        Player_Controller player = enemy.GetComponent<Player_Controller>();
        if (player != null)
        {
            player.Takedamage(_collisionDamage, _collisionDamageType, transform);
            _lastDamageTime = Time.time;
        }
            
    }
    protected virtual void Move()
    {
        if (_currentState == EnemyState.Death)
            return;
        _enemyRB.velocity = transform.right * new Vector2(_speed, _enemyRB.velocity.y);
    }

    protected void Flip()
    {
        if (_currentState == EnemyState.Death)
            return;
        _faceright = !_faceright;
        transform.Rotate(0, 180, 0);
        _canvas.transform.Rotate(0, 180, 0);
    }

    

    private bool IsGroundEnding()
    {
        return !Physics2D.OverlapPoint(_groundCheck.position, _whatIsGround);
    }

    

    protected virtual void ResetState()
    {
        _enemyAnimator.SetBool(EnemyState.Move.ToString(), false);
        _enemyAnimator.SetBool(EnemyState.Death.ToString(), false);
    }

    protected virtual void DisableEnemy()
    {
        _enemyRB.velocity = Vector2.zero;
        _enemyRB.bodyType = RigidbodyType2D.Static;
        GetComponent<Collider2D>().enabled = false;
    }

    #endregion
}

public enum EnemyState
{
    Idle,
    Move,
    Strike,
    PowerStrike,
    Shoot,
    Death,
    Hurt,
}
