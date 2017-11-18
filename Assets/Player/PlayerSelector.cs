using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure; // Required in C#

public class PlayerSelector : MonoBehaviour {

	PlayerIdentity identity;

	public float speed;
	
	Unit currentUnit;

	GamePadState padState;

	public float moveTime = 1f;

	public AnimationCurve scoreByDot;
	public AnimationCurve scoreByDistance;

	Vector2 lastLeftStickMovement;

	Coroutine cameraCo;

	bool aWasPressed;

	void Awake() {
		identity = this.GetComponent<PlayerIdentity>();
		padState = GamePad.GetState((PlayerIndex)identity.id);
	}

	void Start() {
		cameraCo = StartCoroutine(FollowUnit());
		currentUnit = Unit.allUnits[identity.id][0];
		if (currentUnit)
			currentUnit.SetSelected(true);
	}

	void Update() {
		padState = GamePad.GetState((PlayerIndex)identity.id);


		Vector2 leftJoystick = GetLeftJoystickDir();

		bool aIsPressed = padState.Buttons.A == ButtonState.Pressed;
		if (aIsPressed && !aWasPressed) {
			MoveToAnotherUnit(leftJoystick.normalized);
		}
		aWasPressed = aIsPressed;

		if (!currentUnit)
			return;

		if (leftJoystick.magnitude > .5f) {
			lastLeftStickMovement = leftJoystick;
			currentUnit.CommandMoveInDirection(leftJoystick);
		}
		if (padState.Triggers.Right > .5f) {
			currentUnit.CommandFire(lastLeftStickMovement);
		}
	}

	void MoveToAnotherUnit(Vector2 dir) {
		Unit currentBest = null;
		foreach (Unit u in Unit.allUnits[identity.id]) {
			if (u == null)
				continue;
			if (u == currentUnit)
				continue;
			if (currentBest == null) {
				currentBest = u;
				continue;
			}
			float currentBestScore = GetUnitScore(dir, currentBest);
			float thisScore = GetUnitScore(dir, u);

			if (thisScore > currentBestScore)
				currentBest = u;
		}
		if (currentBest) {
			if (currentUnit)
				currentUnit.SetSelected(false);
			currentBest.SetSelected(true);

			currentUnit = currentBest;
			StopCoroutine(cameraCo);
			cameraCo = StartCoroutine(MoveToNewUnit());
		}
	}

	float GetUnitScore(Vector2 dir, Unit u) {
		float distance = Vector3.Distance(this.transform.position, u.transform.position);
		float dot = Vector3.Dot(dir, (u.transform.position - this.transform.position).normalized);

		float score = 0;
		score += scoreByDot.Evaluate(dot);
		score += scoreByDistance.Evaluate(distance);
		Debug.Log(score);
		return score;
	}

	Vector2 GetLeftJoystickDir() {
		return new Vector2(padState.ThumbSticks.Left.X, padState.ThumbSticks.Left.Y);
	}

	IEnumerator FollowUnit() {
		while (true) {
			if (currentUnit)
				this.transform.position = currentUnit.transform.position;
			yield return null;
		}
	}

	IEnumerator MoveToNewUnit() {
		Vector3 from = this.transform.position;
		for (float t = 0; t < moveTime; t += Time.deltaTime) {
			if (currentUnit)
				this.transform.position = Vector2.Lerp(from, currentUnit.transform.position, Mathf.Pow(t / moveTime, .5f));
			yield return null;
		}
		cameraCo = StartCoroutine(FollowUnit());
	}

}
