using System;
using Enums;
using JetBrains.Annotations;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerScript : MonoBehaviour
{
    public GameObject player;
    public bool isGrounded = true;
    [SerializeField] private float jumpForce = 400.0f;
    [SerializeField] private float movingAcceleration = 5.0f;
    [SerializeField] private float runningAcceleration = 7.0f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private GameSettings gameSettings;

    [SerializeField] private float vMinTurn;

    // Animation values
    [SerializeField] private float walkT = 0.2f;
    [SerializeField] private float runT = 3f;
    [SerializeField] private float fallT = 1f;
    [SerializeField] private float freeFallT = 20f;

    [SerializeField] private float landMiddleT = 2f;

    [SerializeField] private float landHardT = 10f;

    // public GravityManager gravity;
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
    private bool _flagOxygenApplied;

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

        var horizontalInput = Input.GetAxis("Horizontal");
        var heightInput = (int) Input.GetAxis("Jump");
        if (!isGrounded)
        {
            // verticalSpeed += fallingAcceleration * Time.deltaTime;
            _horizontalForce = horizontalInput * 5f * movingAcceleration;

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
                if (Mathf.Abs(_rg.velocity.x) > 5 && currentVelocity.x * _horizontalForce > 0) _horizontalForce = 0;
            }
            else
            {
                if (Mathf.Abs(_rg.velocity.y) > 5 && currentVelocity.y * _horizontalForce > 0) _horizontalForce = 0;
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
                _animator.Float();
            }
            else if (absVerticalVelocityAligned < freeFallT)
            {
                _animator.Fall();
            }
            else
            {
                _animator.FreeFall();
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

                _animator.Jump();
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
            if (absHorizontalVelocityAligned < walkT)
            {
                _animator.Stand();
            }
            else if (absHorizontalVelocityAligned < runT)
            {
                _animator.Walk();
            }
            else
            {
                _animator.Run();
            }
        }


        AlignPlayer(false);

        isGrounded = false;
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        var verticalImpactVelocity = Mathf.Abs(Vector2.Dot(Physics2D.gravity.normalized, other.relativeVelocity));

        if (verticalImpactVelocity < landMiddleT)
        {
            _animator.LandEasy();
        }
        else if (verticalImpactVelocity < landHardT)
        {
            _animator.LandMiddle();
        }
        else
        {
            _animator.LandHard();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hole")) Ejection();
        if (other.CompareTag("Collidable")) isGrounded = true;
        if (other.CompareTag("OxygenTank")) ONOxygenTank(other);
    }

    private void ONOxygenTank( Component other)
    {
        gameSettings.oxygenCurrent += gameSettings.oxygenTank;
        gameSettings.oxygenCurrent = (gameSettings.oxygenCurrent > gameSettings.oxygenMax)
            ? gameSettings.oxygenMax
            : gameSettings.oxygenCurrent;
        GameObject.Destroy(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Collidable")) isGrounded = true;
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

    //TODO (obviously) (Wenn Tobi einen Sprintbutton parallel zu shift einabut)
    private static bool CheckRunButtonUI()
    {
        return
            false;
    }

    //Todo. gäme över o2 aus
    private void Suffocate()
    {
        throw new NotImplementedException();
    }
}