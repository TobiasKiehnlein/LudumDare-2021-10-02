using System;
using Enums;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerScript : MonoBehaviour
{
    public bool IsGrounded => _groundCollisions > 0;
    [SerializeField] private float jumpForce = 400.0f;
    [SerializeField] private float movingAcceleration = 5.0f;
    [SerializeField] private float runningAcceleration = 7.0f;
    [SerializeField] private float airForceMultiplier = 5f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float maxAirBoostSpeed = 5f;
    [SerializeField] private GameSettings gameSettings;

    [SerializeField] private float vMinTurn;

    // Animation values
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

    private float _horizontalForce;

    private bool _jumpAllowed;
    private float _jumpLock;
    private Rigidbody2D _rg;
    private Vector2 _vel = new Vector2(0, 0);

    private SpaceManAnimator _animator;
    [SerializeField] private float spaceManRotationSpeed = 10f;
    private float _spaceManRotationGoal = 0f;

    private bool _flagOxygenApplied;

    private int _groundCollisions = 0;

    // Start is called before the first frame update
    private void Start()
    {
        _rg = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<SpaceManAnimator>();
    }

    // Update is called once per frame
    private void Update()
    {
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
        if (!IsGrounded)
        {
            _horizontalForce = horizontalInput * airForceMultiplier * movingAcceleration;

            var horizontalMovementDirection = gameSettings.GravityOrientation switch
            {
                Orientation.Up => Vector2.left,
                Orientation.Down => Vector2.right,
                Orientation.Left => Vector2.down,
                Orientation.Right => Vector2.up,
                _ => Vector2.zero
            };

            var currentVelocity = _rg.velocity;

            if (gameSettings.GravityOrientation == Orientation.Down ||
                gameSettings.GravityOrientation == Orientation.Up)
            {
                if (Mathf.Abs(_rg.velocity.x) > maxAirBoostSpeed && currentVelocity.x * _horizontalForce > 0)
                    _horizontalForce = 0;
            }
            else
            {
                if (Mathf.Abs(_rg.velocity.y) > maxAirBoostSpeed && currentVelocity.y * _horizontalForce > 0)
                    _horizontalForce = 0;
            }

            if (_horizontalForce > Mathf.Epsilon)
            {
                gameSettings.oxygenCurrent -= oxygenConsumptionMoving * Time.deltaTime;
                _flagOxygenApplied = true;
            }

            horizontalMovementDirection *= _horizontalForce;
            _rg.AddForce(horizontalMovementDirection);

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
        else
        {
            _horizontalForce = horizontalInput * UpdateMovementSpeed();
            _flagOxygenApplied = UpdateOxygenConsumption(horizontalInput);


            if (_jumpAllowed && heightInput == 1)
            {
                switch (gameSettings.GravityOrientation)
                {
                    case Orientation.Up:
                        _rg.AddForce(new Vector2(0, -(jumpForce * _rg.mass)));
                        break;
                    case Orientation.Down:
                        _rg.AddForce(new Vector2(0, jumpForce * _rg.mass));
                        break;
                    case Orientation.Left:
                        _rg.AddForce(new Vector2(jumpForce * _rg.mass, 0));
                        break;
                    case Orientation.Right:
                        _rg.AddForce(new Vector2(-(jumpForce * _rg.mass), 0));
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
                Debug.Log(absHorizontalVelocityAligned);
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

        AlignPlayer(false);

        SpaceManAnimator.AnimatorState animatorState = _animator.GetCurrentState();
        float spaceManRotation = _animator.gameObject.transform.localEulerAngles.y;
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
            _animator.gameObject.transform.localRotation = Quaternion.Lerp(_animator.gameObject.transform.localRotation,
                Quaternion.Euler(0f, _spaceManRotationGoal, 0f), Time.deltaTime * spaceManRotationSpeed);
        }

        // Fix grounded animation when not correctly triggered
        if (IsGrounded && (animatorState == SpaceManAnimator.AnimatorState.Falling ||
                           animatorState == SpaceManAnimator.AnimatorState.Floating ||
                           animatorState == SpaceManAnimator.AnimatorState.FreeFalling))
        {
            _animator.Animate(SpaceManAnimator.AnimationState.LandEasy);
        }

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

    private void OnCollisionEnter2D(Collision2D other)
    {
        float verticalImpactVelocity = Mathf.Abs(Vector2.Dot(Physics2D.gravity.normalized, other.relativeVelocity));

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
    
    private void ONOxygenTank( Component other)
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
            if (Vector2.Dot(Physics2D.gravity.normalized, _rg.velocity) > vMinTurn)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle),
                    turnSpeed * Time.deltaTime);
            }
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