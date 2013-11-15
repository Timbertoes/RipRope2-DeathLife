using UnityEngine;
using System.Collections;

public class PlayerControl:MonoBehaviour
{
	
	public LayerMask groundMask;   // Easier to set the mask in the inspector rather than shifting bits.
	public Transform sphereTrans;  // Temp - Used for snapping the sprite / model to the surface on slopes

	private float speed = 15.0f;   // Generic Player Speed - Used in Fixed Update

	private bool grounded = false; // Is the player touching the ground in any way?
	private bool jump = false;     // Should the player jump this frame?
	private bool onSlope = false;  // Is the player on uneven terrain / a slope?

	private int jumps = 0;         // Jumps left. 1 = Single Jump, 2 = Double Jump

	// Custom Raycast Support
	private Vector3 rayOrigin = Vector3.zero;         // Reusable Ray Origin Varible
	private Vector3 rayDirection = Vector3.zero;      // Reusable Ray Direction Variable
	private RaycastHit2D hitBottomLeft;               // Hit info for bottom left (downward) raycast
	private RaycastHit2D hitBottomRight;              // Hit info for bottom right (downward) raycast
	private Vector3 PlayerBottomLeft = Vector3.zero;  // Offset from player origin to bottom-left
	private Vector3 PlayerBottomRight = Vector3.zero; // Offset from player origin to bottom-right

	// Sprite / Character Visual Adjustment
	private Vector3 visualOffset = Vector3.zero; // Offset of sprite / model from collision box

	// Begin MonoBehaviour Methods

	void Start()
	{
		// Get extents for gathering player bounds and offsets
		Vector3 extents = GetComponent<BoxCollider2D>().size * 0.5f; // No longer hard-coded

		PlayerBottomLeft.Set(-extents.x, -extents.y, 0f);
		PlayerBottomRight.Set(extents.x, -extents.y, 0f);
	}

	void Update()
	{
		float rayDist = 2.0f; // Distance to cast rays
		float distTollerance = 1.0f / (50.0f*rayDist); // Small enough? Tolerance for considering player grounded
		//Debug.Log (distTollerance);

		//TODO wrap these into a more efficient function

		// Raycast down from bottom left
		rayOrigin = transform.position + PlayerBottomLeft;
		rayDirection = Vector3.down;
		hitBottomLeft = Physics2D.Raycast(rayOrigin, rayDirection, rayDist, groundMask);
		Debug.DrawRay(rayOrigin, rayDirection * rayDist * hitBottomLeft.fraction, Color.yellow);

		// Raycast down from bottom right
		rayOrigin = transform.position + PlayerBottomRight;
		rayDirection = Vector3.down;
		hitBottomRight = Physics2D.Raycast(rayOrigin, rayDirection, rayDist, groundMask);
		Debug.DrawRay(rayOrigin, rayDirection * rayDist * hitBottomRight.fraction, Color.yellow);

		// Set Grounded Variables
		grounded = false; // Reset

		if(hitBottomLeft.collider != null && hitBottomLeft.fraction <= rayDist * distTollerance)
		{
			// Bottom left raycast hit something, and its within the tollerance. That foot is grounded
			grounded = true;
		}
		if(hitBottomRight.collider != null && hitBottomRight.fraction <= rayDist * distTollerance)
		{
			// Bottom right raycast hit something, and its within the tollerance. That foot is grounded
			grounded = true;
		}

		// Detect if on slope
		onSlope = false; // Reset

		if(hitBottomLeft.collider != null && hitBottomRight.collider != null)
		{
			// Both colliders are hitting something.
			if(hitBottomLeft.fraction != hitBottomRight.fraction)
			{
				// Distance for both hits is unequal. Terrain is uneven / sloping
				onSlope = true;
			}
		}

		// Handle Jumps
		if(grounded)
		{
			jumps = 2; // Reset if grounded. 1 = Single, 2 = Double Jump
		}

		if(Input.GetButtonDown("Jump") && jumps > 0)
		{
			jump = true;
		}
	}

	void FixedUpdate()
	{

		float xInput = Input.GetAxis("Horizontal"); // horizontal / X-Movement. Joystick compatible

		//Use input to move player left and right. Pass Y-Velocity through.
		rigidbody2D.velocity = new Vector2(xInput * speed, rigidbody2D.velocity.y);

		if(grounded)
		{
			// When grounded, allow simply moving left and right.
			float xVelocity = rigidbody2D.velocity.x;
			float yVelocity = 0.0f;

			if(onSlope)
			{
				// When grounded AND on a slope, lock onto the slope -- no bouncing downhill.
				Vector2 averageNormal = (hitBottomLeft.normal + hitBottomRight.normal) * 0.5f; // Averaged hit normal of both rayCasts

				// Draw Normal in Green
				Debug.DrawRay(hitBottomLeft.point, averageNormal, Color.green);
				Debug.DrawRay(hitBottomRight.point, averageNormal, Color.green);

				averageNormal.Set(averageNormal.y, -averageNormal.x); // Rotate averageNormal by 90 degrees
				averageNormal.Normalize();                            // Normalize so we can scale it
				averageNormal*= (xInput * speed);                     // Scale it by move speed

				// Draw moveDirection in Red
				Debug.DrawRay(hitBottomLeft.point, averageNormal*0.1f, Color.red);
				Debug.DrawRay(hitBottomRight.point, averageNormal*0.1f, Color.red);

				// Set speed parallel to the average surface.
				xVelocity = averageNormal.x;
				yVelocity = averageNormal.y;
			}
			Debug.Log(hitBottomLeft.fraction);

			// Set velocity to whatever the variables have been set to by now (sloped or not sloped)
			rigidbody2D.velocity = new Vector2(xVelocity, yVelocity);

		}else{

			// Not grounded. Add Gravity.
			rigidbody2D.AddForce(new Vector2(0f,-4000f)); // Gravity
		}

		// Jumping
		if(jump && jumps > 0)
		{
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 25f); // Set X absolutely, preserve Y
			jump = false;
			jumps--;
		}

		// Update Graphics - Offset from Collision Box
		if(grounded)
		{
			//hitBottomLeft.fraction
			//hitBottomRight.fraction
			visualOffset.Set(0f,-1f - ((hitBottomLeft.fraction+hitBottomRight.fraction)*0.5f),0f);
		}else{
			visualOffset.Set(0f,-1f,0f);
		}

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

