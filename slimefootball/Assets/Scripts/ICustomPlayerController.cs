using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICustomPlayerController
{
    string GetDisplayTag();
    void SetPlayerController( PlayerController playerController );
    void SetTeamIndex( int index );
    void HandleRoundStarted( Transform ball, List<Transform> teamTransforms, List<Transform> opposingTeamTransforms, Transform[] goals, int[] scores, int winningScore );
    void HandleRoundFinished();
}
