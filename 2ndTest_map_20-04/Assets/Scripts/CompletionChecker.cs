using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CompletionChecker : MonoBehaviour {
	
	public GameObject CheckThisObject = null;
	public int RequiredCollisions = 1;
	
	private bool bComplete = false;
	
	[HideInInspector]
	public GameObject collidingWith = null;
	
	[HideInInspector]
	public List<GameObject> collidingLeaves = new List<GameObject>();
	
	private GameObject GetTopParent(Collider other)
	{
		return other.transform.root.gameObject;
	}
	
	public bool CheckCompletion()
	{
		bool collides = true;
		Collider[] colliders = {};
		if (RequiredCollisions == 1)
		{	
			if (collidingWith != null)
			{
				colliders = collidingWith.GetComponents<Collider>();
				if (colliders.Length <= 0)
					colliders = collidingWith.GetComponentsInChildren<Collider>();
				
				foreach (Collider coll in colliders)
				{
					if (!coll.bounds.Intersects(this.gameObject.collider.bounds))
					{
						collides = false;
						break;
					}
				}
			}
			else
			{
				collides = false;
			}
		}
		else
		{
			if (collidingLeaves.Count > 0)
			{
				int noCollisionCount = 0;
				foreach (GameObject go in collidingLeaves)
				{
					if (go != null)
					{
						colliders = go.GetComponents<Collider>();
						if (colliders.Length <= 0)
							colliders = go.GetComponentsInChildren<Collider>();		
					
						foreach (Collider coll in colliders)
						{
							if (!coll.bounds.Intersects(this.gameObject.collider.bounds))
							{
								noCollisionCount++;
								break;
							}
						}
					}
				}
				
				if (collidingLeaves.Count - noCollisionCount < RequiredCollisions)
				{
					collides = false;
				}
			}
			else
			{
				collides = false;
			}
		}
		
		return collides;
	}
	
	private bool CheckForDuplicates(GameObject other)
	{
		bool duplicates = false;	
		
		GameObject parent = GameObject.Find("CompletionGroup");
		
		foreach (Transform child in parent.transform)
		{
			if (child.gameObject != this.gameObject)
			{
				CompletionChecker cc = child.gameObject.GetComponent<CompletionChecker>();
				
				if (cc.collidingWith != null)
				{
					if (other == cc.collidingWith.gameObject)
					{
						duplicates = true;
						break;
					}
				}
			}
		}
		
		return duplicates;
	}
	
	void OnTriggerEnter(Collider other)
	{	
		GameObject otherGO = GetTopParent(other);
		if (RequiredCollisions == 1)
		{
			if (!bComplete)
			{
				if (otherGO.name.Contains(CheckThisObject.name))
				{
					if (!CheckForDuplicates(otherGO))
					{	
						//Debug.Log(this.gameObject + " colliding With " + otherGO);
						collidingWith = otherGO;
						bComplete = true;	
					}
				}
			}
		}
		else
		{
			if (otherGO.name.Contains(CheckThisObject.name))
			{
				collidingLeaves.Add(otherGO);
			}
		}
	}
	
	void OnTriggerExit(Collider other)
	{	
		GameObject otherGO = GetTopParent(other);
		if (RequiredCollisions == 1)
		{
			if (bComplete)
			{
				if (otherGO.name.Contains(CheckThisObject.name))
				{
					bComplete = false;	
				}						
			}
		}
		else
		{
			if (otherGO.name.Contains(CheckThisObject.name))
			{
				collidingLeaves.Remove(otherGO);	
			}
		}
	}
	
}
