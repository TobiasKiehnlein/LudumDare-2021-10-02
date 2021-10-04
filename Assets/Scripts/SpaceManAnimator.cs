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
    private const string JumpStr = "Jump";
    private const string LandEasyStr = "EasyLanding";
    private const string LandMiddleStr = "Landing";
    private const string LandHardStr = "HardLanding";
    private const string DeadStr = "Dead";

    // States of the animator
    public enum AnimatorState
    {
        Falling,
        Jump,
        FreeFalling,
        StandingUp,
        Standing,
        Walking,
        Running,
        Dead,
        Transition,
        None
    }

    // Animation triggered externally
    public enum AnimationState
    {
        Stand,
        Walk,
        Run,
        Fall,
        FreeFall,
        Jump,
        LandEasy,
        LandMiddle,
        LandHard,
        Dead
    }

    public AnimationState LastAnimationState => _lastAnimationState;

    private AnimationState _lastAnimationState = AnimationState.Fall;

    // Animator parameters that can be triggered/changed
    private enum SpaceManControl
    {
        Moving,
        Walk,
        Run,
        Fall,
        FreeFall,
        Jump,
        LandEasy,
        LandMiddle,
        LandHard,
        Dead
    }

    private readonly Dictionary<string, AnimatorState> _animationToAnimatorStates =
        new Dictionary<string, AnimatorState>
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
            {"Jumping Up", AnimatorState.Jump},
            {"Dead", AnimatorState.Dead}
        };

    private Dictionary<int, AnimatorState> _animationHashToAnimatorStates;

    private readonly Dictionary<string, SpaceManControl> _stringToControl = new Dictionary<string, SpaceManControl>()
    {
        {MovingStr, SpaceManControl.Moving},
        {WalkStr, SpaceManControl.Walk},
        {RunStr, SpaceManControl.Run},
        {FallStr, SpaceManControl.Fall},
        {FreeFallStr, SpaceManControl.FreeFall},
        {JumpStr, SpaceManControl.Jump},
        {LandEasyStr, SpaceManControl.LandEasy},
        {LandMiddleStr, SpaceManControl.LandMiddle},
        {LandHardStr, SpaceManControl.LandHard},
        {DeadStr, SpaceManControl.Dead}
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
        return currentState != AnimatorState.StandingUp && currentState != AnimatorState.FreeFalling &&
               currentState != AnimatorState.Dead;
    }

    public void Animate(AnimationState animationState, bool force = false)
    {
        var currentState = GetCurrentState();
        if (!force)
        {
            if (_lastAnimationState == animationState) return;

            if ((currentState == AnimatorState.Dead) || (currentState == AnimatorState.Transition &&
                                                         _lastAnimationState == AnimationState.Dead))
                return; // if dead or in transition to it: do not animate
            
        }

        var updatedState = false;

        switch (animationState)
        {
            case AnimationState.Fall:
                if (currentState == AnimatorState.Jump || currentState == AnimatorState.StandingUp) break;
                ResetTriggers();
                _animator.SetTrigger(_controlToId[SpaceManControl.Fall]);
                updatedState = true;
                break;
            case AnimationState.FreeFall:
                _animator.SetTrigger(_controlToId[SpaceManControl.FreeFall]);
                updatedState = true;
                break;
            case AnimationState.Jump:
                _animator.SetTrigger(_controlToId[SpaceManControl.Jump]);
                updatedState = true;
                break;
            case AnimationState.LandEasy:
                _animator.SetTrigger(_controlToId[SpaceManControl.LandEasy]);
                _animator.ResetTrigger(_controlToId[SpaceManControl.Fall]);
                updatedState = true;
                break;
            case AnimationState.LandMiddle:
                _animator.SetTrigger(_controlToId[SpaceManControl.LandMiddle]);
                _animator.ResetTrigger(_controlToId[SpaceManControl.Fall]);
                updatedState = true;
                break;
            case AnimationState.LandHard:
                _animator.SetTrigger(_controlToId[SpaceManControl.LandHard]);
                _animator.ResetTrigger(_controlToId[SpaceManControl.Fall]);
                updatedState = true;
                break;
            case AnimationState.Stand:
                _animator.SetBool(_controlToId[SpaceManControl.Moving], false);
                updatedState = true;
                break;
            case AnimationState.Walk:
                _animator.SetBool(_controlToId[SpaceManControl.Moving], true);
                _animator.SetTrigger(_controlToId[SpaceManControl.Walk]);
                _animator.ResetTrigger(_controlToId[SpaceManControl.Run]);
                updatedState = true;
                break;
            case AnimationState.Run:
                _animator.SetBool(_controlToId[SpaceManControl.Moving], true);
                _animator.SetTrigger(_controlToId[SpaceManControl.Run]);
                _animator.ResetTrigger(_controlToId[SpaceManControl.Walk]);
                updatedState = true;
                break;
            case AnimationState.Dead:
                _animator.SetBool(_controlToId[SpaceManControl.Dead], true);
                break;
        }

        if (updatedState) _lastAnimationState = animationState;
    }

    public AnimatorState GetCurrentState(bool noTransition = false)
    {
        AnimatorStateInfo stateInfo;
        if (_animator.IsInTransition(0)) stateInfo = _animator.GetNextAnimatorStateInfo(0);
        else stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
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