using UnityEngine;
using System.Collections;

public class GameStateHandler  {

	public enum GameState
	{
		MAIN_MENU,
		QUESTIONNAIRE,
		PLAY,
		PAUSE,
		END
	};
	
	private static GameState currentGameState = GameState.MAIN_MENU;
	
	public static GameState GetCurrentGameState()
	{
		return currentGameState;	
	}
	
	public static void SetCurrentGameState(GameState newState)
	{
		currentGameState = newState;
	}
}
