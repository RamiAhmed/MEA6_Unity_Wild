using UnityEngine;
using System;
using System.Collections;

public class ArrowIndicatorController : MonoBehaviour {
	
	public float activationDistance = 7.5f;
	public float floatSpeed = 2f;
	public float amplitude = 2f;
	public float rotationSpeed = 5f;
	
	private float startY = 0f;
	private GameObject pc = null;
	private MeshRenderer[] renderers;
	
	// Use this for initialization
	void Start () {
		startY = this.transform.position.y;
		renderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();
	}
	
	// Update is called once per frame
	void LateUpdate() {		
		if (pc == null)
		{
			try
			{
				pc = GameObject.FindGameObjectWithTag("Player");
			}
			catch (NullReferenceException e)
			{
				Debug.LogWarning("Could not find an active player: " + e);
			}
		}
		else
		{
		
			if ((pc.transform.position - this.transform.position).magnitude < activationDistance)
			{				
				ToggleRenderers(false);
			}
			else
			{		
				float yPos = startY + amplitude * Mathf.Sin(floatSpeed * Time.time);			
				this.transform.position = new Vector3(this.transform.position.x, yPos, this.transform.position.z);			
				this.transform.Rotate(this.transform.up, rotationSpeed);			
				
				ToggleRenderers(true);
			}
		}
	}
	
	private void ToggleRenderers(bool enable)
	{
		foreach (MeshRenderer r in renderers)
		{
			if (r.enabled != enable)
				r.enabled = enable;
		}
	}
}
