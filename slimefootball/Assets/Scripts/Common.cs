using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Common : MonoBehaviour
{
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

}
