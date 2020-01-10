using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] Dropdown teamOneAiEdit;
    [SerializeField] Dropdown teamTwoAiEdit;

    private void Start()
    {
        InitUi();
    }

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

    private void InitUi()
    {
        List<string> aiNames = new List<string>();
        for( int i = 0; i <= (int)Common.AiImplementations.Random; i++ )
        {
            Common.AiImplementations type = (Common.AiImplementations)i;
            aiNames.Add( Common.ToString( type ) );
        }
        teamOneAiEdit.AddOptions( aiNames );
        teamTwoAiEdit.AddOptions( aiNames );

        Common.AiImplementations[] teamAiImplementations = MenuData.TeamAiImplementations;
        if( teamAiImplementations == null || teamAiImplementations.Length < 2 )
            Debug.LogError( "teamAiImplementations array in MenuData not the expected length" );

        teamOneAiEdit.value = (int)MenuData.TeamAiImplementations[0];
        teamTwoAiEdit.value = (int)MenuData.TeamAiImplementations[1];
    }

    private void StartGame()
    {
        Common.AiImplementations selectedTeamOneAi = (Common.AiImplementations)teamOneAiEdit.value;
        Common.AiImplementations selectedTeamTwoAi = (Common.AiImplementations)teamTwoAiEdit.value;

        Common.AiImplementations[] teamAiImplementations = new Common.AiImplementations[2];
        teamAiImplementations[0] = selectedTeamOneAi;
        teamAiImplementations[1] = selectedTeamTwoAi;
        MenuData.TeamAiImplementations = teamAiImplementations;

        SceneManager.LoadScene( "game", LoadSceneMode.Single );
    }
}
