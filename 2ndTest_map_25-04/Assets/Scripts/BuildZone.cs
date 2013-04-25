using UnityEngine;
using System.Collections;

public class BuildZone : MonoBehaviour {
	
	private PlayerController pController = null;
	
	// Update is called once per frame
	void Start () {
		if (pController == null)
		{
			pController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		}	
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (pController != null)
		{
			if (other.gameObject == pController.gameObject)
			{
				if (!pController.bIsTimeCounting)
				{
					pController.bIsTimeCounting = true;		
				}
			}
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if (pController != null)
		{
			if (other.gameObject == pController.gameObject)
			{
				if (pController.bIsTimeCounting)
				{
					pController.bIsTimeCounting = false;			
				}
			}
		}
	}
}
