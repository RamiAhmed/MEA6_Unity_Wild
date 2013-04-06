using UnityEngine;
using System.Collections;

public class BuildingTrashCan : MonoBehaviour
{
	private GameObject playerRef = null;
	
	void OnTriggerEnter(Collider other)
	{
		if (playerRef == null)
			playerRef = GameObject.FindGameObjectWithTag("Player");
		
		GameObject otherGO = other.gameObject;
		while (otherGO.transform.parent != null)
			otherGO = otherGO.transform.parent.gameObject;
		
		BuildingObject otherBO = otherGO.gameObject.GetComponent<BuildingObject>();
		if (otherBO != null && otherBO.enabled)
		{
			if (!otherBO.GetHasUser())
			{
				playerRef.GetComponent<PlayerController>().placedObjects.Remove(otherGO);
				Destroy(otherGO);	
			}
		}
	}
}

