using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 5f;

    Rigidbody2D rb;
    int playerIndex = 0;

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
        //string verticalAxisName = "Vertical" + playerIndex + "_key"; // TODO: JUMP
        ProcessHorizontalAxisInput( horizontalAxisName );
    }

    void ProcessHorizontalAxisInput( string horizontalName )
    {
        float horizontalInput = Input.GetAxis( horizontalName );
        float xVel = horizontalInput * maxSpeed;

        rb.velocity = new Vector2( xVel, rb.velocity.y );
    }
}
