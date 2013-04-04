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

	// Defines whether this player controller is top-down or not
	public bool IsTopDownPlayer = false;

	public GameObject[] SpawnableBuildingObjects;

	// A reference to the player's main camera.
	[HideInInspector]
	public Camera PlayerCamera;

	// A public variable used by scripts, not for the editor, hence [HideInInspector]
	[HideInInspector]
	public UsableObject UsedObject = null;

	private Vector3 startPosition = Vector3.zero;
	private bool isTimeCounting = false;
	private float elapsedTime = 0f;
	private float lastSpawn = 0f;

	private MouseLook mouseLookComponent;

	
	private enum GameState
	{
		MAIN_MENU,
		PLAY,
		PAUSE,
		END
	};
	
	private GameState currentGameState = GameState.MAIN_MENU;
	
	/******************************************************
	 **************** INITIALIZATION **********************
	 ******************************************************/
	
	void Start()
	{
		if (!FindCamera("Main Camera"))
		{
			FindCamera("Camera");
		}

		startPosition = this.transform.position;
		
		this.gameObject.AddComponent(typeof(SaveScreenshots));
		mouseLookComponent = this.gameObject.GetComponent<MouseLook>();
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
	void Update() {
		if (currentGameState == GameState.PLAY)
		{
			SetPlayerActive(true);
			if (Time.timeScale != 1f)
				Time.timeScale = 1f;
			
			PlayStateLoop();
		}
		else
		{
			SetPlayerActive(false);	
			
			if (Time.timeScale != 0f)
				Time.timeScale = 0f;
		} 
		
		if (currentGameState == GameState.END)
		{
			//Application.Quit();
		}
		else
		{
			if (Input.GetKeyDown(KeyCode.Pause) || Input.GetKeyDown(KeyCode.P))
			{
				Debug.Log("Pause");
				
				if (currentGameState == GameState.PLAY)
					currentGameState = GameState.PAUSE;
				
				else if (currentGameState == GameState.PAUSE)
					currentGameState = GameState.PLAY;
			}
			
			// If the player pressed 'Escape'/'Esc'
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (this.gameObject.GetComponent<SaveScreenshots>() != null)
					this.gameObject.GetComponent<SaveScreenshots>().TakeScreenshot();
				
				if (currentGameState != GameState.MAIN_MENU)
					currentGameState = GameState.MAIN_MENU;
				
				else
					currentGameState = GameState.END;
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
		
		// make sure time goes on
		TimeCounting();	
		
		// disable the mouse cursor unless holding CTRL
		DisableMouseCursorOnControl();

		// If the player pressed 'E' or left mouse click
		if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
		{
			// If the player is currently not using an object
			if (UsedObject == null)
			{
				// sweep in front of player in search of usable objects
				SweepTestToUse();
			}
			else
			{
				// place the currently used object
				PlaceObject();
			}
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
		if (currentGameState == GameState.PLAY)
		{
			float width = 150f, height = 50f;
			float x = 5f, y = Screen.height - height - 5f;
	
			for (int i = 0; i < SpawnableBuildingObjects.Length; i++)
			{
				GameObject spawnObject = SpawnableBuildingObjects.ElementAt(i).gameObject;
				bool newBtn = GUI.Button(new Rect(x + (width * i), y, width, height), (i+1).ToString() + ": Create " + spawnObject.name);
				if (newBtn)
				{
					SpawnBuildingObject(spawnObject.name);
				}
			}
		}
		
		else if (currentGameState == GameState.PAUSE)
		{
			float width = 200f, height = 100f;
			GUI.Label(new Rect(Screen.width/2f - (width/2f), Screen.height/2f - (height/2f), width, height), "GAME IS PAUSED");	
		}
		
		else if (currentGameState == GameState.MAIN_MENU)
		{
			float width = 200f, height = 400f;
			float x = Screen.width / 2f - (width / 2f), y = Screen.height/2f - (height/2f);
			GUI.BeginGroup(new Rect(x, y, width, height));
			
			GUI.Box(new Rect(0f, 10f, width, 40f), "Main Menu");
			
			if (GUI.Button(new Rect(0f, 60f, width, 50f), "Start/Resume Game"))
			{
				currentGameState = GameState.PLAY;
			}
			
			if (GUI.Button(new Rect(0f, 115f, width, 50f), "Exit Game"))
			{
				currentGameState = GameState.END;
			}
			
			GUI.EndGroup();
		}
		
		else if (currentGameState == GameState.END)
		{
			float width = 200f, height = 400f;
			float x = Screen.width / 2f - (width / 2f), y = Screen.height/2f - (height/2f);
			GUI.BeginGroup(new Rect(x, y, width, height));
			
			GUI.Box(new Rect(0f, 10f, width, 40f), "Are you sure you want to exit?");
			
			if (GUI.Button(new Rect(0f, 50f, width/2f, 50f), "Yes"))
			{
				Debug.Log("Shutting down");
				Application.Quit();
			}
			
			if (GUI.Button(new Rect(width/2f, 50f, width/2f, 50f), "No"))
			{
				currentGameState = GameState.MAIN_MENU;
			}
			
			GUI.EndGroup();
		}
	}
	
	/******************************************************
	 **************** HELPER METHODS FOR UPDATING *********
	 ******************************************************/
	
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
				{
					Screen.lockCursor = false;
				}
				
				SetPlayerActive(false);
			}
			else
			{
				if (!Screen.lockCursor)
				{
					Screen.lockCursor = true;
				}
				
				SetPlayerActive(true);
			}
		}		
	}
	
	private void TimeCounting()
	{
		if (!isTimeCounting)
		{
			if (startPosition.x != this.transform.position.x &&
				startPosition.z != this.transform.position.z)
			{
				isTimeCounting = true;
			}
		}
		else
		{
			elapsedTime += Time.deltaTime;
		}
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
		BuildingObject bo = UsedObject.GetComponent<BuildingObject>();
		if (bo != null)
		{
			if (bo.bCanBePlaced)
			{
				// if the player is currently using an object AND is pressing 'e'
				ClearObjectReferences();
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
	
	/* USING / PICKING UP */
	
	private void SweepTestToUse()
	{
		CharacterController cCont = this.GetComponent<CharacterController>();
		if (cCont != null && cCont.enabled)
		{
			Vector3 startPos = this.transform.position + cCont.center + (Vector3.up * -cCont.height * 0.5f);
			Vector3 endPos = startPos + Vector3.up * cCont.height;
			
			RaycastHit[] downHits = Physics.RaycastAll(this.transform.position, -this.transform.up, cCont.height);			
			RaycastHit[] sweepHits = Physics.CapsuleCastAll(startPos, endPos, cCont.radius, this.transform.forward, UseDistance);
			
			bool okObject = true;
			foreach (RaycastHit sweepHit in sweepHits)
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
					SpawnBuildingObject(SpawnableBuildingObjects.ElementAt(i).name);
					spawn = true;
				}
				break;
			}
		}

		return spawn;
	}

	private void SpawnBuildingObject(string Prefab = "BuildingPrefab")
	{
		if (UsedObject == null)
		{
			if (elapsedTime - lastSpawn > 0.5f)
			{
				lastSpawn = elapsedTime;

				// Spawn a new instance of "Building Prefab"
				GameObject buildingObject = Instantiate(Resources.Load(Prefab)) as GameObject;
				if (buildingObject != null)
				{
					// Set that we are using the new building object
					UseObject(buildingObject);
				}
				else
				{
					Debug.LogError("Could not instantiate " + Prefab);
				}
			}
			else
			{
				Debug.LogWarning("Cannot spawn again so soon");
			}
		}
		else
		{
			Debug.LogWarning("Only one object can be used at a time");
		}
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
	
	/******************************************************
	 **************** EVENTS ******************************
	 ******************************************************/

	void OnApplicationQuit()
	{
		if (elapsedTime > 0f)
		{
			string logFilePath = @Application.dataPath + @"/Data/",
				   logFileName = @"time_log.txt";

			if (!Directory.Exists(logFilePath))
			{
				Directory.CreateDirectory(logFilePath);
			}

			string fullPath = logFilePath + logFileName;
			if (!File.Exists(fullPath))
			{
				File.Create(fullPath).Close();
			}

			try
			{
				TextWriter tw = new StreamWriter(fullPath, true);
				tw.WriteLine("Total time: " + elapsedTime + ", " + this.gameObject.name);
				tw.Close();
				tw.Dispose();
			}
			catch (IOException e)
			{
				Debug.LogWarning(e);
			}
		}
	}
}