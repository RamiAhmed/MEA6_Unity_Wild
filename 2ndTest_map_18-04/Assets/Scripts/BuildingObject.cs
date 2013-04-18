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
	
	public float SecondsToKinematic = 5f;

	[HideInInspector]
	public Vector3 RotationVector = Vector3.zero;

	[HideInInspector]
	public bool bCanBePlaced = false;

	private int collisions = 0;
	private Color[] originalColors;
	private bool bCanUseColors = false;
	
	private float timeSincePlaced = 0f;
	
	private Collider[] colliders;
	private Rigidbody[] rigidbodies;
	
	private bool bIsGrounded = false;
	
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
		if (GetHasUser())
		{
			// Make sure that the player's UsedObject is this particular object
			if (User.UsedObject == this)
			{
				UpdatePosition();

				UpdateRotation();
				
				SetCollidersInactive(true);

				UpdateMaterialColors();
				
				timeSincePlaced = 0f;
				bIsGrounded = false;
			}
			else
			{
				Debug.LogWarning(this.gameObject + " has unknown user : " + this.User);
			}
		}
		// If the usable object currently does not have a user
		else
		{
			if (!bHasBeenPlaced)
				bHasBeenPlaced = true;
	
			ChangeAllMaterialColors(originalColors);
			

			if (timeSincePlaced < SecondsToKinematic)
			{
				if (bIsGrounded)
				{
					timeSincePlaced += Time.deltaTime;
				}		
				
				SetCollidersInactive(false);
			}
			else
			{
				foreach (Rigidbody rBody in rigidbodies)
				{
					if (!rBody.isKinematic)
						rBody.isKinematic = true;
				}
			}
		}
	}

	private void UpdatePosition()
	{
		Vector3 targetPos = Vector3.zero;
		float desiredDistance = HoldingDistance;
		float playerHeight = User.transform.position.y * 0.5f;
		
		Vector2 midScreen = new Vector2(Screen.width/2, Screen.height/2);

		targetPos = new Vector3(midScreen.x, midScreen.y, desiredDistance);
		targetPos = User.PlayerCamera.ScreenToWorldPoint(targetPos);

		float minimumHeight = Terrain.activeTerrain.SampleHeight(targetPos) + this.renderer.bounds.extents.y;
		float maximumHeight = minimumHeight + playerHeight;	
		targetPos.y = Mathf.Clamp(targetPos.y, minimumHeight, maximumHeight);
		
		targetPos = User.transform.InverseTransformPoint(targetPos);
		targetPos.z = Mathf.Clamp(targetPos.z, -desiredDistance, desiredDistance);
		targetPos.x = Mathf.Clamp(targetPos.x, -desiredDistance, desiredDistance);
		targetPos = User.transform.TransformPoint(targetPos);
		
		this.transform.position = targetPos;
	}

	private void UpdateRotation()
	{
		// If the rotation vector has been set by player controller
		if (RotationVector != Vector3.zero)
		{
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
	
	private void SetCollidersInactive(bool enable)
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
		if (other.collider.GetType() != typeof(TerrainCollider) && other.gameObject.name != "BuildZoneVolume" && !other.gameObject.CompareTag("CompletionBox"))
		{
			collisions++;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.collider.GetType() != typeof(TerrainCollider) && other.gameObject.name != "BuildZoneVolume" && !other.gameObject.CompareTag("CompletionBox"))
		{
			collisions--;
		}
	}

	private bool IsColliding()
	{
		return collisions > 0;
	}
	
	void OnCollisionEnter(Collision collInfo)
	{
		if (collInfo.collider.GetType() == typeof(TerrainCollider))
		{
			if (!bIsGrounded)
				bIsGrounded = true;
		}
	}
}

