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
	
	private Collider[] colliders;
	private Rigidbody[] rigidbodies;
	
	private bool bIsGrounded = false;
	
	private float elapsedSeconds = 0f;
	private float lastUsedTime = 0f;
	private float lastPlacedTime = 0f;
	private float lastGroundedTime = 0f;
	
	private enum BuildingObjectState 
	{
		PLACED,
		PLACED_SOLID,
		ROTATING,
		MOVING
	};
	
	private BuildingObjectState currentBOState = BuildingObjectState.MOVING;	
	
	void Start()
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
		
		if (bRotateRandomOnStart)
			this.transform.rotation = UnityEngine.Random.rotation;
		
		InitColliders();
		
		if (GetHasUser())
			currentBOState = BuildingObjectState.MOVING;
	}

	// FixedUpdate is called once every physics time step. This is the place to do physics-based game behaviour.
	void FixedUpdate()
	{
		elapsedSeconds += Time.deltaTime;
		
		switch (currentBOState)
		{
			case BuildingObjectState.MOVING:
			{
				if (GetHasUser() && User.UsedObject == this)
				{
					UpdatePosition();
					if (elapsedSeconds - lastUsedTime > 0.1f)
					{
						SetCollidersInactive(true);
						UpdateMaterialColors();
					}
				
					if (bIsGrounded)
						bIsGrounded = false;
				
					// If the rotation vector has been set by player controller
					if (RotationVector != Vector3.zero)
						currentBOState = BuildingObjectState.ROTATING;
				}	
				else 
				{
					lastPlacedTime = elapsedSeconds;
					currentBOState = BuildingObjectState.PLACED;			
				}
			}break;
			
			case BuildingObjectState.ROTATING:
			{
				if (GetHasUser() && User.UsedObject == this)
				{
					UpdateRotation();
					SetCollidersInactive(true);
					UpdateMaterialColors();
				
					if (RotationVector == Vector3.zero || Vector3.Distance(this.transform.position, User.transform.position) > (HoldingDistance*1.5f))
						currentBOState = BuildingObjectState.MOVING;
				}
				else
				{
					lastPlacedTime = elapsedSeconds;
					currentBOState = BuildingObjectState.PLACED;
				}
			}break;
			
			case BuildingObjectState.PLACED:
			{	
				if (!GetHasUser())
				{
					SetCollidersInactive(false);
					UpdateMaterialColors();
				
					if (elapsedSeconds - lastPlacedTime > 1f)
					{
						if (!bHasBeenPlaced)
							bHasBeenPlaced = true;
					}
				
					if (bIsGrounded)
					{
						if (elapsedSeconds - lastGroundedTime > SecondsToKinematic)
						{
							foreach (Rigidbody rBody in rigidbodies)
							{
								if (!rBody.isKinematic)
									rBody.isKinematic = true;
							}	
						
							currentBOState = BuildingObjectState.PLACED_SOLID;
						}
					}
				}
				else
				{
					if (User.UsedObject == this)
					{
						lastUsedTime = elapsedSeconds;
						currentBOState = BuildingObjectState.MOVING;
					}
					else
					{
						Debug.LogWarning("GetHasUser() but UsedObject != this");	
					}
				}
			}break;
			
			case BuildingObjectState.PLACED_SOLID:
			{
				if (GetHasUser())
				{
					if (User.UsedObject == this)
					{
						lastUsedTime = elapsedSeconds;
						currentBOState = BuildingObjectState.MOVING;
					}
					else
					{
						Debug.LogWarning("GetHasUser() but UsedObject != this");	
					}
				}
				
			}break;
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
		
		targetPos = User.transform.InverseTransformPoint(targetPos);
		targetPos.z = Mathf.Clamp(targetPos.z, -desiredDistance, desiredDistance);
		targetPos.x = Mathf.Clamp(targetPos.x, -desiredDistance, desiredDistance);
		targetPos = User.transform.TransformPoint(targetPos);
		
		float minimumHeight = Terrain.activeTerrain.SampleHeight(targetPos) + this.renderer.bounds.extents.y;
		float maximumHeight = minimumHeight + playerHeight;	
		targetPos.y = Mathf.Clamp(targetPos.y, minimumHeight, maximumHeight);		
		
		this.transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z);
	}

	private void UpdateRotation()
	{
		this.transform.RotateAround(this.transform.position, RotationVector, RotationSpeed);
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
			
			if (enable)
			{
				if (!rBody.IsSleeping())
					rBody.Sleep();
			}
			else
			{
				if (rBody.IsSleeping())
					rBody.WakeUp();
			}
		}
	}

	private void UpdateMaterialColors()
	{
		if (currentBOState == BuildingObjectState.PLACED)
		{
			ChangeAllMaterialColors(originalColors);
		}
		else
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
		if (currentBOState == BuildingObjectState.PLACED)
		{
			if (collInfo.collider.GetType() == typeof(TerrainCollider))
			{
				if (!bIsGrounded)
				{
					bIsGrounded = true;
					lastGroundedTime = elapsedSeconds;
				}
			}
		}
	}
}

