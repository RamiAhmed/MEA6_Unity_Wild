using UnityEngine;
using System.Collections;

public class BuildingTrashCan : MonoBehaviour
{
	private PlayerController playerRef = null;
	
	void OnTriggerEnter(Collider other)
	{
		if (playerRef == null)
			playerRef = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		
		GameObject otherGO = other.gameObject;
		while (otherGO.transform.parent != null)
			otherGO = otherGO.transform.parent.gameObject;
		
		BuildingObject otherBO = otherGO.gameObject.GetComponent<BuildingObject>();
		if (otherBO != null && otherBO.enabled)
		{
			if (!otherBO.GetHasUser())
			{
				playerRef.placedObjects.Remove(otherGO);
				Destroy(otherGO);	
			}
		}
	}
}

