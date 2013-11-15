using UnityEngine;
using System.Collections;

public class PlayerControl:MonoBehaviour
{

	public Transform groundCheck;
	public LayerMask groundMask;
	public Transform sphereTrans; // Temp

	private float speed = 15.0f;

	private bool grounded = false;
	private bool jump = false;
	private bool onSlope = false;

	private int jumps = 0;

	// Custom Raycast Support
	private Vector3 rayOrigin = Vector3.zero;
	private Vector3 rayDirection = Vector3.zero;
	private RaycastHit2D hitBottomLeft;
	private RaycastHit2D hitBottomRight;
	private Vector3 PlayerBottomLeft = Vector3.zero;
	private Vector3 PlayerBottomRight = Vector3.zero;

	// Sprite / Character Visual Adjustment
	private Vector3 visualOffset = Vector3.zero;

	// Use this for initialization
	void Start()
	{
		Vector3 extents = new Vector3(0.5f, 1.0f, 0f);
		PlayerBottomLeft.Set(-extents.x, -extents.y, 0f);
		PlayerBottomRight.Set(extents.x, -extents.y, 0f);

	}
	
	// Update is called once per frame
	void Update()
	{
		float rayDist = 2.0f;
		float distTollerance = 1.0f / (50.0f*rayDist); // Small enough?

		//Debug.Log (distTollerance);

		rayOrigin = transform.position + PlayerBottomLeft;
		rayDirection = Vector3.down;
		hitBottomLeft = Physics2D.Raycast(rayOrigin, rayDirection, rayDist, groundMask);
		Debug.DrawRay(rayOrigin, rayDirection * rayDist * hitBottomLeft.fraction, Color.yellow);

		rayOrigin = transform.position + PlayerBottomRight;
		rayDirection = Vector3.down;
		hitBottomRight = Physics2D.Raycast(rayOrigin, rayDirection, rayDist, groundMask);
		Debug.DrawRay(rayOrigin, rayDirection * rayDist * hitBottomRight.fraction, Color.yellow);

		// Set Grounded Variables
		grounded = false;

		if(hitBottomLeft.collider != null && hitBottomLeft.fraction <= rayDist * distTollerance)
		{
			grounded = true;
		}
		if(hitBottomRight.collider != null && hitBottomRight.fraction <= rayDist * distTollerance)
		{
			grounded = true;
		}

		// Detect if on slope
		onSlope = false;

		if(hitBottomLeft.collider != null && hitBottomRight.collider != null)
		{
			// Both colliders are hitting something.
			if(hitBottomLeft.fraction != hitBottomRight.fraction)
			{
				onSlope = true;
			}
		}

		// Handle Jumps
		if(grounded)
		{
			jumps = 2; // Double Jump
		}

		if(Input.GetButtonDown("Jump") && jumps > 0)
		{
			jump = true;
		}
	}

	void FixedUpdate()
	{

		float xInput = Input.GetAxis("Horizontal"); // horizontal / X-Movement. Joystick compatible

		//Use input to move player left and right. Pass Y through.
		rigidbody2D.velocity = new Vector2(xInput * speed, rigidbody2D.velocity.y);

		if(grounded)
		{
			float xVelocity = rigidbody2D.velocity.x;
			float yVelocity = 0.0f;

			if(onSlope)
			{
				Vector2 averageNormal = (hitBottomLeft.normal + hitBottomRight.normal) * 0.5f;
				// Draw Normal in Green
				Debug.DrawRay(hitBottomLeft.point, averageNormal, Color.green);
				Debug.DrawRay(hitBottomRight.point, averageNormal, Color.green);

				//Draw moveDirection in Red
				averageNormal.Set(averageNormal.y, -averageNormal.x);
				averageNormal.Normalize();
				averageNormal*= (xInput * speed);

				Debug.DrawRay(hitBottomLeft.point, averageNormal*0.1f, Color.red);
				Debug.DrawRay(hitBottomRight.point, averageNormal*0.1f, Color.red);

				// Debug.Log (averageNormal);

				xVelocity = averageNormal.x;
				yVelocity = averageNormal.y;
			}
			Debug.Log(hitBottomLeft.fraction);

			rigidbody2D.velocity = new Vector2(xVelocity, yVelocity);

		}else{
			rigidbody2D.AddForce(new Vector2(0f,-4000f)); // Gravity
		}

		if(jump && jumps > 0)
		{
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 25f); // Set X absolutely, preserve Y
			jump = false;
			jumps--;
		}

		// Update Graphics
		if(grounded)
		{
			//hitBottomLeft.fraction
			//hitBottomRight.fraction
			visualOffset.Set(0f,-1f - ((hitBottomLeft.fraction+hitBottomRight.fraction)*0.5f),0f);
		}else{
			visualOffset.Set(0f,-1f,0f);
		}

		/*
		if(Input.GetKeyDown(KeyCode.LeftShift))
		{
			rigidbody2D.AddForce(new Vector2(1000000f,0f));
		}
		*/

		//sphereTrans.localPosition = visualOffset;

	}
	
}

/*
 * 
 * Platforming Requirements:
 *   [x] Player can move left and right
 *      [x] Player should be able to stand near ledges without sinking (Capsule Problems)
 *   [x] Player can go up and down slopes
 *      [x] Player should 'Stick' to slopes. No bouncing downward.
 *   [x] Player can jump / double jump
 *   [ ] Player can dash
 *   [ ] Player can cling / slide down walls
 *      [ ] While clinging, player can jump to let go or wall jump upward / outward
 * 
 */

