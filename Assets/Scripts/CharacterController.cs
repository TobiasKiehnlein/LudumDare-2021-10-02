using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    Rigidbody2D rg;
    public GameObject player;
    private float movingSpeed = 5.0f;
    private float JumpForce = 400.0f;
    private float fallingAcceleration = -4.90665f;
    private float jumplock = 0;
    private bool jumpAllowed = false;
    Vector2 vel = new Vector2(0, 0);
    public bool isGrounded = true;
    float horizontalSpeed;
    // Start is called before the first frame update
    void Start()
    {
        rg = GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame
    void Update()
    {
        rg.gravityScale = 1;
        isGrounded = (abs(rg.velocity.y) < 0.05f);
        jumplock -= Time.deltaTime;
        if (jumplock < 0) {jumplock = 0f;
            jumpAllowed = true;
        }
        float horizontalInput = Input.GetAxis("Horizontal");
        int heightInput = (int) Input.GetAxis("Jump");
        if (!isGrounded)
        {
           // verticalSpeed += fallingAcceleration * Time.deltaTime;
            horizontalSpeed = horizontalInput * 0.33f * movingSpeed;
            vel = new Vector2(horizontalSpeed, rg.velocity.y);
        }
        else
        {
       
           
                horizontalSpeed = horizontalInput * movingSpeed;

               
                if (jumpAllowed && heightInput == 1 )
                {
                    rg.AddForce(new Vector2(0, (JumpForce * rg.mass)));
                    jumplock = 0.1f;
                    jumpAllowed = false;
                }
                vel = new Vector2(horizontalSpeed, rg.velocity.y);
        }
        rg.velocity = vel;
    }

    private float abs(float input)
    {
        return ((input < 0f) ? -input : input);
    }
}
