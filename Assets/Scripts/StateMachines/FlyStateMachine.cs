using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AppleTreeTargeter))]
[RequireComponent(typeof(GameObjectTargeter))]
[RequireComponent(typeof(Seek))]
[RequireComponent(typeof(Movement))]
public class FlyStateMachine : MonoBehaviour {

	private enum State {
		Eating,
		SeekFood, 
		Fleeing
	}

	private Targeter appleTreeTargeter;
	private GameObjectTargeter playerTargeter;
	private Seek seekComponent;
	private Movement movement;
	private State currentState;
	private float timeEating;
	private bool doneEating;
	private float fleeSpeed;
	private static AudioSource SoundSource; // Static so we don't get a weird chorus effect when all flies flee at once

	public float fleeDistance;
	public float appleTreeSpeed;
	public float appleTreeDist;
	public float maxEatTime;
	public AudioClip FleeSound;

	void Awake () {

		appleTreeTargeter = (AppleTreeTargeter)GetComponent<AppleTreeTargeter>();
		playerTargeter = (GameObjectTargeter)GetComponent<GameObjectTargeter>();
		seekComponent = (Seek)GetComponent<Seek>();
		movement = (Movement)GetComponent<Movement>();

		playerTargeter.Target = GameObject.Find("Player");

		ResetEatingVars();

		addAudioSource();

		// Difficulty settings
		
		int difficulty = 1;
		GameObject optionsGameObj = GameObject.Find("GameOptions");
		
		// We won't be able to find the options GameObject if we haven't gone via the main menu
		if (optionsGameObj != null) {
			GameOptions options = optionsGameObj.GetComponent<GameOptions>();
			difficulty = options.difficulty;
		}

		switch (difficulty) {
		case 0:
			// Easy
			fleeSpeed = 2.5f;
			break;
		case 1:
		default:
			// Normal
			fleeSpeed = 3.0f;
			break;
		case 2:
			// Hard
			fleeSpeed = 3.2f;
			break;
		case 3:
			// Insane
			fleeSpeed = 4.0f;
			break;
		}
	}

	private void addAudioSource() {
		
		SoundSource = gameObject.AddComponent<AudioSource>();
		SoundSource.loop = false;
		SoundSource.volume = 0.65f; // The flies do get pretty annoying on full volume...
		SoundSource.pitch = 0.8f; // The noise is a bit high to start with
	}
	
	// Update is called once per frame
	void Update () {
		UpdateState();

		switch (currentState)
		{
			case State.Fleeing:
				seekComponent.flee = true;
				seekComponent.SetTargeter(playerTargeter);
				movement.speed = fleeSpeed;
				break;
			case State.SeekFood:
				seekComponent.flee = false;
				seekComponent.SetTargeter(appleTreeTargeter);
				movement.speed = appleTreeSpeed;
				break;
			case State.Eating:
				timeEating += Time.deltaTime;
				if (timeEating >= maxEatTime)
				{
					doneEating = true;
					((AppleTreeTargeter)appleTreeTargeter).UpdateTree();
				}
				break;
		}
	}

	// Determine the flies current state.
	private void UpdateState() {
		float distanceFromPlayer;
		Vector2? playerPos = playerTargeter.GetTarget();
		
		if (playerPos == null) {
			distanceFromPlayer = float.MaxValue;
		} else {
			distanceFromPlayer = ((Vector2)(transform.position) - (Vector2)(playerTargeter.GetTarget())).magnitude;
		}

		float distanceFromAppleTree = Vector2.Distance((Vector2)appleTreeTargeter.GetTarget(), (Vector2)transform.position);

		if (SoundSource == null) {
			addAudioSource();
		}

		// Only if the player is close and not underwater.
		if (distanceFromPlayer < fleeDistance && !PlayerInfo.IsUnderwater()) {
			if (currentState != State.Fleeing && !SoundSource.isPlaying) { // Don't stop/start the sound every time a new fly flees
				SoundSource.clip = FleeSound;
				SoundSource.Play();
			}
			currentState = State.Fleeing;
			ResetEatingVars();
			//Random chance to change the tree the fly will visit to eat.
			if(Random.Range(0,2) == 0) 
				((AppleTreeTargeter)appleTreeTargeter).UpdateTree();
		} else if ( (distanceFromAppleTree < appleTreeDist) && !doneEating ){
			currentState = State.Eating;
		} else {
			currentState = State.SeekFood;
			ResetEatingVars();
		}
	}

	private void ResetEatingVars()
	{
		doneEating = false;
		timeEating = 0f;
	}
}
