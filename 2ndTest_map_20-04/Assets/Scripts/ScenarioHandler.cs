using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ScenarioHandler : MonoBehaviour {
	
	public bool bShowCurrentScenario = true;
	
	public enum Scenario
	{
		MOUSE,
		KEYBOARD,
		MIXED
	};
	
	private List<Scenario> availableScenarios;
	private Scenario currentScenario;
	
	void Start() 
	{
		availableScenarios = new List<Scenario>();
		InitScenario();		
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
		availableScenarios.Add(Scenario.MIXED);
		
		RandomizeScenarios();
	}
	
	public void GetNewRandomScenario()
	{
		if (availableScenarios.Count > 1)
		{
			int randomIndex = UnityEngine.Random.Range(0, availableScenarios.Count);
			
			//Debug.Log("New random scenario: " + randomIndex);
			
			currentScenario = availableScenarios[randomIndex];
			availableScenarios.RemoveAt(randomIndex);
		}
		else if (availableScenarios.Count == 1)
		{
			//Debug.Log("Only 1 new random scenario left");
			currentScenario = availableScenarios[0];
			availableScenarios.RemoveAt(0);
		}
		else
		{
			Debug.LogWarning("No new scenarios found!");
		}
	}
	
	private void RandomizeScenarios()
	{
		for (int i = 0; i < availableScenarios.Count; i++)
		{
			if (UnityEngine.Random.Range(0,2) == 0)
			{
				Scenario temp = availableScenarios[i];
				int randomIndex = UnityEngine.Random.Range(i, availableScenarios.Count);
				availableScenarios[i] = availableScenarios[randomIndex];
				availableScenarios[randomIndex] = temp;
			}
		}
	}
}
