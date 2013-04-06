using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScenarioHandler : MonoBehaviour {
	
	public bool bShowCurrentScenario;
	
	public List<GameObject> playerControllers;
	
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
		//DontDestroyOnLoad(this.gameObject);
		
		InitScenario();
		GetNewRandomScenario(false);		
	}
	
	void OnGUI()
	{
		if (bShowCurrentScenario)
			GUI.Box(new Rect(Screen.width - 250f, 10f, 200f, 50f), "Scenario: " + currentScenario.ToString());
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
		
		/*
		foreach (GameObject pc in playerControllers)
		{
			if (pc.activeSelf)
				pc.SetActive(false);
		}	
		*/	
	}
	
	public void GetNewRandomScenario(bool bOpenGoogle = true)
	{
		if (currentPlayer != null)
		{
			Destroy(currentPlayer);
			currentPlayer = null;
		}
		
		if (availableScenarios.Count > 1)
		{
			int randomIndex = Random.Range(0, availableScenarios.Count);
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
			if (bOpenGoogle)
				Application.OpenURL("www.google.com");
			else
				Application.OpenURL("https://docs.google.com/forms/d/1GpFp6wvW-BsHYD4WQvjkUoT7GWK3n81woKketR_2vgY/viewform");
		}
		else
			Debug.LogError("Could not find player controller in scenario " + currentScenario.ToString());
	}
}
