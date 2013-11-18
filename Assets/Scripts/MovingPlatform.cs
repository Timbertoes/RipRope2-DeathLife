using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
	
	public float moveX = 0.0f;
	public float moveY = 0.0f;
	
	private Vector3 startPosition;

	private int frame = 0;
	private float increment = 0f;

	// Use this for initialization
	void Start ()
	{
		startPosition = this.transform.position;
	}
	
	// Update is called once per frame
	//void Update ()
	void FixedUpdate ()
	{
		//Debug.Log("Update Platform's Velocity");
		increment += Time.deltaTime;

		//this.transform.position = startPosition;
		//this.transform.Translate(moveX*Mathf.Sin(Time.time), moveY*Mathf.Sin(Time.time), 0.0f);

		//rigidbody2D.velocity = new Vector2(moveX*Mathf.Sin(Time.time), moveY*Mathf.Sin(Time.time));

		this.transform.position = startPosition;
		//this.transform.Translate(moveX*Mathf.Sin(increment), moveY*Mathf.Sin(increment), 0.0f);
		this.transform.Translate(moveX*Mathf.Sin(Time.fixedTime), moveY*Mathf.Sin(Time.fixedTime), 0.0f);
		//Debug.Log(Time.fixedTime);
		/*
		frame++;

		if(frame % 5 == 0)
		{
			rigidbody2D.velocity = new Vector2(200.0f*Time.deltaTime, 200.0f*Time.deltaTime);
		}else{
			rigidbody2D.velocity = new Vector2(0.0f,0f);
		}*/


	}
}
