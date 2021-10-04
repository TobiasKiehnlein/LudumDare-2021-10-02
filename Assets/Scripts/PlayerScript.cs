using System;
using Enums;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [SerializeField] private ScoreStatistics scoreStatistics;

    #endregion

    #region Members

    private Vector2 _vel = Vector2.zero;
    private bool IsGrounded => _groundCollisions > 0;
    private float _horizontalForce;
    private bool _jumpAllowed;
    private float _jumpLock;
    private Rigidbody2D _rg;
    private SpaceManAnimator _animator;
    private float _spaceManRotationGoal = 0f;
    private bool _flagOxygenApplied;
    private bool _flagRaycastWallProximityFound = false;
    private int _groundCollisions = 0;
    private bool animatorCanMove;
    private float horizontalInput;
    private int heightInput;

    #endregion

    private void Start()
    {
        scoreStatistics.countJumping = 0;
        scoreStatistics.timeRunning = 0;
        scoreStatistics.timeIdleOrFloating = 0;
        scoreStatistics.timeWalkingInAir = 0;
        scoreStatistics.timeWalking = 0;
        scoreStatistics.numberOfHardCrashes = 0;
        scoreStatistics.gameWon = false;
        _rg = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<SpaceManAnimator>();
    }


    #region MovementImpementation

    private void MoveInAir()
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
        if (Mathf.Approximately(Vector2.Dot(Physics2D.gravity.normalized, _rg.velocity),
            Vector2.Dot(Vector2.one, _rg.velocity)))
        {
            scoreStatistics.timeIdleOrFloating += Time.deltaTime;
        }
        else
        {
            scoreStatistics.timeWalkingInAir += Time.deltaTime;
        }
    }


    private void Jump()
    {
        switch (gameSettings.GravityOrientation)
        {
            case Orientation.Up:
                _rg.AddForce(Vector2.down * jumpForce * _rg.mass);
                break;
            case Orientation.Down:
                _rg.AddForce(Vector2.up * jumpForce * _rg.mass);
                break;
            case Orientation.Left:
                _rg.AddForce(Vector2.right * jumpForce * _rg.mass);
                break;
            case Orientation.Right:
                _rg.AddForce(Vector2.left * jumpForce * _rg.mass);
                break;
            default:
                _rg.AddForce(new Vector2(0, jumpForce * _rg.mass));
                break;
        }

        _animator.Animate(SpaceManAnimator.AnimationState.Jump);

        gameSettings.oxygenCurrent -= oxygenConsumptionJump;
        _flagOxygenApplied = true;
        scoreStatistics.countJumping++;
        _jumpLock = 0.1f;
        _jumpAllowed = false;
    }

    private void Walk()
    {
        _vel = gameSettings.GravityOrientation switch
        {
            Orientation.Up => new Vector2(-_horizontalForce, _rg.velocity.y),
            Orientation.Down => new Vector2(_horizontalForce, _rg.velocity.y),
            Orientation.Left => new Vector2(_rg.velocity.x, -_horizontalForce),
            Orientation.Right => new Vector2(_rg.velocity.x, _horizontalForce),
            _ => new Vector2(_horizontalForce, _rg.velocity.y)
        };
        _rg.velocity = _vel;
    }

    #endregion

    #region AnimationTrigger

    private void AnimateAirMovement()
    {
        AudioManager.Instance.StopSound(Sfx.FootSteps);
        var absVerticalVelocityAligned = Mathf.Abs(GetVerticalVelocityAligned());
        if (absVerticalVelocityAligned < fallT)
        {
            _animator.Animate(SpaceManAnimator.AnimationState.Float);
            AudioManager.Instance.StartSound(Music.MediumDrums, 2f);
        }
        else if (absVerticalVelocityAligned < freeFallT)
        {
            _animator.Animate(SpaceManAnimator.AnimationState.Fall);
            AudioManager.Instance.StartSound(Music.MediumDrums, 2f);
        }
        else
        {
            _animator.Animate(SpaceManAnimator.AnimationState.FreeFall);
            AudioManager.Instance.StartSound(Music.IntenseDrums, 2f);
        }
    }

    private void AnimateWalkMovement()
    {
        var absHorizontalVelocityAligned = Mathf.Abs(GetHorizontalVelocityAligned());
        if (Mathf.Abs(horizontalInput) > 0.1 || absHorizontalVelocityAligned >= walkT)
        {
            if (absHorizontalVelocityAligned < runT)
            {
                _animator.Animate(SpaceManAnimator.AnimationState.Walk);
                AudioManager.Instance.StartSound(Sfx.FootSteps);
                AudioManager.Instance.StartSound(Music.MediumDrums, 2f);
                scoreStatistics.timeWalking += Time.deltaTime;
            }
            else
            {
                _animator.Animate(SpaceManAnimator.AnimationState.Run);
                AudioManager.Instance.StartSound(Music.IntenseDrums, 2f);
                AudioManager.Instance.StartSound(Sfx.FootSteps);
                scoreStatistics.timeRunning += Time.deltaTime;
            }
        }
        else
        {
            _animator.Animate(SpaceManAnimator.AnimationState.Stand);
            AudioManager.Instance.StartSound(Music.SilentDrums, 2f);
            AudioManager.Instance.StopSound(Sfx.FootSteps);
            scoreStatistics.timeIdleOrFloating += Time.deltaTime;
        }
    }

    private void MiscAnimationStuff()
    {
        var animatorState = _animator.GetCurrentState();
        var spaceManRotation = _animator.gameObject.transform.localEulerAngles.y;
        if (animatorCanMove)
        {
            // Set _spaceManRotationGoal based on input
            if (horizontalInput > 0)
            {
                _spaceManRotationGoal = -90;
            }
            else if (horizontalInput < 0)
            {
                _spaceManRotationGoal = 90;
            }
        }
        else if (animatorState == SpaceManAnimator.AnimatorState.FreeFalling)
        {
            _spaceManRotationGoal = 0;
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
            AudioManager.Instance.StartSound(Sfx.Hit);
        }
    }

    #endregion

    #region Misc

    private void CleanUpUpdateInput()
    {
      
        _flagOxygenApplied = false;
        _jumpLock -= Time.deltaTime;
        if (_jumpLock < 0)
        {
            _jumpLock = 0f;
            _jumpAllowed = true;
        }

        animatorCanMove = _animator.CanMove();
        horizontalInput = animatorCanMove ? Input.GetAxis("Horizontal") : 0;
        heightInput = animatorCanMove ? (int) Input.GetAxis("Jump") : 0;
        
    }

    private void PrepareGroundMovement()
    {
        _horizontalForce = horizontalInput * UpdateMovementSpeed();
        _flagOxygenApplied = UpdateOxygenConsumption(horizontalInput);
    }

    private void OxygenCalculation()
    {
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

    #endregion


    private void Update()
    {
        CleanUpUpdateInput();
        Debug.Log("" + heightInput + "  " + _horizontalForce + "  " + animatorCanMove + "  " + IsGrounded);
        
            if (!IsGrounded)
            {
                if (Mathf.Abs(horizontalInput) >= Mathf.Epsilon)
                {
                MoveInAir();
                AnimateAirMovement();
                }
            }
            else
            {
                PrepareGroundMovement();
                if (_jumpAllowed && heightInput == 1)
                {
                    Jump();
                }

                Walk();
                AnimateWalkMovement();
            }
       
        MiscAnimationStuff();
        AlignPlayer(false);
        OxygenCalculation();
    }



    // private Vector2 RescaleToMaxVelocity(Vector2 force)
    // {
    //     var perpendicular = Vector2.Perpendicular(Physics2D.gravity.normalized);
    //     var currentSpeed = Mathf.Abs(Vector2.Dot(_rg.velocity, perpendicular));
    //     var factor = ScaleFactor(currentSpeed, vMaxAcceleratableFloor);
    //     return factor * force;
    // }

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
            // Increment for free fall landings
            var state = _animator.GetCurrentState(true);
            if (state == SpaceManAnimator.AnimatorState.FreeFalling)
            {
                scoreStatistics.numberOfHardCrashes++;
                AudioManager.Instance.StartSound(Sfx.HugeSlap);
            }

            if (verticalImpactVelocity < landMiddleT)
            {
                _animator.Animate(SpaceManAnimator.AnimationState.LandEasy);
                AudioManager.Instance.StartSound(Sfx.Hit);
            }
            else if (verticalImpactVelocity < landHardT)
            {
                _animator.Animate(SpaceManAnimator.AnimationState.LandMiddle);
                AudioManager.Instance.StartSound(Sfx.Hit);
            }
            else
            {
                _animator.Animate(SpaceManAnimator.AnimationState.LandHard);
                AudioManager.Instance.StartSound(Sfx.Hit);
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

    private void Suffocate()
    {
        // TODO trigger animations
        gameSettings.oxygenCurrent = 0;
        SceneManager.LoadScene(2);
    }
}