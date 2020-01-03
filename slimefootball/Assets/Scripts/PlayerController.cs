using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float jumpForceDurationS = 0.3f;
    public float jumpForcePerSecond = 1000f;

    Rigidbody2D rb;

    public enum JumpState
    {
        can_jump,
        jumping,
        falling
    };

    JumpState jumpState = JumpState.can_jump;
    bool hasReleasedJumpSinceLastJump = true;
    float jumpStartTime = 0f;
    bool manualInputEnabled = false;

    public void SetManualInputEnabled(bool enabled)
    {
        manualInputEnabled = enabled;
    }

    public void MoveLeft()
    {
        ProcessHorizontalAxisInput( -1f );
    }

    public void MoveRight()
    {
        ProcessHorizontalAxisInput( 1f );
    }

    public void MoveHorizontal( float value )
    {
        float clampedValue = Mathf.Clamp(value, -1f, 1f);
        ProcessHorizontalAxisInput( clampedValue );
    }

    public void Jump()
    {
        ProcessVerticalAxisInput(1f);
    }

    public void MoveVertical(float value)
    {
        float clampedValue = Mathf.Clamp(value, -1f, 1f);
        ProcessVerticalAxisInput( clampedValue );
    }

    public JumpState GetJumpState()
    {
        return jumpState;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if( rb == null )
            Debug.LogError( "no rigid body in player" );

    }

    // Update is called once per frame
    void Update()
    {        
    }

    void ProcessHorizontalAxisInput( float horizontalInput )
    {
        float xVel = horizontalInput * maxSpeed;
        rb.velocity = new Vector2( xVel, rb.velocity.y );
    }

    void ProcessVerticalAxisInput( float verticalInput )
    {
        // Jump rules:
        //      1. if player is on floor they can press jump
        //      2. while holding jump jump force will be applied
        //      3. after releasing jump jump force stops
        //      4. player can't jump again until landed
        //      5. jump force only applied for a limited time
        //      6. after a successful jump the user cannot jump until they release and press it again

        bool pressingJump = (verticalInput > 0.0f);
        if( !pressingJump )
            hasReleasedJumpSinceLastJump = true;

        switch(jumpState)
        {
            case JumpState.can_jump:
            {
                if( pressingJump && (hasReleasedJumpSinceLastJump || !manualInputEnabled) )
                {
                    hasReleasedJumpSinceLastJump = false;
                    jumpStartTime = Time.time;
                    jumpState = JumpState.jumping;
                    rb.velocity = new Vector2( rb.velocity.x, 6f );
                }
                break;
            }

            case JumpState.jumping:
            {
                // Check if player let go of jump
                if( !pressingJump )
                {
                    jumpState = JumpState.falling;
                }
                else
                {
                    // Check how long we've been pressing jump for
                    float jumpPressDuration = Time.time - jumpStartTime;
                    if( jumpPressDuration >= jumpForceDurationS )
                    {
                        jumpState = JumpState.falling;
                    }
                    else
                    {
                        // Continue applying jump force
                        float forceToApply = (Time.deltaTime * jumpForcePerSecond);
                        rb.AddForce( new Vector2( 0f, forceToApply ) );
                    }
                }
                break;
            }

            case JumpState.falling:
            {
                // When falling we apply extra downwards force on top of gravity to make
                // it feel more satisfying. I.E floaty upwards jump followed by a fast
                // satisfying fall.
                float forceToApply = -0.5f * (Time.deltaTime * jumpForcePerSecond);
                rb.AddForce( new Vector2( 0f, forceToApply ) );
                break;
            }
        }
    }

    private void OnTriggerEnter2D( Collider2D collision )
    {
        // Ignore any collisions with non-environment objects
        if( collision.gameObject.layer != LayerMask.NameToLayer( "Environment" ) )
            return;

        jumpState = JumpState.can_jump;
    }

    private void OnTriggerStay2D( Collider2D collision )
    {
        // Ignore any collisions with non-environment objects
        if( collision.gameObject.layer != LayerMask.NameToLayer( "Environment" ) )
            return;

        if( jumpState == JumpState.falling )
        {
            jumpState = JumpState.can_jump;
        }
    }
}
