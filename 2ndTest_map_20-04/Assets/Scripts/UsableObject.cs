/* Coded by Rami Ahmed Bock, 2013 */

/*
	Instructions:

		This is the (abstract) super class script for BuildingObject.cs.
		This script has almost no functionality of its own, other than showing the text "I am being used" if used.
*/

using UnityEngine;
using System.Collections;

public abstract class UsableObject : MonoBehaviour {

	// Defines whether to show the "I am being used" debug text on the player's GUI
	public bool bEnableDebugText = false;

	// Defines whether this object has once been placed, hence it cannot just be 'deleted' anymore, but should be destroyed
	public bool bHasBeenPlaced = false;

	// Define a User variable as a game object, which will hold a reference to the current User's game object
	[HideInInspector]
	public PlayerController User = null;

	public void UsedBy(GameObject usedBy)
	{
		if (!GetHasUser())
		{
			// Set the User variable to the passed-in reference to the player's game object
			User = usedBy.GetComponent<PlayerController>();
			if (GetHasUser())
			{
				// Get the PlayerController script component of the player's game object, and set the player's usedObject to this particular UsableObject
				User.UsedObject = this;
			}
			else
			{
				Debug.LogWarning(usedBy + " does not have a PlayerController!");
			}
		}
	}

	// Draws on the player's HUD every frame
	void OnGUI()
	{
		// If this usable object's current User is not null (i.e., if it is being used)
		if (GetHasUser() && bEnableDebugText)
		{
			// Find the middle of the screen
			Vector2 midScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
			// Draw debug text to the middle of the screen
			GUI.Label(new Rect(midScreen.x-50, midScreen.y-25, 100, 50), "I am being used by " + User);
		}
	}

	public bool GetHasUser()
	{
		return User != null;
	}
}
