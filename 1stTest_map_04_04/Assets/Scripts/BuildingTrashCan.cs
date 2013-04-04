using UnityEngine;
using System.Collections;

public class BuildingTrashCan : MonoBehaviour
{
	void OnTriggerEnter(Collider other)
	{
		GameObject otherGO = other.gameObject;
		while (otherGO.transform.parent != null)
			otherGO = otherGO.transform.parent.gameObject;
		
		BuildingObject otherBO = otherGO.gameObject.GetComponent<BuildingObject>();
		if (otherBO != null && otherBO.enabled)
		{
			if (!otherBO.GetHasUser())
			{
				Destroy(otherGO);	
			}
		}
	}
}

