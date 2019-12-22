﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 5f;

    Rigidbody2D rb;
    int playerIndex = 0;

    enum JumpState
    {
        can_jump,
        jumping,
        falling
    };

    JumpState jumpState = JumpState.can_jump;
    bool hasReleasedJumpSinceLastJump = true;

    float jumpForceDurationS = 0.3f;
    float jumpStartTime = 0f;
    float jumpForcePerSecond = 1000f;

    public void SetPlayerIndex(int n)
    {
        playerIndex = n;
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
        // TODO: allow support for players 3 and 4
        if( playerIndex == 0 || playerIndex == 1 )
        {
            ProcessKeyboardInput();

            // If no keyboard movement then try gamepad
            //if( inputVector.x == 0f && inputVector.y == 0f)
                //ProcessGamepadInput();
        }
    }

    void ProcessKeyboardInput()
    {
        string horizontalAxisName = "Horizontal" + playerIndex + "_key";
        string verticalAxisName = "Vertical" + playerIndex + "_key";

        ProcessHorizontalAxisInput( horizontalAxisName );
        ProcessVerticalAxisInput( verticalAxisName );
    }

    void ProcessHorizontalAxisInput( string horizontalName )
    {
        float horizontalInput = Input.GetAxis( horizontalName );
        float xVel = horizontalInput * maxSpeed;

        rb.velocity = new Vector2( xVel, rb.velocity.y );
    }

    void ProcessVerticalAxisInput( string verticalName )
    {
        if( playerIndex != 0 )
            return;

        float verticalInput = Input.GetAxisRaw(verticalName );

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
                if( pressingJump && hasReleasedJumpSinceLastJump )
                {
                    hasReleasedJumpSinceLastJump = false;
                    jumpStartTime = Time.time;
                    jumpState = JumpState.jumping;
                    rb.AddForce( new Vector2( 0f, 150f ) );
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
        jumpState = JumpState.can_jump;
    }

    private void OnTriggerStay2D( Collider2D collision )
    {
        if( jumpState == JumpState.falling )
        {
            jumpState = JumpState.can_jump;
        }
    }
}
