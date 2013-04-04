using UnityEngine;
using System.Collections;

public class BuildingTrashCan : MonoBehaviour
{
	void OnTriggerEnter(Collider other)
	{
		BuildingObject otherBO = other.gameObject.GetComponent<BuildingObject>();
		if (otherBO != null && otherBO.enabled)
		{
			if (!otherBO.GetHasUser())
			{
				Destroy(other.gameObject);	
			}
		}
	}
}

