using UnityEngine;
using System.Collections;

public class TopDownMouseLook : MonoBehaviour {
	
	private GameObject player;
	
	// Use this for initialization
	void Start () {
		player = GameObject.FindGameObjectWithTag("Player");
		if (player == null)
			Debug.LogWarning("TopDownMouseLook Could not find a valid player.");
	}
	
	// Update is called once per frame
	void Update() {
		if (Input.GetMouseButton(1))
		{
			
		}
		
		/*
		if (Input.GetMouseButton(1))
		{
			Vector3 referenceForward = player.transform.forward;
			Vector3 referenceRight = Vector3.Cross(Vector3.up, referenceForward);
			
			Vector3 newDirection = this.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.camera.farClipPlane));
			
			float angle = Vector3.Angle(newDirection, referenceForward);
			float sign = (Vector3.Dot(newDirection, referenceRight) > 0.0f) ? 1.0f: -1.0f;
			
			float rotationSpeed = 5f;
			// Right of the reference vector
			if (sign > 0.0f)
			{
				Debug.Log("Rotate rightwards");	
				this.transform.Rotate(Vector3.forward, rotationSpeed);
			}
			else
			{
				Debug.Log("Rotate leftwards");
				this.transform.Rotate(Vector3.forward, -rotationSpeed);
			}
		}
		*/
		/*
		if (Input.GetMouseButton(1))
		{
			Vector3 mousePos = this.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, this.camera.farClipPlane));
			mousePos.y = 0;
			Debug.Log("MousePos: " + mousePos);
			
			player.transform.LookAt(mousePos);
		}
		*/
		/*
		Ray mouseRay = this.camera.ScreenPointToRay(Input.mousePosition);
		
		if (Input.GetMouseButtonDown(1))
		{
			RaycastHit[] hits = Physics.RaycastAll(mouseRay.origin, mouseRay.direction, (player.transform.position.y * 2f));
			foreach (RaycastHit hit in hits)
			{
				if (hit.collider.GetType() == typeof(TerrainCollider))
				{
					Vector3 pos = hit.transform.position;//player.transform.TransformPoint(new Vector3(hit.point.x, player.transform.position.y, hit.point.z));
					Debug.Log("Fire at " + pos);
					player.transform.LookAt(pos, Vector3.up);
					//this.transform.LookAt(new Vector3(this.transform.position.x, pos.y, this.transform.position.z));
					break;
				}
			}
		}
		*/
	}
}
