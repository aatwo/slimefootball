using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MenuData
{
    private static Common.GameMode gameMode;
    public static Common.GameMode GameMode
    {
        get { return gameMode; }
        set { gameMode = value; }
    }
}
