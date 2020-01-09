using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MenuData
{
    private static Common.GameMode gameMode = Common.GameMode.SinglePlayer1v1;
    public static Common.GameMode GameMode
    {
        get { return gameMode; }
        set { gameMode = value; }
    }

    private static Common.AiImplementations[] teamAiImplementations = new Common.AiImplementations[] { Common.AiImplementations.Default, Common.AiImplementations.Aaron };
    public static Common.AiImplementations[] TeamAiImplementations
    {
        get { return teamAiImplementations;  }
        set { teamAiImplementations = value; }
    }
}
