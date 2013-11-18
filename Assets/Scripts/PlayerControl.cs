using UnityEngine;
using System.Collections;

public class PlayerControl:MonoBehaviour
{
	
	public LayerMask groundMask;   // Easier to set the mask in the inspector rather than shifting bits.
	public Transform sphereTrans;  // Temp - Used for snapping the sprite / model to the surface on slopes

	public Material activePlatformMaterial; // For easier debugging
	public Material defaultMaterial;

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
	private float rayDist = 2.0f; 			            // Distance to cast rays
	private float distTollerance = 0.01f;              // Distance to consider grounded (percent of fraction)

	// Sprite / Character Visual Adjustment
	private Vector3 visualOffset = Vector3.zero; // Offset of sprite / model from collision box

	// Moving platform support
	private Transform activePlatform;
	private Vector3 activeLocalPlatformPoint;
	private Vector3 activeGlobalPlatformPoint;
	private Vector3 lastPlatformVelocity;

	//private Transform lastPlatform; // Deprecated
	private Vector3 lastPlatformPosition;

	// Begin MonoBehaviour Methods

	void Start()
	{
		activePlatform = null;
		//lastPlatform = null;

		// Get extents for gathering player bounds and offsets
		Vector3 extents = GetComponent<BoxCollider2D>().size * 0.5f; // No longer hard-coded

		PlayerBottomLeft.Set(-extents.x, -extents.y, 0f);
		PlayerBottomRight.Set(extents.x, -extents.y, 0f);

		// Raycast Support
		rayDist = 2.0f; // Distance to cast rays

		//If distTollerance is too low, we won't ground and be able to jump.
		distTollerance = (1.0f / (50.0f*rayDist)); // Small enough? Tolerance for considering player grounded.
		distTollerance *= rayDist; // Scale to rayDist since we get it as a percent from the RaycastHit2D
	}

	void Update()
	{
		// Update detects input and sets variables. Movement/Physics in FixedUpdate
		if(Input.GetButtonDown("Jump") && jumps > 0)
		{
			jump = true;
		}

	}

	void FixedUpdate()
	{
		// Raycasts. TODO considder using RaycastNonAlloc for memory purposes

		//distTollerance = 0.06f;
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
		
		//if(hitBottomLeft.collider != null && hitBottomLeft.fraction <= rayDist * distTollerance)
		if(raycastDistance(hitBottomLeft, distTollerance))
		{
			// Bottom left raycast hit something, and its within the tollerance. That foot is grounded
			grounded = true;
		}

		//if(hitBottomRight.collider != null && hitBottomRight.fraction <= rayDist * distTollerance)
		if(raycastDistance(hitBottomRight, distTollerance))
		{
			// Bottom right raycast hit something, and its within the tollerance. That foot is grounded
			grounded = true;
		}

		//Debug.Log (hitBottomRight.

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

		// FixedUpdate handles actual movement.

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
				//averageNormal.Normalize();

				//This was to reduce speed as you go up a slope. Not really necessary. Added slide.
				/*
				float slopeMultiplier = 1.0f;
				float slopeTollerance = 0.8f;

				//Debug.Log("averageNormal: "+averageNormal);

				// Set slop limit here by normal tollerance
				if(averageNormal.y > slopeTollerance)
				{
					// On an average to horizontal slope
					slopeMultiplier = 1.0f;
				}else{
					// On a steep slope. Normal.y <= 0.8
					//Debug.Log( 1.0f - ((slopeTollerance - averageNormal.y)*2.0f) );
					slopeMultiplier = Mathf.Clamp(1.0f - ((slopeTollerance - averageNormal.y)*2.0f), 0.0f, 1.0f);
					Debug.Log(slopeMultiplier);
				}
				*/

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
			// Debug.Log(hitBottomLeft.fraction);

			// TODO: horizontal movement on platforms inherits a lot. See if it should be reduced.
			// Mod if on platform
			//xVelocity -= lastPlatformVelocity.x;
			//yVelocity -= lastPlatformVelocity.y;

			// Set velocity to whatever the variables have been set to by now (sloped or not sloped)
			rigidbody2D.velocity = new Vector2(xVelocity, yVelocity);

			//Debug.Log ("SetVelocity: "+rigidbody2D.velocity);

		}else{

			// Not grounded. Add Gravity.
			rigidbody2D.AddForce(new Vector2(0f,-4000f)); // Gravity
			//rigidbody2D.transform.Translate(0f,-0.1f,0f); // Gravity
			//rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, rigidbody2D.velocity.y - 1f);
		}

		//Debug.Log("Grounded? : "+grounded + " dist: " + hitBottomLeft.fraction);

		// Jumping
		if(jump && jumps > 0)
		{
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 25f);
			jump = false;
			jumps--;
		}

		// Update Graphics - Offset from Collision Box
		if(grounded)
		{
			visualOffset.Set(0f,-1f - ((hitBottomLeft.fraction+hitBottomRight.fraction)*0.5f),0f);
		}else{
			visualOffset.Set(0f,-1f,0f);
		}

		//sphereTrans.localPosition = visualOffset;

		movingPlatformLogic();
	} // End FixedUpdate
	
	// Begin custom methods - Alphabetical
	void movingPlatformLogic()
	{
		//TODO: Clean it up a bit.
		Collider2D leftPlatform = null;
		Collider2D rightPlatform = null;
		Collider2D newPlatform = null;

		// Detect if either ray has a potential platform beneath it
		if(raycastDistance(hitBottomLeft, distTollerance) && hitBottomLeft.normal.y > 0.5f)
		{
			// Left foot is grounded
			leftPlatform = hitBottomLeft.collider;
		}

		if(raycastDistance(hitBottomRight, distTollerance) && hitBottomRight.normal.y > 0.5f)
		{
			// Right foot is grounded
			rightPlatform = hitBottomRight.collider;
		}

		if(leftPlatform == null && rightPlatform == null)
		{
			if(activePlatform != null)
			{
				Debug.Log("(Set Active Platform to NULL)");
				activePlatform.renderer.sharedMaterial = defaultMaterial;
				activePlatform = null; // Reset
				//lastPlatform = null;
			}
		}
		else if(leftPlatform != null && rightPlatform != null && activePlatform != null)
		{
			newPlatform = null; // Transferring horizontal platforms while one is active.
		}
		else if(leftPlatform != null)
		{
			newPlatform = leftPlatform;
		}
		else if(rightPlatform != null)
		{
			newPlatform = rightPlatform;
		}

		if(newPlatform != null && activePlatform != newPlatform.collider2D.transform)
		{
			// New Platform
			Debug.Log("Set Active Platform to "+newPlatform);
			if(activePlatform != null)
			{
				activePlatform.renderer.sharedMaterial = defaultMaterial;
			}

			activePlatform = newPlatform.collider2D.transform;
			activePlatform.renderer.sharedMaterial = activePlatformMaterial;

			lastPlatformPosition = activePlatform.position;
			//lastPlatform = activePlatform;
		}

		// Andy Method
		if (activePlatform != null)
		{
			lastPlatformVelocity = activePlatform.position - lastPlatformPosition;
			lastPlatformPosition = activePlatform.position;

			//Debug.Log("Get Platform Vel: "+lastPlatformVelocity);
			transform.Translate(lastPlatformVelocity);
		}
	}

	bool raycastDistance(RaycastHit2D hit, float dist)
	{
		if(hit.collider != null && hit.fraction <= dist)
		{
			return true;
		}
		return false;
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
 *   [x] Player can interact with moving platforms
 *   [ ] Player can dash
 *   [ ] Player can cling / slide down walls
 *      [ ] While clinging, player can jump to let go or wall jump upward / outward
 * 
 */

