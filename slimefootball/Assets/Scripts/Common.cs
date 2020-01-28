using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Common : MonoBehaviour
{
    public enum AiImplementations
    {
        Default,
        Aaron,
        Rich,

        Random
    }

    public enum Ability
    {
        normal,
        ball_reverse,
        turbo_running,

        count
    }

    public static string ToString(Ability a)
    {
        switch(a)
        {
            case Ability.normal: return "normal";
            case Ability.ball_reverse: return "ball reverse";
            case Ability.turbo_running: return "turbo running";
            default: return "";
        }
    }

    public static string ToString(AiImplementations type)
    {
        switch(type)
        {
            case AiImplementations.Default: return "Default AI";
            case AiImplementations.Aaron: return "Aarons AI";
            case AiImplementations.Rich: return "Richs AI";
            case AiImplementations.Random: return "Random";
            default: return "Missing name in Common.cs";
        }
    }

    public enum GameMode
    {
        AiOnly1v1,
        SinglePlayer1v1,
        SinglePlayer1v2,
        TwoPlayer1v1,
        AiOnly2v2,
        SinglePlayer2v2,
        TwoPlayer2v2,
        TwoPlayerCoop2v2,
        TwoPlayerCoop2v10
    };

    public enum Direction
    {
        left,
        right
    };

    enum GameState
    {
        Playing,
        Resetting,
        Finished
    };

    static public string ballTag = "Ball";
    static public string environmentLayerName = "Environment";
    static public string playersSortingLayerName = "Players";

}
