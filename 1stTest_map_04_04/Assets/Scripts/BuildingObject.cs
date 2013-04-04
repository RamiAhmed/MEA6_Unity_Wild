/* Coded by Rami Ahmed Bock, 2013 */

/*
	Instructions:

	Place this script on any object which should be used as a building object, i.e. picked up and placed again.
	No further steps needed.
*/

using UnityEngine;
using System;
using System.Collections;

public class BuildingObject : UsableObject
{
	// This is the distance in Unity units at which the used object is held (distance from player pawn)
	public float HoldingDistance = 3f;

	// The speed with which this building object rotates (degrees per frame)
	public float RotationSpeed = 2.5f;
	
	// Set this to true if building object is supposed to be rotated randomly at start
	public bool bRotateRandomOnStart = false;

	[HideInInspector]
	public Vector3 RotationVector = Vector3.zero;

	[HideInInspector]
	public bool bCanBePlaced = false;

	private int collisions = 0;
	private Color[] originalColors;
	private bool bCanUseColors = false;
	
	private float timeSincePlaced = 0f;
	private float sleepTime = 30f;
	
	private Collider[] colliders;
	private Rigidbody[] rigidbodies;

	void Start()
	{
		try
		{
			Material[] mats = this.renderer.materials;
			if (mats.Length > 0)
			{
				originalColors = new Color[mats.Length];
				bCanUseColors = true;
				for (int i = 0; i < mats.Length; i++)
				{
					originalColors.SetValue(mats[i].color, i);
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogWarning(e);
		}
		
		if (bRotateRandomOnStart)
			this.transform.rotation = UnityEngine.Random.rotation;
		
		InitColliders();
	}

	// FixedUpdate is called once every physics time step. This is the place to do physics-based game behaviour.
	void FixedUpdate()
	{
		// If the usable object currently has a user
		if (GetHasUser() && User.UsedObject == this)
		{
			// Make sure that the player's UsedObject is this particular object
			if (User.UsedObject == this)
			{
				UpdatePosition();

				UpdateRotation();
				
				ToggleCollidersActive(true);

				UpdateMaterialColors();
				
				timeSincePlaced = 0f;
			}
		}
		// If the usable object currently does not have a user
		else
		{
			if (!bHasBeenPlaced)
				bHasBeenPlaced = true;
	
			ChangeAllMaterialColors(originalColors);
			
			if (timeSincePlaced < sleepTime)
			{
				timeSincePlaced += Time.deltaTime;
					
				ToggleCollidersActive(false);
			}
			else
			{
				if (!this.rigidbody.isKinematic)
					this.rigidbody.isKinematic = true;
			}
		}
	}

	private void UpdatePosition()
	{
		Vector3 targetPos = Vector3.zero;
		if (!User.IsTopDownPlayer)
		{
			// Calculate a 2D vector at the middle of the screen
			Vector2 midScreen = new Vector2(Screen.width/2, Screen.height/2);

			// Find the distance from the user to his camera, in order to make the vector longer in non-1st person perspective
			float distanceMultiplier = (User.transform.position - User.PlayerCamera.transform.position).magnitude;

			// Define a 3D vector at the middle of the screen, using the HoldingDistance variable for depth (distance from player pawn)
			targetPos = new Vector3(midScreen.x, midScreen.y, HoldingDistance + distanceMultiplier);
			targetPos = User.PlayerCamera.ScreenToWorldPoint(targetPos);

			float pawnToObjectDistance = (User.transform.position - targetPos).magnitude;
			if (pawnToObjectDistance < HoldingDistance)
			{
				targetPos = User.transform.InverseTransformPoint(targetPos);
				targetPos.z += HoldingDistance;
				targetPos = User.transform.TransformPoint(targetPos);
			}
		}
		else
		{
			targetPos = User.transform.position + User.transform.forward * (HoldingDistance * 1.5f);
		}

		// Minimum height is calculated by getting the height of the terrain and adding half of the object's own height
		float minimumHeight = Terrain.activeTerrain.SampleHeight(targetPos) + this.renderer.bounds.extents.y;
		if (targetPos.y < minimumHeight)
		{
			targetPos.y = minimumHeight;
		}

		// Set this building object's transform at the specified vector above, after converting the vector to real in-game 3D positions
		this.transform.position = targetPos;
	}

	private void UpdateRotation()
	{
		// If the rotation vector has been set by player controller
		if (RotationVector != Vector3.zero)
		{
			//this.transform.Rotate(RotationVector, RotationSpeed);
			this.transform.RotateAround(this.transform.position, RotationVector, RotationSpeed);
		}
	}
	
	private void InitColliders()
	{
		colliders = this.gameObject.GetComponents<Collider>();
		if (colliders.Length <= 0)
		{
			colliders = this.gameObject.GetComponentsInChildren<Collider>();
			if (colliders.Length <= 0)
				Debug.LogWarning("Could not find any colliders for " + this.gameObject);
		}
		
		rigidbodies = this.gameObject.GetComponents<Rigidbody>();
		if (rigidbodies.Length <= 0)
		{
			rigidbodies = this.gameObject.GetComponentsInChildren<Rigidbody>();
			if (rigidbodies.Length <= 0)
				Debug.LogWarning("Could not find any rigidbodies for " + this.gameObject);
		}		
	}
	
	private void ToggleCollidersActive(bool enable)
	{
		foreach (Collider coll in colliders)
		{
			if (coll.isTrigger != enable)
				coll.isTrigger = enable;
		}
		
		foreach (Rigidbody rBody in rigidbodies)
		{
			if (rBody.isKinematic != enable)
				rBody.isKinematic = enable;
		}
	}

	private void UpdateMaterialColors()
	{
		if (IsColliding())
		{
			//Debug.Log ("Colliding");
			ChangeAllMaterialColors(Color.red);

			if (bCanBePlaced)
				bCanBePlaced = false;
		}
		else
		{
			ChangeAllMaterialColors(Color.green);

			if (!bCanBePlaced)
				bCanBePlaced = true;
		}
	}
	
	private void ChangeAllMaterialColors(Color color)
	{
		if (bCanUseColors)
		{
			for (int i = 0; i < this.renderer.materials.Length; i++)
			{
				if (this.renderer.materials[i].color != color)
				{
					this.renderer.materials[i].color = color;	
				}
			}
		}
	}
	
	private void ChangeAllMaterialColors(Color[] colors)
	{
		if (bCanUseColors)
		{
			for (int i = 0; i < this.renderer.materials.Length; i++)
			{
				if (this.renderer.materials[i].color != colors[i])
				{
					this.renderer.materials[i].color = colors[i];	
				}
			}		
		}
	}	

	void OnTriggerEnter(Collider other)
	{
		if (other.collider.GetType() != typeof(TerrainCollider))
		{
			//Debug.Log ("Triggering with " + other);
			collisions++;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.collider.GetType() != typeof(TerrainCollider))
		{
			//Debug.Log ("No longer triggering with " + other);
			collisions--;
		}
	}

	private bool IsColliding()
	{
		//Debug.Log ("Collision count: " + collisions);
		return collisions > 0;
	}
}

