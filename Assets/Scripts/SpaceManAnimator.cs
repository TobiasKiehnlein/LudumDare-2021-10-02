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

    public enum AnimatorState
    {
        Falling,
        FreeFalling,
        StandingUp,
        Standing,
        Walking,
        Running,
        Floating,
        Transition,
        None
    }

    public enum AnimationState
    {
        Stand,
        Walk,
        Run,
        Fall,
        FreeFall,
        Float,
        Jump,
        LandEasy,
        LandMiddle,
        LandHard
    }

    private AnimationState _lastAnimationState = AnimationState.Fall;

    public enum SpaceManControl
    {
        Moving,
        Walk,
        Run,
        Fall,
        FreeFall,
        Float,
        Jump,
        LandEasy,
        LandMiddle,
        LandHard
    }

    private readonly Dictionary<string, AnimatorState> _animationToAnimatorStates = new Dictionary<string, AnimatorState>
    {
        {"Falling", AnimatorState.FreeFalling},
        {"Falling Idle", AnimatorState.Falling},
        {"Falling Flat Impact", AnimatorState.StandingUp},
        {"Standing Up", AnimatorState.StandingUp},
        {"Jumping Down", AnimatorState.StandingUp},
        {"Falling To Landing", AnimatorState.StandingUp},
        {"Hard Landing", AnimatorState.StandingUp},
        {"Idle", AnimatorState.Standing},
        {"Walking", AnimatorState.Walking},
        {"Running", AnimatorState.Running},
        {"Floating", AnimatorState.Floating},
        {"Jumping Up", AnimatorState.Floating}
    };

    private Dictionary<int, AnimatorState> _animationHashToAnimatorStates;

    private readonly Dictionary<string, SpaceManControl> _stringToControl = new Dictionary<string, SpaceManControl>()
    {
        {MovingStr, SpaceManControl.Moving},
        {WalkStr, SpaceManControl.Walk},
        {RunStr, SpaceManControl.Run},
        {FallStr, SpaceManControl.Fall},
        {FreeFallStr, SpaceManControl.FreeFall},
        {FloatStr, SpaceManControl.Float},
        {JumpStr, SpaceManControl.Jump},
        {LandEasyStr, SpaceManControl.LandEasy},
        {LandMiddleStr, SpaceManControl.LandMiddle},
        {LandHardStr, SpaceManControl.LandHard}
    };

    private Dictionary<SpaceManControl, int> _controlToId;


    public void Start()
    {
        _animator = this.GetComponentInChildren<Animator>();
        _animationHashToAnimatorStates = new Dictionary<int, AnimatorState>();
        foreach (var animationStr in _animationToAnimatorStates)
        {
            _animationHashToAnimatorStates.Add(Animator.StringToHash(animationStr.Key), animationStr.Value);
        }

        _controlToId = new Dictionary<SpaceManControl, int>();
        foreach (var parameter in _animator.parameters)
        {
            if (_stringToControl.TryGetValue(parameter.name, out var control))
            {
                _controlToId.Add(control, parameter.nameHash);
            }
            else
            {
                Debug.LogError($"Warning: Unknown animator parameter: {parameter.name}");
            }
        }
    }

    public bool CanMove()
    {
        var currentState = GetCurrentState();
        return currentState != AnimatorState.StandingUp && currentState != AnimatorState.FreeFalling;
    }

    public void Animate(AnimationState animationState)
    {
        if (_lastAnimationState == animationState) return;
        
        
        var currentState = GetCurrentState();

        if ((currentState == AnimatorState.Transition || currentState == AnimatorState.StandingUp) &&
            (_lastAnimationState == AnimationState.LandEasy || _lastAnimationState == AnimationState.LandMiddle ||
             _lastAnimationState == AnimationState.LandHard))
            return;

        var updatedState = false;
        
        switch (animationState)
        {
            case AnimationState.Fall:
                if (currentState != AnimatorState.StandingUp)
                {
                    _animator.SetTrigger(_controlToId[SpaceManControl.Fall]);
                    updatedState = true;
                }

                break;
            case AnimationState.Float:
                if (currentState != AnimatorState.StandingUp)
                {
                    _animator.SetTrigger(_controlToId[SpaceManControl.Float]);
                    updatedState = true;
                }

                break;
            case AnimationState.FreeFall:
                if (currentState != AnimatorState.StandingUp)
                {
                    _animator.SetTrigger(_controlToId[SpaceManControl.FreeFall]);
                    updatedState = true;
                }

                break;
            case AnimationState.Jump:
                _animator.SetTrigger(_controlToId[SpaceManControl.Jump]);
                updatedState = true;
                break;
            case AnimationState.LandEasy:
                _animator.SetTrigger(_controlToId[SpaceManControl.LandEasy]);
                updatedState = true;
                break;
            case AnimationState.LandMiddle:
                _animator.SetTrigger(_controlToId[SpaceManControl.LandMiddle]);
                updatedState = true;
                break;
            case AnimationState.LandHard:
                _animator.SetTrigger(_controlToId[SpaceManControl.LandHard]);
                updatedState = true;
                break;
            case AnimationState.Stand:
                if (currentState != AnimatorState.StandingUp)
                {
                    _animator.SetBool(_controlToId[SpaceManControl.Moving], false);
                    updatedState = true;
                }

                break;
            case AnimationState.Walk:
                if (currentState != AnimatorState.StandingUp)
                {
                    _animator.SetBool(_controlToId[SpaceManControl.Moving], true);
                    _animator.SetTrigger(_controlToId[SpaceManControl.Walk]);
                    _animator.ResetTrigger(_controlToId[SpaceManControl.Run]);
                    updatedState = true;
                }

                break;
            case AnimationState.Run:
                if (currentState != AnimatorState.StandingUp)
                {
                    _animator.SetBool(_controlToId[SpaceManControl.Moving], true);
                    _animator.SetTrigger(_controlToId[SpaceManControl.Run]);
                    _animator.ResetTrigger(_controlToId[SpaceManControl.Walk]);
                    updatedState = true;
                }

                break;
        }

        if (updatedState) _lastAnimationState = animationState;
    }

    public AnimatorState GetCurrentState()
    {
        if (_animator.IsInTransition(0)) return AnimatorState.Transition;
        
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (_animationHashToAnimatorStates.TryGetValue(stateInfo.shortNameHash, out var state))
        {
            return state;
        }

        Debug.LogError($"Warning: Unknown animation: {stateInfo.shortNameHash}");

        return AnimatorState.None;
    }

    private void ResetTriggers()
    {
        foreach (var parameter in _animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger)
            {
                _animator.ResetTrigger(parameter.nameHash);
            }
        }
    }
}