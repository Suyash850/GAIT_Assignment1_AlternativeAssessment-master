using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour 
{
	// Linear motion
	public float speed = 3f;
	public float acceleration = 10f;
	public bool SnapToRightAngles = false;
	public float SnapTime = 0.2f;

	// Angular motion
	public float angularAccelelation = 5.0f;
	public float angularMaxSpeed = 180.0f;
	public float angleAdjustment = 0.0f; // In case the sprite isn't facing left (0 degrees) to begin with

	private float angularPosition;

	private float actualRotation;
	private float previousRotation;
	private float timeCurrentRotation = 0.0f;
	private float overrideRotation = float.MinValue; // For rotating the frog when it shoots bubbles

	public void OverrideRotation(float overrideRotation) {
		this.overrideRotation = overrideRotation;
	}
	
	public void StopOverrideRotation() {
		this.overrideRotation = float.MinValue;
	}

	public void Start() {

		angularPosition = GetComponent<Rigidbody2D>().transform.localEulerAngles.z;

		// Rotate to the initial position
		GetComponent<Rigidbody2D>().transform.localEulerAngles = new Vector3(0.0f, 0.0f, angularPosition + angleAdjustment);
	}

	public void FaceDown() {
		actualRotation = 270.0f + angleAdjustment;
		previousRotation = actualRotation;
		angularPosition = actualRotation - angleAdjustment;
		GetComponent<Rigidbody2D>().transform.localEulerAngles = new Vector3(0.0f, 0.0f, actualRotation);
	}
	
	public void Move(Vector3 desired_velocity)
	{
		// Get the vector's direction.
		if (desired_velocity.sqrMagnitude > 1)
			desired_velocity = desired_velocity.normalized;
		
		// Give it some speed.
		desired_velocity *= speed;

		// TODO: Modify the below line to that instead of immediately setting the rigidbody's velocity
		// to the desired velocity, it *accelerates* until it reaches the desired velocity. The change in
		// the rigidbody's velocity should not exceed (acceleration * Time.deltaTime).
		GetComponent<Rigidbody2D>().velocity = desired_velocity;

		// Angular movement
		if ((desired_velocity != Vector3.zero) || (overrideRotation != float.MinValue)) { // Don't rotate unless we're moving or firing

			float targetAngularVel;
			
			if (overrideRotation != float.MinValue) {
				targetAngularVel = overrideRotation - angularPosition;
			} else {
				targetAngularVel = Mathf.Rad2Deg * Mathf.Atan2(desired_velocity.y, desired_velocity.x) - angularPosition;
			}

			while (targetAngularVel > 180.0f)
				targetAngularVel -= 360.0f;
			
			while (targetAngularVel < -180.0f)
				targetAngularVel += 360.0f;

			if (Mathf.Abs(targetAngularVel) > angularMaxSpeed) {
				targetAngularVel *= (angularMaxSpeed / Mathf.Abs(targetAngularVel));
			}

			targetAngularVel *= angularAccelelation;
			
			angularPosition = angularPosition + targetAngularVel * Time.deltaTime;

			actualRotation = angularPosition + angleAdjustment;

			while (actualRotation < 0.0f)
				actualRotation += 360.0f;
			
			while (actualRotation > 360.0f)
				actualRotation -= 360.0f;

			if (SnapToRightAngles) {

				if ((actualRotation > 45.0f) && (actualRotation < 135.0f)) {
					actualRotation = 90.0f;
				} else if ((actualRotation > 135.0f) && (actualRotation < 225.0f)) {
					actualRotation = 180.0f;
				} else if ((actualRotation > 225.0f) && (actualRotation < 315.0f)) {
					actualRotation = 270.0f;
				} else if ((actualRotation > 315.0f) || (actualRotation < 45.0f)) {
					actualRotation = 0.0f;
				}

				if (previousRotation == actualRotation) {
					
					timeCurrentRotation += Time.deltaTime;
					
					if (timeCurrentRotation > SnapTime) {
						GetComponent<Rigidbody2D>().transform.localEulerAngles = new Vector3(0.0f, 0.0f, actualRotation);
					}
					
				} else {
					timeCurrentRotation = 0.0f;
				}
				
				previousRotation = actualRotation;
			} else {

				// We're not snapping so just apply the rotation every frame
				GetComponent<Rigidbody2D>().transform.localEulerAngles = new Vector3(0.0f, 0.0f, actualRotation);
			}
		}
	}
}
