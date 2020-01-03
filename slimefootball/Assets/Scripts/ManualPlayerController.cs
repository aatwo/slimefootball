using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualPlayerController : MonoBehaviour
{
    public PlayerController playerController;

    Transform ball;
    float searchIntervalS = 1f;
    float lastBallSearchTime = 0f;
    float lastPlayerSearchTime = 0f;

    int playerControllerIndex = -1;

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
        if( timeSinceLastSearchS < searchIntervalS )
            return;

        lastPlayerSearchTime = Time.time;
        playerController = GetComponent<PlayerController>();
        if( playerController != null )
            playerController.SetManualInputEnabled( false );
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
        playerController.MoveVertical( verticalInput );
    }
}
