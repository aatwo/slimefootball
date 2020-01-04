using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float jumpForceDurationS = 0.3f;
    public float jumpForcePerSecond = 1000f;

    Rigidbody2D rb;

    [SerializeField]
    Sprite[] playerSprites;

    [SerializeField] GameObject LeftTriggerDetector;
    [SerializeField] GameObject RightTriggerDetector;
    [SerializeField] GameObject BottomTriggerDetector;

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
    bool canMoveLeft = true;
    bool canMoveRight = true;

    public void SetManualInputEnabled(bool enabled)
    {
        manualInputEnabled = enabled;
    }

    public void SetPlayerSpriteIndex(int index)
    {
        if( index < 0 || index > playerSprites.Length )
            Debug.LogError("invalid player sprite index in player controller");

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if( spriteRenderer == null )
            Debug.LogError("unable to find sprite renderer in player");

        spriteRenderer.sprite = playerSprites[index];
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

        if( playerSprites == null )
            Debug.LogError( "no character sprites set" );

        if( LeftTriggerDetector == null )
            Debug.LogError("no left detector set in player controller");

        if( RightTriggerDetector == null )
            Debug.LogError( "no right detector set in player controller" );

        if( BottomTriggerDetector == null )
            Debug.LogError( "no bottom detector set in player controller" );

        // Create trigger detectors and attach them to the specified game objects

        { // Detect bottom collisions
            TriggerDetector script = BottomTriggerDetector.AddComponent<TriggerDetector>();
            script.OnTriggerEnterEvent += HandleBottomTriggerEnter;
            script.OnTriggerStayEvent += HandleBottomTriggerStay;
        }

        { // Detect left collisions
            TriggerDetector script = LeftTriggerDetector.AddComponent<TriggerDetector>();
            script.OnTriggerEnterEvent += HandleLeftTriggerEnter;
            script.OnTriggerStayEvent += HandleLeftTriggerEnter;
            script.OnTriggerExitEvent += HandleLeftTriggerExit;
        }

        { // Detect right collisions
            TriggerDetector script = RightTriggerDetector.AddComponent<TriggerDetector>();
            script.OnTriggerEnterEvent += HandleRightTriggerEnter;
            script.OnTriggerStayEvent += HandleRightTriggerEnter;
            script.OnTriggerExitEvent += HandleRightTriggerExit;
        }
    }

    void ProcessHorizontalAxisInput( float horizontalInput )
    {
        float xVel = horizontalInput * maxSpeed;

        if( !canMoveLeft && xVel < 0f )
            xVel = 0f;

        if( !canMoveRight && xVel > 0f )
            xVel = 0f;

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

    void HandleBottomTriggerEnter( Collider2D collision )
    {
        // Ignore any collisions with non-environment objects
        if( collision.gameObject.layer != LayerMask.NameToLayer( Common.environmentLayerName ) )
            return;

        jumpState = JumpState.can_jump;
    }

    void HandleBottomTriggerStay( Collider2D collision )
    {
        // Ignore any collisions with non-environment objects
        if( collision.gameObject.layer != LayerMask.NameToLayer( Common.environmentLayerName ) )
            return;

        if( jumpState == JumpState.falling )
        {
            jumpState = JumpState.can_jump;
        }
    }

    void HandleLeftTriggerEnter( Collider2D collision )
    {
        // Ignore any collisions with non-environment objects
        if( collision.gameObject.layer != LayerMask.NameToLayer( Common.environmentLayerName ) )
            return;

        canMoveLeft = false;
    }

    void HandleLeftTriggerExit( Collider2D collision )
    {
        // Ignore any collisions with non-environment objects
        if( collision.gameObject.layer != LayerMask.NameToLayer( Common.environmentLayerName ) )
            return;

        canMoveLeft = true;
    }

    void HandleRightTriggerEnter( Collider2D collision )
    {
        // Ignore any collisions with non-environment objects
        if( collision.gameObject.layer != LayerMask.NameToLayer( Common.environmentLayerName ) )
            return;

        canMoveRight = false;
    }

    void HandleRightTriggerExit( Collider2D collision )
    {
        // Ignore any collisions with non-environment objects
        if( collision.gameObject.layer != LayerMask.NameToLayer( Common.environmentLayerName ) )
            return;

        canMoveRight = true;
    }
    
}
