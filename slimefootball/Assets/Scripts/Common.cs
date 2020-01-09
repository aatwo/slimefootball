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

    public enum GameMode
    {
        AiOnly1v1,
        SinglePlayer1v1,
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
