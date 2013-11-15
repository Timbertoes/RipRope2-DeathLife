using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
	
	public float moveX = 0.0f;
	public float moveY = 0.0f;
	
	private Vector3 startPosition;
	
	// Use this for initialization
	void Start ()
	{
		startPosition = this.transform.position;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		//this.transform.position = startPosition;
		//this.transform.Translate(moveX*Mathf.Sin(Time.time), moveY*Mathf.Sin(Time.time), 0.0f);
		rigidbody2D.velocity = new Vector2(moveX*Mathf.Sin(Time.time), moveY*Mathf.Sin(Time.time));
	}
}
