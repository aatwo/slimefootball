using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICustomPlayerController
{
    string GetDisplayName();
    void SetPlayerController( PlayerController playerController );
    void SetTeamIndex( int index );
    void HandleRoundStarted( Transform ball, Transform[] goals, int[] scores, int winningScore );
    void HandleRoundFinished();
}
