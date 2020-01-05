using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualPlayerController : MonoBehaviour
{
    public PlayerController playerController;

    Transform ball;
    float searchIntervalS = 1f;
    float lastPlayerSearchTime = 0f;
    int playerControllerIndex = -1;

    bool hasReleasedJumpSinceLastJump = true;

    public void SetPlayerControllerIndex( int n )
    {
        playerControllerIndex = n;
    }

    // Update is called once per frame
    void Update()
    {
        SearchForPlayerController();

        if( playerControllerIndex != -1 )
        {
            ProcessKeyboardInput();
        }
    }

    void SearchForPlayerController()
    {
        if( playerController != null )
            return;

        float timeSinceLastSearchS = Time.time - lastPlayerSearchTime;
        bool isFirstTimeSearch = (lastPlayerSearchTime == 0f);
        if( timeSinceLastSearchS < searchIntervalS && !isFirstTimeSearch )
            return;

        lastPlayerSearchTime = Time.time;
        playerController = GetComponent<PlayerController>();
    }

    void ProcessKeyboardInput()
    {
        if( playerController == null )
            return;

        string horizontalAxisName = "Horizontal" + playerControllerIndex + "_key";
        string verticalAxisName = "Vertical" + playerControllerIndex + "_key";

        float horizontalInput = Input.GetAxis( horizontalAxisName );
        playerController.MoveHorizontal( horizontalInput );

        float verticalInput = Input.GetAxisRaw( verticalAxisName );
        bool pressingJump = (verticalInput > 0.0f);

        if( !pressingJump )
        {
            hasReleasedJumpSinceLastJump = true;
            playerController.MoveVertical( 0f );
        }

        // We only want to apply jump force while the user presses the jump button for the first jump cycle
        else if( pressingJump && hasReleasedJumpSinceLastJump )
        {
            hasReleasedJumpSinceLastJump = false;
            playerController.MoveVertical( verticalInput );
        }

        // Once the jump is started we want to apply jump force only while the jump state is jumping
        else if( pressingJump && !hasReleasedJumpSinceLastJump && playerController.GetJumpState() == PlayerController.JumpState.jumping)
        {
            playerController.MoveVertical( verticalInput );
        }

        else
        {
            playerController.MoveVertical( 0f );
        }
    }
}
