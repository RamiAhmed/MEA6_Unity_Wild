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
		
		//Vector3 mousePos = this.camera.ScreenToWorldPoint(Input.mousePosition);
		Ray mouseRay = this.camera.ScreenPointToRay(Input.mousePosition);
		
		if (Input.GetMouseButtonDown(1))
		{
			RaycastHit[] hits = Physics.RaycastAll(mouseRay.origin, mouseRay.direction, (player.transform.position.y * 2f));
			foreach (RaycastHit hit in hits)
			{
				if (hit.collider.GetType() == typeof(TerrainCollider))
				{
					Vector3 pos = player.transform.TransformPoint(new Vector3(hit.point.x, player.transform.position.y, hit.point.z));
					Debug.Log("Fire at " + pos);
					player.transform.LookAt(pos, Vector3.up);
					//this.transform.LookAt(new Vector3(this.transform.position.x, pos.y, this.transform.position.z));
					break;
				}
			}
		}
	}
}
