using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ScenarioHandler : MonoBehaviour {
	
	public bool bShowCurrentScenario = true;
	
	public List<GameObject> playerControllers;
	
	private string QuestionnaireURL = "https://docs.google.com/forms/d/1GpFp6wvW-BsHYD4WQvjkUoT7GWK3n81woKketR_2vgY/viewform";
	private string HelperPicURL = "https://docs.google.com/file/d/0B1xZRCO0P8gZUkdPVXVtMEg2TlU/edit?usp=sharing";
	
	public enum Scenario
	{
		FIRST_PERSON = 0, // first person
		THIRD_PERSON = 1, // third person
		TOP_DOWN = 2  // top-down
	};
	
	private List<Scenario> availableScenarios = new List<Scenario>();
	private Scenario currentScenario;
	
	private GameObject currentPlayer = null;

	// Use this for initialization
	void Start() 
	{
		InitScenario();
		GetNewRandomScenario(false);		
	}
	
	void OnGUI()
	{
		if (bShowCurrentScenario)
		{
			string currentScenarioText = "";
			
			switch (currentScenario)
			{
				case Scenario.FIRST_PERSON: currentScenarioText = "First Person"; break;
				case Scenario.THIRD_PERSON: currentScenarioText = "Third Person"; break;
				case Scenario.TOP_DOWN: currentScenarioText = "Top-Down"; break;
			}
			
			currentScenarioText += " Perspective";
			
			GUI.Box(new Rect(Screen.width - 250f, 10f, 200f, 25f), currentScenarioText);
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
		availableScenarios.Add(Scenario.FIRST_PERSON);
		availableScenarios.Add(Scenario.THIRD_PERSON);
		availableScenarios.Add(Scenario.TOP_DOWN);	
	}
	
	public void GetNewRandomScenario(bool bOpenHelper = true, bool bForceOpenBrowser = false)
	{
		if (currentPlayer != null)
		{
			Destroy(currentPlayer);
			currentPlayer = null;
		}
		
		if (availableScenarios.Count > 1)
		{
			int randomIndex = UnityEngine.Random.Range(0, availableScenarios.Count);
			currentScenario = availableScenarios[randomIndex]; 
			currentPlayer = playerControllers[randomIndex];
		}
		else
		{
			currentScenario = availableScenarios[0];
			currentPlayer = playerControllers[0];
		}
		
		availableScenarios.Remove(currentScenario);			
		
		if (currentPlayer != null)
		{
			currentPlayer.SetActive(true);
			playerControllers.Remove(currentPlayer);
			
			if (!Application.isEditor || bForceOpenBrowser)
			{			
				if (bOpenHelper)
					Application.OpenURL(HelperPicURL);
				else
					Application.OpenURL(QuestionnaireURL);
			}
		}
		else
			Debug.LogError("Could not find player controller in scenario " + currentScenario.ToString());
	}
}
