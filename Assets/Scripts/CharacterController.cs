using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public GameObject player;
    public bool isGrounded = true;
    private readonly float _jumpForce = 400.0f;
    private readonly float _turnSpeed = 5f;


    private Orientation _currentOrientation;

    private float _horizontalSpeed;

    private bool _isUnAligned = true;
    private bool _jumpAllowed;
    private float _jumpLock;
    private readonly float _movingSpeed = 5.0f;
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
        _currentOrientation = GravityManager.GeTInstance().GETCurrentOrientation();
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
            var force = Vector2.zero;

            switch (_currentOrientation)
            {
                case Orientation.Up:
                    force = -Vector2.right;
                    break;
                case Orientation.Down:
                    force = -Vector2.left;
                    break;
                case Orientation.Left:
                    force = Vector2.up;
                    break;
                case Orientation.Right:
                    force = Vector2.down;
                    break;
            }

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


        if (_isUnAligned) AlignPlayer();

        isGrounded = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Collidable")) isGrounded = true;
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