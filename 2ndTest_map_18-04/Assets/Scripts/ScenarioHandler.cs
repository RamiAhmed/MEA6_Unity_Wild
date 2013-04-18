using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ScenarioHandler : MonoBehaviour {
	
	public bool bShowCurrentScenario = true;
	
	public enum Scenario
	{
		MOUSE,
		KEYBOARD
	};
	
	private List<Scenario> availableScenarios;
	private Scenario currentScenario;
	
	void Start() 
	{
		availableScenarios = new List<Scenario>();
		InitScenario();		
		//GetNewRandomScenario();
	}
	
	void OnGUI()
	{
		if (bShowCurrentScenario && GameStateHandler.GetCurrentGameState() != GameStateHandler.GameState.QUESTIONNAIRE)
		{
			float width = 150f, height = 50f;
			GUI.Box(new Rect(Screen.width - (width + 10f), 5f, width, height), "Scenario: " + currentScenario.ToString());
		}
	}
	
	public Scenario GetCurrentScenario()
	{
		return currentScenario;
	}
	
	public int GetScenarioCount()
	{
		return availableScenarios.Count;
	}
	
	private void InitScenario()
	{
		availableScenarios.Add(Scenario.MOUSE);
		availableScenarios.Add(Scenario.KEYBOARD);
	}
	
	public void GetNewRandomScenario()
	{
		if (availableScenarios.Count > 1)
		{
			int randomIndex = UnityEngine.Random.Range(0, availableScenarios.Count);
			
			currentScenario = availableScenarios[randomIndex];
			availableScenarios.RemoveAt(randomIndex);
		}
		else if (availableScenarios.Count == 1)
		{
			currentScenario = availableScenarios[0];
			availableScenarios.RemoveAt(0);
		}
		else
		{
			Debug.LogWarning("No new scenarios found!");
		}
	}
}
