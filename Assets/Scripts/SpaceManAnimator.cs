using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceManAnimator : MonoBehaviour
{
    private Animator _animator;

    private const string MovingStr = "Moving";
    private const string WalkStr = "Walk";
    private const string RunStr = "Run";
    private const string FallStr = "Fall";
    private const string FreeFallStr = "FreeFall";
    private const string FloatStr = "Float";
    private const string JumpStr = "Jump";
    private const string LandEasyStr = "EasyLanding";
    private const string LandMiddleStr = "Landing";
    private const string LandHardStr = "HardLanding";

    private enum AnimationState
    {
        Falling,
        StandingUp,
        Standing,
        Walking,
        Running,
        Floating
    }

    private Dictionary<AnimationState, List<string>> _statesToAnimation = new Dictionary<AnimationState, List<string>>
    {
        {AnimationState.Falling, new List<string> {"Falling", "Falling Idle"}},
        {AnimationState.StandingUp, new List<string> {"Falling Flat Impact", "Jumping Down", "Falling To Landing", "Hard Landing"}},
        {AnimationState.Standing, new List<string> {"Idle"}},
        {AnimationState.Walking, new List<string> {"Walking"}},
        {AnimationState.Running, new List<string> {"Running"}},
        {AnimationState.Floating, new List<string> {"Floating"}}
    };

    private Dictionary<AnimationState, List<int>> _statesToAnimationHash;

    private AnimationState currentState = AnimationState.Falling;
    

    public void Start()
    {
        _animator = this.GetComponentInChildren<Animator>();
        foreach (var state in _statesToAnimation)
        {
            _statesToAnimationHash.Add(state.Key, state.Value.ConvertAll(str => Animator.StringToHash(str)));
        }
    }

    public void Fall()
    {
        _animator.SetTrigger(FallStr);
    }

    public void Float()
    {
        if (currentState != AnimationState.Floating && currentState != AnimationState.StandingUp)
        {
            _animator.SetTrigger(FloatStr);
        }
    }

    public void Stand()
    {
        if (currentState != AnimationState.Standing && currentState != AnimationState.StandingUp)
        {
            _animator.ResetTrigger(WalkStr);
            _animator.ResetTrigger(RunStr);
            _animator.SetBool(MovingStr, false);
        }
    }

    public void Walk()
    {
        if (currentState != AnimationState.Walking)
        {
            _animator.SetTrigger(WalkStr);
            _animator.ResetTrigger(RunStr);
            _animator.SetBool(MovingStr, true);
        }
    }

    public void Run()
    {
        if (currentState != AnimationState.Running)
        {
            _animator.SetTrigger(RunStr);
            _animator.ResetTrigger(WalkStr);
            _animator.SetBool(MovingStr, true);
        }
    }

    public void Jump()
    {
        _animator.SetTrigger(JumpStr);
    }

    public void LandEasy()
    {
        if (currentState != AnimationState.StandingUp)
        {
            _animator.SetTrigger(LandEasyStr);
        }
    }

    public void LandMiddle()
    {
        if (currentState != AnimationState.StandingUp)
        {
            _animator.SetTrigger(LandMiddleStr);
        }
    }

    public void LandHard()
    {
        if (currentState != AnimationState.StandingUp)
        {
            _animator.SetTrigger(LandHardStr);
        }
    }

    public void FreeFall()
    {
        _animator.SetTrigger(FreeFallStr);
    }

    public bool CanMove()
    {
        return currentState != AnimationState.StandingUp;
    }
/*
    private AnimationState GetCurrentState()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        foreach (var VARIABLE in _animator)
        {
            _statesToAnimationHash.
        }
    }
    
    public string GetCurrentAnimatorStateName()
    {
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

        string stateName;
        if (NameTable.TryGetValue(stateInfo.shortNameHash, out stateName))
        {
            return stateName;
        }
        else
        {
            Debug.LogWarning("Unknown animator state name.");
            return string.Empty;
        }
    }*/
}
