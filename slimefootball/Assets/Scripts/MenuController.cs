using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void OnAiOnly1v1ButtonPressed()
    {
        MenuData.GameMode = Common.GameMode.AiOnly1v1;
        StartGame();
    }

    public void OnSinglePlayer1v1ButtonPressed()
    {
        MenuData.GameMode = Common.GameMode.SinglePlayer1v1;
        StartGame();
    }

    public void OnTwoPlayer1v1ButtonPressed()
    {
        MenuData.GameMode = Common.GameMode.TwoPlayer1v1;
        StartGame();
    }

    public void OnAiOnly2v2ButtonPressed()
    {
        MenuData.GameMode = Common.GameMode.AiOnly2v2;
        StartGame();
    }

    public void OnSinglePlayer2v2ButtonPressed()
    {
        MenuData.GameMode = Common.GameMode.SinglePlayer2v2;
        StartGame();
    }

    public void OnTwoPlayer2v2ButtonPressed()
    {
        MenuData.GameMode = Common.GameMode.TwoPlayer2v2;
        StartGame();
    }

    public void OnTwoPlayerCoop2v2ButtonPressed()
    {
        MenuData.GameMode = Common.GameMode.TwoPlayerCoop2v2;
        StartGame();
    }

    public void OnTwoPlayerCoop2v10ButtonPressed()
    {
        MenuData.GameMode = Common.GameMode.TwoPlayerCoop2v10;
        StartGame();
    }

    private void StartGame()
    {
        // TODO: UI component to select team AI implementations
        Common.AiImplementations[] teamAiImplementations = new Common.AiImplementations[2];
        teamAiImplementations[0] = Common.AiImplementations.Aaron;
        teamAiImplementations[1] = Common.AiImplementations.Rich;
        MenuData.TeamAiImplementations = teamAiImplementations;

        SceneManager.LoadScene( "game", LoadSceneMode.Single );
    }
}
