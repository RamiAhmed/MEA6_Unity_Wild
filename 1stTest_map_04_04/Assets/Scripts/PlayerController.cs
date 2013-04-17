/* Coded by Rami Ahmed Bock, 2013 */

/*
	Instructions:

	Place this script on the player.
	No further steps necessary, except for TopDown perspective, where the 'IsTopDownPlayer' checkbox should be checked.
*/

using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	// UseDistance controls the interaction distance, i.e. within 2 Unity units the player may interact with usable objects
	public float UseDistance = 2.0f;

	public GameObject[] SpawnableBuildingObjects;
	
	public bool bPrintTime = false;
	
	[HideInInspector]
	public bool bIsTimeCounting = false;
	
	// A reference to the player's main camera.
	[HideInInspector]
	public Camera PlayerCamera;

	// A public variable used by scripts, not for the editor, hence [HideInInspector]
	[HideInInspector]
	public UsableObject UsedObject = null;
	
	[HideInInspector]
	public List<GameObject> placedObjects = new List<GameObject>();

	private float elapsedTime = 0f;
	private float lastSpawn = 0f;

	private MouseLook mouseLookComponent;
	
	private ScenarioHandler scenarioHandler;
	
	private Vector3 startPosition = Vector3.zero;
	
	/******************************************************
	 **************** INITIALIZATION **********************
	 ******************************************************/
	
	void Start()
	{
		if (!FindCamera("Main Camera"))
		{
			FindCamera("Camera");
		}

		this.gameObject.AddComponent(typeof(SaveScreenshots));
		mouseLookComponent = this.gameObject.GetComponent<MouseLook>();
		
		scenarioHandler = GameObject.Find("ScenarioHandlerBox").GetComponent<ScenarioHandler>();
		
		startPosition = this.transform.position;
	}

	private bool FindCamera(string CameraName)
	{
		bool success = false;
		try
		{
			PlayerCamera = this.transform.FindChild(CameraName).camera;
			Debug.Log("Found the player's camera successfully");
			success = true;
		}
		catch (NullReferenceException e)
		{
			Debug.LogWarning("Could not find camera under player named: " + CameraName + "\n" + e);
		}

		return success;
	}
	
	/******************************************************
	 **************** UPDATING / LOOP *********************
	 ******************************************************/	

	// Update is called once per frame
	void Update() 
	{
		GameStateHandler.GameState currentGameState = GameStateHandler.GetCurrentGameState();
		if (currentGameState == GameStateHandler.GameState.PLAY)
		{
			SetPlayerActive(true);
			if (Time.timeScale != 1f)
				Time.timeScale = 1f;
			
			PlayStateLoop();
		}
		else
		{
			SetPlayerActive(false);	
			
			if (Screen.lockCursor)
			{
				Screen.lockCursor = false;
			}	
			
			if (Time.timeScale != 0f)
				Time.timeScale = 0f;
		} 
		
		if (currentGameState != GameStateHandler.GameState.END && currentGameState != GameStateHandler.GameState.QUESTIONNAIRE)
		{
			if (Input.GetKeyDown(KeyCode.Pause) || Input.GetKeyDown(KeyCode.P))
			{
				if (currentGameState == GameStateHandler.GameState.PLAY)
					GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.PAUSE);
				
				else if (currentGameState == GameStateHandler.GameState.PAUSE)
					GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.PLAY);
			}
			
			// If the player pressed 'Escape'/'Esc'
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (currentGameState != GameStateHandler.GameState.MAIN_MENU)
					GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.MAIN_MENU);
				
				else
					GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.PLAY);
			}
		}
	}
	
	private void PlayStateLoop()
	{
		// If no player camera has been found, do not try to update
		if (PlayerCamera == null)
		{
			Debug.LogError(this + " could not find a camera!");
			return;
		}		
		
		CheckPlayerPosition();
		
		// make sure time goes on
		TimeCounting();	
		
		// disable the mouse cursor unless holding CTRL
		DisableMouseCursorOnControl();

		// If the player pressed 'E'
		if (Input.GetKeyDown(KeyCode.E))
		{
			// If the player is currently not using an object
			if (UsedObject == null)
			{
				// sweep in front of player in search of usable objects
				SweepTestToUse();
			}
		}
			
		// if the player clicked left mouse button
		else if (Input.GetMouseButtonDown(0))
		{
			// if the player is currently using an object
			if (UsedObject != null)
			{
				// place the currently used object
				PlaceObject();
			}
		}
		
		else if (Input.GetKey(KeyCode.Period))
		{
			//Debug.Log ("Delete");
			if (UsedObject != null)
			{
				ClearObjectReferences(true);		
			}
		}
		
		else if (Input.GetKey(KeyCode.Comma))
		{
			this.transform.position = startPosition;	
		}

		else
		{
			// if we are not spawning object
			if (!HandleObjectSpawning())
			{
				// maybe we are rotating?
				HandleObjectRotation();
			}
		}		
	}
	
	/* GUI */

	void OnGUI()
	{
		GameStateHandler.GameState currentGameState = GameStateHandler.GetCurrentGameState();
		if (currentGameState == GameStateHandler.GameState.PLAY)
		{
			float width = 150f, height = 50f;
			float x = 5f, y = Screen.height - height - 5f;
	
			for (int i = 0; i < SpawnableBuildingObjects.Length; i++)
			{
				GameObject spawnObject = SpawnableBuildingObjects.ElementAt(i).gameObject;
				string buttonText = (i+1).ToString() + ": Create " + spawnObject.name;
				if (GUI.Button(new Rect(x + (width * i), y, width, height), buttonText))
				{
					SpawnBuildingObject(spawnObject.name);
				}
			}
			
			if (bPrintTime)
				GUI.Label(new Rect(5f, 5f, 100f, 50f), "Time: " + elapsedTime);
		}
		
		else if (currentGameState == GameStateHandler.GameState.PAUSE)
		{
			float width = 200f, height = 50f;
			GUI.Box(new Rect((Screen.width/2f) - (width/2f), (Screen.height/2f) - (height/2f), width, height), "GAME IS PAUSED");	
		}
		
		else if (currentGameState == GameStateHandler.GameState.MAIN_MENU)
		{
			HandleMainMenu();
		}
		
		else if (currentGameState == GameStateHandler.GameState.END)
		{
			if (Application.isEditor)
				GUI.Box(new Rect(Screen.width/2f - 100, Screen.height/2f - 25, 200, 50), "GAME OVER");
			else
				Application.Quit();
		}
	}
	
	private void HandleMainMenu()
	{
		float width = 200f, height = 400f;
		float x = (Screen.width / 2f) - (width / 2f), y = (Screen.height / 2f) - (height / 2f);		

		GUILayout.BeginArea(new Rect(x, y, width, height));
		GUILayout.BeginVertical();
			
		GUILayout.Box("Main Menu");
				
		if (!GUI.skin.box.wordWrap)
			GUI.skin.box.wordWrap = true;			
		
		if (GUILayout.Button("Resume Game"))
		{
			GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.PLAY);	
		}
			
		if (scenarioHandler.GetScenarioCount() > 0)
		{	
			if (GUILayout.Button("Next Scenario"))
			{
				RemovePlacedObjects();
				this.transform.position = startPosition;
				
				if (UsedObject != null)
					ClearObjectReferences(true);
				
				GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.QUESTIONNAIRE);
			}
		}
		else
		{	
			if (GUILayout.Button("Exit Game"))
			{
				GameStateHandler.SetCurrentGameState(GameStateHandler.GameState.QUESTIONNAIRE);
			}
		}	

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
	
	/******************************************************
	 **************** HELPER METHODS **********************
	 ******************************************************/
	
	private void CheckPlayerPosition()
	{
		float allowedDistance = 100f;
		
		if ((this.transform.position - startPosition).sqrMagnitude > (allowedDistance * allowedDistance))
		{
			this.transform.position = startPosition;	
		}
	}
	
	private void SortAfterDistance(RaycastHit[] array, Vector3 compareTo)
	{
		bool swapped;
		int i, j;
		for (i = 0; i < array.Count(); i++)
		{
			swapped = false;
			for (j = 1; j < array.Count(); j++)
			{
				float aDist = (array[j-1].point - compareTo).sqrMagnitude;
				float bDist = (array[j].point - compareTo).sqrMagnitude;
				if (aDist > bDist)
				{
					RaycastHit temp = array[j-1];	
					array[j-1] = array[j];
					array[j] = temp;
					
					swapped = true;
				}
			}
			
			if (!swapped)
				break;
		}
	}	
	
	private void SetPlayerActive(bool enable)
	{
		if (mouseLookComponent != null)
		{
			if (this.gameObject.GetComponent<MouseLook>().enabled != enable)
				this.gameObject.GetComponent<MouseLook>().enabled = enable;
	
			if (PlayerCamera.GetComponent<MouseLook>().enabled != enable)
				PlayerCamera.GetComponent<MouseLook>().enabled = enable;		
		}
	}
	
	private void DisableMouseCursorOnControl()
	{
		if (mouseLookComponent != null)
		{
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			{
				if (Screen.lockCursor)
					Screen.lockCursor = false;
				
				SetPlayerActive(false);
			}
			else
			{
				if (!Screen.lockCursor)
					Screen.lockCursor = true;
				
				SetPlayerActive(true);
			}
		}		
	}

	private void TimeCounting()
	{
		if (bIsTimeCounting)
			elapsedTime += Time.deltaTime;
	}	
	
	public float GetElapsedTime()
	{
		return elapsedTime;
	}
	
	/******************************************************
	 ******* BUILDING OBJECT METHODS FOR UPDATING *********
	 ******************************************************/

	private void HandleObjectRotation()
	{
		if (UsedObject != null)
		{
			BuildingObject bo = UsedObject.GetComponent<BuildingObject>();
			if (bo != null && bo.enabled)
			{
				Vector3 rotationVector = Vector3.zero;

				if (Input.GetKey(KeyCode.Keypad8)) // up on keypad
				{
					rotationVector += this.transform.right;
				}
				else if (Input.GetKey(KeyCode.Keypad2)) // down on keypad
				{
					rotationVector += -this.transform.right;
				}

				if (Input.GetKey(KeyCode.Keypad7)) // left on keypad
				{
					rotationVector += this.transform.forward;
				}
				else if (Input.GetKey(KeyCode.Keypad9)) // right on keypad
				{
					rotationVector += -this.transform.forward;
				}

				if (Input.GetKey(KeyCode.Keypad4))
				{
					rotationVector += this.transform.up;
				}
				else if (Input.GetKey(KeyCode.Keypad6))
				{
					rotationVector += -this.transform.up;
				}

				bo.RotationVector = rotationVector;
			}
			else
			{
				Debug.LogWarning(UsedObject + " is not a building object!");
			}
		}
	}
	
	private void PlaceObject()
	{
		if (elapsedTime - lastSpawn > 1f)
		{
			lastSpawn = elapsedTime;
			
			BuildingObject bo = UsedObject.GetComponent<BuildingObject>();
			if (bo != null)
			{
				if (bo.bCanBePlaced)
				{
					// if the player is currently using an object AND is pressing 'e'
					ClearObjectReferences();
					placedObjects.Add(bo.gameObject);
				}
				else
				{
					Debug.Log("Cannot place " + UsedObject + " at this position");
				}
			}
			else
			{
				Debug.LogWarning(UsedObject + " is not a building object!");
			}	
		}
		else
		{
			Debug.LogWarning("Cannot place again so soon");
		}
	}
	
	private void RemovePlacedObjects()
	{
		while (placedObjects.Count > 0)
		{
			GameObject obj = placedObjects[0];
			placedObjects.Remove(obj);
			Destroy(obj);
		}
	}
	
	/* USING / PICKING UP */
	
	private void SweepTestToUse()
	{
		CharacterController cCont = this.GetComponent<CharacterController>();
		if (cCont != null && cCont.enabled)
		{
			Vector3 startPos = this.transform.position + cCont.center + (Vector3.up * -cCont.height * 0.5f);
			Vector3 endPos = startPos + Vector3.up * cCont.height;
			
			RaycastHit[] downHits = Physics.RaycastAll(this.transform.position, -this.transform.up, cCont.height);
			
			Vector3 forwardVec = this.transform.forward;
			
			RaycastHit[] sweepHits = Physics.CapsuleCastAll(startPos, endPos, cCont.radius, forwardVec, UseDistance);
			Vector3 midScreen = PlayerCamera.ScreenToWorldPoint(new Vector3(Screen.width/2f, Screen.height/2f, 0f));
			SortAfterDistance(sweepHits, midScreen);
			
			bool okObject = true;
			foreach (RaycastHit sweepHit in sweepHits)
			{
				if (sweepHit.collider.GetType () != typeof(TerrainCollider))
				{
					foreach (RaycastHit downHit in downHits)
					{
						if (downHit.collider.GetType() != typeof(TerrainCollider))
						{
							if (downHit.collider.gameObject == sweepHit.collider.gameObject)
							{
								okObject = false;				
							}
						}
					}
					
					if (okObject)
					{
						GameObject hit = sweepHit.collider.gameObject;
						while (hit.transform.parent != null)
							hit = hit.transform.parent.gameObject;
	
						if (UseObject(hit))
							break;
					}
					else
					{
						Debug.LogWarning("Cannot pick up while standing on " + sweepHit.collider.gameObject);
					}
				}
			}
		}
		else
		{
			Debug.LogError(this.gameObject + " has no enabled CharacterController");
		}
	}		

	private bool UseObject(GameObject go)
	{
		bool success = false;
		if (go != null)
		{
			// Get the UsableObject.cs (C# script) component from the raycast hit
			UsableObject uo = go.GetComponent<UsableObject>();
			if (uo != null && uo.enabled)
			{
				// Call the method UsedBy from the UsableObject class, passing in a reference to player's gameObject
				uo.UsedBy(this.gameObject);
				success = true;
			}
			else
			{
				Debug.Log(go + " is not usable");
			}
		}
		else
		{
			Debug.LogError("Could not use " + go);
		}
		
		return success;
	}
	
	/* SPAWNING */
	
	private bool HandleObjectSpawning()
	{
		bool spawn = false;
		for (int i = 0; i < SpawnableBuildingObjects.Length; i++)
		{
			if (Input.GetKeyDown((KeyCode)(49 + i))) // alpha numeric keys starting at 1 = 49, 9 = 47
			{
				if (UsedObject != null && !UsedObject.bHasBeenPlaced)
				{	
					ClearObjectReferences(true);
				}
				
				if (UsedObject == null)
				{
					if (SpawnBuildingObject(SpawnableBuildingObjects.ElementAt(i).name) != null)
						spawn = true;
				}
				break;
			}
		}

		return spawn;
	}

	private GameObject SpawnBuildingObject(string Prefab)
	{
		if (UsedObject == null)
		{
			// Spawn a new instance of "Building Prefab"
			GameObject buildingObject = Instantiate(Resources.Load(Prefab)) as GameObject;
			if (buildingObject != null)
			{
				// Set that we are using the new building object
				UseObject(buildingObject);
				return buildingObject;
			}
			else
			{
				Debug.LogError("Could not instantiate " + Prefab);
			}
		}
		else
		{
			Debug.LogWarning("Only one object can be used at a time");
		}
		
		return null;
	}
	
	/* CLEARING / REMOVING */
	
	private void ClearObjectReferences(bool bDestroyUsedObject = false)
	{
		if (UsedObject != null)
		{
			// Make sure that the player does not have a 'UsedObject', and the object we previously used does not have a 'user'
			if (UsedObject.GetHasUser())
			{
				UsedObject.User = null;
			}
			
			if (bDestroyUsedObject)
				Destroy(UsedObject.gameObject);
			
			UsedObject = null;
		}
		else
		{
			Debug.LogWarning("No used object to clear references from");
		}
	}		
}