using System;
using Enums;
using ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterController : MonoBehaviour
{
    public GameObject player;
    public bool isGrounded = true;
    [SerializeField] private float jumpForce = 400.0f;
    [SerializeField] private float movingAcceleration = 5.0f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private GameSettings gameSettings;

    [SerializeField] private float vMinTurn;
    // public GravityManager gravity;


    private float _horizontalForce;

    private bool _jumpAllowed;
    private float _jumpLock;
    private Rigidbody2D _rg;
    private Vector2 _vel = new Vector2(0, 0);

    // Start is called before the first frame update
    private void Start()
    {
        _rg = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
       
      
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

            horizontalMovementDirection *= _horizontalForce;
            _rg.AddForce(horizontalMovementDirection);
        }
        else
        {
            _horizontalForce = horizontalInput * movingAcceleration;


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
        }


        AlignPlayer(false);

        isGrounded = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Hole")) Ejection();
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
        if(fallbackModeUsed)
        {
            transform.rotation = gameSettings.GravityOrientation switch
            {
                Orientation.Down => Quaternion.Euler(0, 0, 0),
                Orientation.Up => Quaternion.Euler(0, 0, 180f),
                Orientation.Left => Quaternion.Euler(0, 0, 270f),
                Orientation.Right => Quaternion.Euler(0, 0, 90f),
                _ => transform.rotation
            };
        }else
        {
            float targetAngle = gameSettings.GravityOrientation switch
            {
                Orientation.Up => 180,
                Orientation.Down => 0,
                Orientation.Left => 90,
                Orientation.Right => 270,
                _ => 0
            };
            if(Vector2.Dot(Physics2D.gravity.normalized,_rg.velocity) > vMinTurn)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle),
                    turnSpeed * Time.deltaTime);
            }
        }
    }
}