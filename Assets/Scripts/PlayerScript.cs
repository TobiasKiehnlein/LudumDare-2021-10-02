using System;
using Enums;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerScript : MonoBehaviour
{
    #region SerializeFields

    [SerializeField] private float jumpForce = 400.0f;
    [SerializeField] private float movingAcceleration = 5.0f;
    [SerializeField] private float runningAcceleration = 7.0f;
    [SerializeField] private float airForceMultiplier = 5f;
    [SerializeField] private float maxDistanceToWallUntilBlocked = 1.5f;
    [SerializeField] private float turnSpeed = 5f;

    [SerializeField] private float scalingFactorSpeedInputStop = .5f;

    //disabled to create consistent Naming
    //  [SerializeField] private float maxAirBoostSpeed = 5f;
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private float vMinTurn;
    [SerializeField] private float vMaxAcceleratableFloor = 9f;
    [SerializeField] private float vMaxAcceleratableAir = 5f;
    [SerializeField] private float walkT = 0.2f;
    [SerializeField] private float runT = 3f;
    [SerializeField] private float fallT = 1f;
    [SerializeField] private float freeFallT = 20f;
    [SerializeField] private float landMiddleT = 2f;
    [SerializeField] private float landHardT = 10f;
    [SerializeField] private float oxygenConsumptionMoving;
    [SerializeField] private float oxygenConsumptionRunning;
    [SerializeField] private float oxygenConsumptionIdle;
    [SerializeField] private float oxygenConsumptionJump;
    [SerializeField] private float spaceManRotationSpeed = 10f;

    #endregion

    #region Members
    private Vector2 _vel = Vector2.zero;
    private bool IsGrounded => _groundCollisions > 0;

    private float _horizontalForce;

    private bool _jumpAllowed;
    private float _jumpLock;

    private Rigidbody2D _rg;
    // private Vector2 _vel = new Vector2(0, 0);

    private SpaceManAnimator _animator;

    private float _spaceManRotationGoal = 0f;

    private bool _flagOxygenApplied;
    private bool _flagRaycastWallProximityFound = false;

    private int _groundCollisions = 0;

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        _rg = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<SpaceManAnimator>();
    }

    // Update is called once per frame
    private void Update()
    {
        _flagRaycastWallProximityFound = false;
        _flagOxygenApplied = false;

        _jumpLock -= Time.deltaTime;
        if (_jumpLock < 0)
        {
            _jumpLock = 0f;
            _jumpAllowed = true;
        }

        var animatorCanMove = _animator.CanMove();
        var horizontalInput = animatorCanMove ? Input.GetAxis("Horizontal") : 0;
        var heightInput = animatorCanMove ? (int) Input.GetAxis("Jump") : 0;
        horizontalInput *= RayCheck(horizontalInput);
        // Debug.Log(Vector2.Perpendicular(Physics2D.gravity));
        //   Debug.Log(horizontalInput);
        if (!IsGrounded)
        {
            if (!IsGrounded)
            {
                _horizontalForce = horizontalInput * airForceMultiplier * movingAcceleration;
                _vel = gameSettings.GravityOrientation switch
                {
                    Orientation.Up => Vector2.left,
                    Orientation.Down => Vector2.right,
                    Orientation.Left => Vector2.down,
                    Orientation.Right => Vector2.up,
                    _ => Vector2.zero
                };
                if (_horizontalForce > Mathf.Epsilon)
                {
                    gameSettings.oxygenCurrent -= oxygenConsumptionMoving * Time.deltaTime;
                    _flagOxygenApplied = true;
                }

               _vel *= _horizontalForce;
               _vel += Physics2D.gravity.normalized *
                   Vector2.Dot(Physics2D.gravity.normalized, _rg.velocity);

               _rg.velocity = _vel;
                //TODO PETER: Updaten! Stimmt das noch?
                var absVerticalVelocityAligned = Mathf.Abs(GetVerticalVelocityAligned());
                if (absVerticalVelocityAligned < fallT)
                {
                    _animator.Animate(SpaceManAnimator.AnimationState.Float);
                }
                else if (absVerticalVelocityAligned < freeFallT)
                {
                    _animator.Animate(SpaceManAnimator.AnimationState.Fall);
                }
                else
                {
                    _animator.Animate(SpaceManAnimator.AnimationState.FreeFall);
                }
            }

            horizontalMovementDirection *= _horizontalForce;
            _rg.AddForce(horizontalMovementDirection);

            var absVerticalVelocityAligned = Mathf.Abs(GetVerticalVelocityAligned());
            if (absVerticalVelocityAligned < fallT)
            {
                _animator.Animate(SpaceManAnimator.AnimationState.Float);
                AudioManager.Instance.StartSound(Music.MediumDrums,2f);
            }
            else if (absVerticalVelocityAligned < freeFallT)
            {
                _animator.Animate(SpaceManAnimator.AnimationState.Fall);
                AudioManager.Instance.StartSound(Music.MediumDrums,2f);
            }
            else
            {
                _animator.Animate(SpaceManAnimator.AnimationState.FreeFall);
                AudioManager.Instance.StartSound(Music.IntenseDrums,2f);
            }
        }
        else
        {
            _horizontalForce = horizontalInput * UpdateMovementSpeed();
            _flagOxygenApplied = UpdateOxygenConsumption(horizontalInput);


                if (_jumpAllowed && heightInput == 1)
                {
                    switch (gameSettings.GravityOrientation)
                    {
                        case Orientation.Up:
                            _rg.AddForce( Vector2.down *jumpForce * _rg.mass);
                            break;
                        case Orientation.Down:
                            _rg.AddForce( Vector2.up *jumpForce * _rg.mass);
                            break;
                        case Orientation.Left:
                            _rg.AddForce( Vector2.right *jumpForce * _rg.mass);
                            break;
                        case Orientation.Right:
                            _rg.AddForce( Vector2.left *jumpForce * _rg.mass);
                            break;
                        default:
                            _rg.AddForce(new Vector2(0, jumpForce * _rg.mass));
                            break;
                    }

                    _animator.Animate(SpaceManAnimator.AnimationState.Jump);

                    gameSettings.oxygenCurrent -= oxygenConsumptionJump;
                    _flagOxygenApplied = true;

                    _jumpLock = 0.1f;
                    _jumpAllowed = false;
                }

                _vel = gameSettings.GravityOrientation switch
                {
                    Orientation.Up => new Vector2(-_horizontalForce, _rg.velocity.y),
                    Orientation.Down => new Vector2(_horizontalForce, _rg.velocity.y),
                    Orientation.Left => new Vector2(_rg.velocity.x, -_horizontalForce),
                    Orientation.Right => new Vector2(_rg.velocity.x, _horizontalForce),
                    _ => new Vector2(_horizontalForce, _rg.velocity.y)
                };
                _rg.velocity = _vel;
                var absHorizontalVelocityAligned = Mathf.Abs(GetHorizontalVelocityAligned());
                if (Mathf.Abs(horizontalInput) > 0.1 || absHorizontalVelocityAligned >= walkT)
                {
                    if (absHorizontalVelocityAligned < runT)
                    {
                        _animator.Animate(SpaceManAnimator.AnimationState.Walk);
                    }
                    else
                    {
                        _animator.Animate(SpaceManAnimator.AnimationState.Run);
                    }
                }
                else
                {
                    _animator.Animate(SpaceManAnimator.AnimationState.Stand);
                }
            }


            SpaceManAnimator.AnimatorState animatorState = _animator.GetCurrentState();
            float spaceManRotation = _animator.gameObject.transform.localEulerAngles.y;
            if (animatorCanMove)
            {
                if (absHorizontalVelocityAligned < runT)
                {
                    _animator.Animate(SpaceManAnimator.AnimationState.Walk);
                    AudioManager.Instance.StartSound(Music.SilentDrums,2f);
                }
                else if (horizontalInput < 0)
                {
                    _animator.Animate(SpaceManAnimator.AnimationState.Run);
                    AudioManager.Instance.StartSound(Music.MediumDrums,2f);
                }
            }
            else if (animatorState == SpaceManAnimator.AnimatorState.FreeFalling)
            {
                _animator.Animate(SpaceManAnimator.AnimationState.Stand);
                AudioManager.Instance.StartSound(Music.Silent,2f);
            }

            if (Mathf.Abs(_spaceManRotationGoal - spaceManRotation) < 0.05)
            {
                _animator.gameObject.transform.localRotation = Quaternion.Euler(0f, _spaceManRotationGoal, 0f);
            }
            else
            {
                _animator.gameObject.transform.localRotation = Quaternion.Lerp(
                    _animator.gameObject.transform.localRotation,
                    Quaternion.Euler(0f, _spaceManRotationGoal, 0f), Time.deltaTime * spaceManRotationSpeed);
            }

            // Fix grounded animation when not correctly triggered
            if (IsGrounded && (animatorState == SpaceManAnimator.AnimatorState.Falling ||
                               animatorState == SpaceManAnimator.AnimatorState.Floating ||
                               animatorState == SpaceManAnimator.AnimatorState.FreeFalling))
            {
                _animator.Animate(SpaceManAnimator.AnimationState.LandEasy);
            }
        }

        AlignPlayer(false);
        if (_flagOxygenApplied)
        {
        }
        else
        {
            gameSettings.oxygenCurrent -= oxygenConsumptionIdle;
        }

        if (gameSettings.oxygenCurrent <= Mathf.Epsilon)
        {
            Suffocate();
        }
    }

    private float HorizontalInputCheck(float horizontalInput)
    {
        var desiredDirection = Mathf.Sign(horizontalInput) * Vector2.Perpendicular(Physics2D.gravity.normalized);
        var res = Physics2D.Raycast((Vector2) transform.position, desiredDirection, Mathf.Infinity, LayerMask.GetMask("Wall"));
        var vel = _rg.velocity;
        var stopped = res.distance < maxDistanceToWallUntilBlocked;
        Debug.Log(vel);
        vel = (stopped) ? Physics2D.gravity.normalized * Vector2.Dot(Physics2D.gravity.normalized, vel) : vel;
        Debug.Log(vel);
        _rg.velocity = vel;
        _flagRaycastWallProximityFound = res.distance < maxDistanceToWallUntilBlocked;
        return (res.distance < maxDistanceToWallUntilBlocked) ? 0 : 1;
    }


    private Vector2 RescaleToMaxVelocity(Vector2 force)
    {
        var perpendicular = Vector2.Perpendicular(Physics2D.gravity.normalized);
        var currentSpeed = Mathf.Abs(Vector2.Dot(_rg.velocity, perpendicular));
        var factor = ScaleFactor(currentSpeed, vMaxAcceleratableFloor);
        return factor * force;
    }

    //allows easy implementation of more fitting curves
    // private static float ScaleFactor(float current, float targetMax)
    // {
    //     return Math.Abs(current - targetMax) / targetMax;
    // }

    private void OnCollisionEnter2D(Collision2D other)
    {
        var verticalImpactVelocity = Mathf.Abs(Vector2.Dot(Physics2D.gravity.normalized, other.relativeVelocity));

        if (IsGrounded)
        {
            if (verticalImpactVelocity < landMiddleT)
            {
                _animator.Animate(SpaceManAnimator.AnimationState.LandEasy);
            }
            else if (verticalImpactVelocity < landHardT)
            {
                _animator.Animate(SpaceManAnimator.AnimationState.LandMiddle);
            }
            else
            {
                _animator.Animate(SpaceManAnimator.AnimationState.LandHard);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hole")) Ejection();
        if (other.CompareTag("Collidable")) ++_groundCollisions;
        if (other.CompareTag("OxygenTank")) ONOxygenTank(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Collidable") && _groundCollisions > 0) --_groundCollisions;
    }

    private bool UpdateOxygenConsumption(float horizontalInput)
    {
        if (Abs(horizontalInput) < Mathf.Epsilon) return false;
        if (CheckRunButtonUI() || Input.GetKey(KeyCode.LeftShift))
        {
            gameSettings.oxygenCurrent -= oxygenConsumptionRunning;
        }
        else
        {
            gameSettings.oxygenCurrent -= oxygenConsumptionMoving;
        }

        return true;
    }

    private void ONOxygenTank(Component other)
    {
        gameSettings.oxygenCurrent += gameSettings.oxygenTank;
        gameSettings.oxygenCurrent = (gameSettings.oxygenCurrent > gameSettings.oxygenMax)
            ? gameSettings.oxygenMax
            : gameSettings.oxygenCurrent;
        GameObject.Destroy(other.gameObject);
    }

    private void Ejection()
    {
        // a slightly more beautiful solution might be an option for Game över
        Time.timeScale = 0;
    }

    private static float Abs(float input)
    {
        return input < 0f ? -input : input;
    }


    private void AlignPlayer(bool fallbackModeUsed)
    {
        if (fallbackModeUsed)
        {
            transform.rotation = gameSettings.GravityOrientation switch
            {
                Orientation.Down => Quaternion.Euler(0, 0, 0),
                Orientation.Up => Quaternion.Euler(0, 0, 180f),
                Orientation.Left => Quaternion.Euler(0, 0, 270f),
                Orientation.Right => Quaternion.Euler(0, 0, 90f),
                _ => transform.rotation
            };
        }
        else
        {
            float targetAngle = gameSettings.GravityOrientation switch
            {
                Orientation.Up => 180,
                Orientation.Down => 0,
                Orientation.Left => 270,
                Orientation.Right => 90,
                _ => 0
            };
            //  if (Vector2.Dot(Physics2D.gravity.normalized, _rg.velocity) > vMinTurn)
            // {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle),
                turnSpeed * Time.deltaTime);
            //  }
        }
    }

    private float GetHorizontalVelocityAligned()
    {
        return Vector2.Dot(Vector2.Perpendicular(Physics2D.gravity.normalized), _rg.velocity);
    }

    private float GetVerticalVelocityAligned()
    {
        return Vector2.Dot(Physics2D.gravity.normalized, _rg.velocity);
    }

    private float UpdateMovementSpeed()
    {
        if (CheckRunButtonUI() || Input.GetKey(KeyCode.LeftShift))
            return runningAcceleration;

        return movingAcceleration;
    }

    //TODO (For touch support)
    private static bool CheckRunButtonUI()
    {
        return false;
    }

    //Todo.
    private void Suffocate()
    {
        throw new NotImplementedException();
    }
}