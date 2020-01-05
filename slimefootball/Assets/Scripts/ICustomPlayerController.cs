using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICustomPlayerController
{
    void SetPlayerController( PlayerController playerController );
    void SetTeamIndex( int index );
    void StartRound( Transform ball, Transform[] goals, int[] scores, int winningScore );
    void EndRound();
}
