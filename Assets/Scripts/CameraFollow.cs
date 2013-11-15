using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	
	public Transform followObj;
	
	private Vector3 targetPos;
	private Vector3 newCameraPos;
	
	// Use this for initialization
	void Start()
	{
		newCameraPos = this.transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
		targetPos = followObj.position;
		
		newCameraPos.x -= (this.transform.position.x - targetPos.x) * 0.1f;
		newCameraPos.y -= (this.transform.position.y - targetPos.y) * 0.1f;
		
		this.transform.position = newCameraPos;
	}
}
