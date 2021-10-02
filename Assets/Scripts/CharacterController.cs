using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    Rigidbody2D _rg;
    public GameObject player;
    private float movingSpeed = 5.0f;
    private float JumpForce = 400.0f;
    private float _jumpLock = 0;
    private bool _jumpAllowed = false;
    Vector2 _vel = new Vector2(0, 0);
    public bool isGrounded = true;
    float _horizontalSpeed;
    // Start is called before the first frame update
    void Start()
    {
        _rg = GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame
    void Update()
    {
        _rg.gravityScale = 1;

        switch (GravityManager.GeTInstance().GETCurrentOrientation())
        {
            case Orientation.Up :
            case Orientation.Down :
                isGrounded = (Abs(_rg.velocity.y) < 0.05f);
                break;
            case Orientation.Left:
                case Orientation.Right:
                isGrounded = (Abs(_rg.velocity.x) < 0.05f);
                break;
            default: break;
        }
        _jumpLock -= Time.deltaTime;
        if (_jumpLock < 0) {_jumpLock = 0f;
            _jumpAllowed = true;
        }
        float horizontalInput = Input.GetAxis("Horizontal");
        int heightInput = (int) Input.GetAxis("Jump");
        if (!isGrounded)
        {
           // verticalSpeed += fallingAcceleration * Time.deltaTime;
            _horizontalSpeed = horizontalInput * 0.33f * movingSpeed;
            
            
            switch (GravityManager.GeTInstance().GETCurrentOrientation())
            {
                case Orientation.Up :
                    _vel = new Vector2(-_horizontalSpeed, _rg.velocity.y);
                    break;
                case Orientation.Down :
                    _vel = new Vector2(_horizontalSpeed, _rg.velocity.y);
                    break;
                case Orientation.Left:
                    _vel = new Vector2(_rg.velocity.x, _horizontalSpeed);
                    break;
                case Orientation.Right:
                    _vel = new Vector2(_rg.velocity.x, -_horizontalSpeed);
                    break;
                
                default: break;
            }
            
            
            
            
            
            
            
            
        }
        else
        {
       
           
                _horizontalSpeed = horizontalInput * movingSpeed;

               
                if (_jumpAllowed && heightInput == 1 )
                {
                    
                    switch (GravityManager.GeTInstance().GETCurrentOrientation())
                    {
                        case Orientation.Up :
                            _rg.AddForce(new Vector2(0, -(JumpForce * _rg.mass)));
                            break;
                        case Orientation.Down :
                            _rg.AddForce(new Vector2(0, (JumpForce * _rg.mass)));
                            break;
                        case Orientation.Left:
                            _rg.AddForce(new Vector2((JumpForce * _rg.mass), 0));
                            break;
                        case Orientation.Right:
                            _rg.AddForce(new Vector2(-(JumpForce * _rg.mass), 0));
                            break;
                
                        default: break;
                    }
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                    
                   
                    _jumpLock = 0.1f;
                    _jumpAllowed = false;
                }
                switch (GravityManager.GeTInstance().GETCurrentOrientation())
                {
                    case Orientation.Up :
                        _vel = new Vector2(-_horizontalSpeed, _rg.velocity.y);
                        break;
                    case Orientation.Down :
                        _vel = new Vector2(_horizontalSpeed, _rg.velocity.y);
                        break;
                    case Orientation.Left:
                        _vel = new Vector2(_rg.velocity.x, _horizontalSpeed);
                        break;
                    case Orientation.Right:
                        _vel = new Vector2(_rg.velocity.x, -_horizontalSpeed);
                        break;
                
                    default: break;
                }
                
        }
        _rg.velocity = _vel;
    }

    private static float Abs(float input)
    {
        return ((input < 0f) ? -input : input);
    }
}
