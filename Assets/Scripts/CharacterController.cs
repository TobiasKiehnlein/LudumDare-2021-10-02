using System;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public GameObject player;
    public bool isGrounded = true;
    private readonly float _jumpForce = 400.0f;
    private readonly float _movingSpeed = 5.0f;
    private readonly float _turnSpeed = 5f;

    // public GravityManager gravity;
    private Orientation _currentOrientation;

    private float _horizontalSpeed;

    private bool _isUnAligned = true;
    private bool _jumpAllowed;
    private float _jumpLock;
    private Orientation _oldOrientation;
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
        _rg.gravityScale = 1;
        // _currentOrientation = gravity.GETCurrentOrientation();
        _currentOrientation = Orientation.Down;
        var grav = _currentOrientation switch
        {
            Orientation.Up => -Vector2.right,
            Orientation.Down => -Vector2.left,
            Orientation.Left => Vector2.up,
            Orientation.Right => Vector2.down,
            _ => Vector2.zero
        };
        grav *= 5;

        Physics2D.gravity = grav;
        this.gameObject.transform.rotation = _currentOrientation switch
        {
            Orientation.Down => Quaternion.Euler(0, 0, 0),
            Orientation.Up => Quaternion.Euler(0, 0, 180f),
            Orientation.Left => Quaternion.Euler(0, 0, 90f),
            Orientation.Right => Quaternion.Euler(0, 0, 270f),
            _ => this.gameObject.transform.rotation
        };
/*   switch (_currentOrientation)
 {
      case Orientation.Up:
      case Orientation.Down:
          isGrounded = (Abs(_rg.velocity.y) < 0.05f);
          break;
      case Orientation.Left:
      case Orientation.Right:
          isGrounded = (Abs(_rg.velocity.x) < 0.05f);
          break;
      default: break;
  }*/

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
            _horizontalSpeed = horizontalInput * 5f * _movingSpeed;

            var force = _currentOrientation switch
            {
                Orientation.Up => -Vector2.up,
                Orientation.Down => -Vector2.down,
                Orientation.Left => Vector2.left,
                Orientation.Right => Vector2.right,
                _ => Vector2.zero
            };

            if (_currentOrientation == Orientation.Down || _currentOrientation == Orientation.Up)
            {
                if (Mathf.Abs(_rg.velocity.x) > 5) _horizontalSpeed = 0;
            }
            else
            {
                if (Mathf.Abs(_rg.velocity.y) > 5) _horizontalSpeed = 0;
            }

            force *= _horizontalSpeed;
            _rg.AddForce(force);
        }
        else
        {
            _horizontalSpeed = horizontalInput * _movingSpeed;


            if (_jumpAllowed && heightInput == 1)
            {
                switch (_currentOrientation)
                {
                    case Orientation.Up:
                        _rg.AddForce(new Vector2(0, -(_jumpForce * _rg.mass)));
                        break;
                    case Orientation.Down:
                        _rg.AddForce(new Vector2(0, _jumpForce * _rg.mass));
                        break;
                    case Orientation.Left:
                        _rg.AddForce(new Vector2(_jumpForce * _rg.mass, 0));
                        break;
                    case Orientation.Right:
                        _rg.AddForce(new Vector2(-(_jumpForce * _rg.mass), 0));
                        break;
                }


                _jumpLock = 0.1f;
                _jumpAllowed = false;
            }

            switch (_currentOrientation)
            {
                case Orientation.Up:
                    _vel = new Vector2(-_horizontalSpeed, _rg.velocity.y);
                    break;
                case Orientation.Down:
                    _vel = new Vector2(_horizontalSpeed, _rg.velocity.y);
                    break;
                case Orientation.Left:
                    _vel = new Vector2(_rg.velocity.x, _horizontalSpeed);
                    break;
                case Orientation.Right:
                    _vel = new Vector2(_rg.velocity.x, -_horizontalSpeed);
                    break;
            }

            _rg.velocity = _vel;
        }


      //  if (_isUnAligned) AlignPlayer();

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

    public void UpdatePlayerGravity(Orientation newOrientation)
    {
        _isUnAligned = true;
        _oldOrientation = _currentOrientation;
        _currentOrientation = newOrientation;
    }

    private void AlignPlayer()
    {
        float targetAngle = 0;
        switch (_currentOrientation)
        {
            case Orientation.Up:
                targetAngle = 180;
                break;
            case Orientation.Down:
                targetAngle = 0;
                break;
            case Orientation.Left:
                targetAngle = 90;
                break;
            case Orientation.Right:
                targetAngle = 270;
                break;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, targetAngle),
            _turnSpeed * Time.deltaTime);
    }
}