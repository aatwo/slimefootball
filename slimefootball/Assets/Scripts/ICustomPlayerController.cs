using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICustomPlayerController
{
    string GetDisplayTag();
    void SetPlayerController( PlayerController playerController );
    void SetTeamIndex( int index );
    void HandleRoundStarted( Transform ball, List<Vector3> teamPositions, List<Vector3> opposingTeamPositions, Transform[] goals, int[] scores, int winningScore );
    void HandleRoundFinished();
}
