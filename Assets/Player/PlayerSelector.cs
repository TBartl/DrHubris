using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure; // Required in C#

public class PlayerSelector : MonoBehaviour {

	PlayerIdentity identity;

	public float maxTransferDistance = 10f;
	public float speed;
	
	Unit currentUnit;

	GamePadState padState;

	public float moveTime = 1f;

	public AnimationCurve scoreByDot;
	public AnimationCurve scoreByDistance;

	Vector2 lastLeftStickMovement;

	Coroutine cameraCo;

	bool aWasPressed;

	List<Unit> connectedUnits;
	Unit bestConnectedUnit;

	public int connectionLinePoolSize = 16;
	public GameObject connectionLinePrefab;
	List<LineRenderer> connectionLines = new List<LineRenderer>();
	public Material targetConnectionMat;
	public AnimationCurve lrWidthByDistPercent;

	public float reloadTime;
	bool reloaded = true;
	public float maxMoveBufferTime = .5f;
	float moveBufferTime;
	

	void Awake() {
		identity = this.GetComponent<PlayerIdentity>();
		padState = GamePad.GetState((PlayerIndex)identity.id);
	}

	void Start() {
		cameraCo = StartCoroutine(FollowUnit());
		//currentUnit = Unit.allUnits[identity.id][0];
		if (currentUnit)
			currentUnit.SetSelected(true);

		for (int i = 0; i < connectionLinePoolSize; i++) {
			connectionLines.Add(Instantiate(connectionLinePrefab, this.transform).GetComponent<LineRenderer>());
		}
	}

	void Update() {
		padState = GamePad.GetState((PlayerIndex)identity.id);
		
		Vector2 leftJoystick = GetLeftJoystickDir();

		UpdateBestUnit(leftJoystick.normalized);

		bool aIsPressed = padState.Buttons.A == ButtonState.Pressed;
		if (aIsPressed && !aWasPressed) {
			moveBufferTime = maxMoveBufferTime;
		}
		if (moveBufferTime > 0 && reloaded) {
			MoveToAnotherUnit(leftJoystick.normalized);
			StartCoroutine(Reload());
		}
		aWasPressed = aIsPressed;
		moveBufferTime -= Time.deltaTime;

		DrawConnections();

		if (!currentUnit)
			return;

		if (leftJoystick.magnitude > .5f) {
			lastLeftStickMovement = leftJoystick;
			currentUnit.CommandMoveInDirection(leftJoystick);
		}
		if (padState.Buttons.B == ButtonState.Pressed) {
			currentUnit.CommandFire(lastLeftStickMovement);
		}
	}

	void UpdateBestUnit(Vector2 dir) {
		bestConnectedUnit = null;
		connectedUnits = new List<Unit>();
		foreach (Unit u in Unit.allUnits[identity.id]) {
			if (u == null)
				continue;
			if (u == currentUnit)
				continue;
			if (currentUnit != null) {
				float dist = Vector3.Distance(this.transform.position, u.transform.position);
				if (dist > maxTransferDistance)
					continue;
			}

			connectedUnits.Add(u);

			if (bestConnectedUnit == null) {
				bestConnectedUnit = u;
				continue;
			}

			float currentBestScore = GetUnitScore(dir, bestConnectedUnit);
			float thisScore = GetUnitScore(dir, u);

			if (thisScore > currentBestScore)
				bestConnectedUnit = u;
		}
	}

	void DrawConnections() {
		int unitIndex = 0;
		Vector3 offset = Vector3.forward * .05f;
		foreach (LineRenderer lr in connectionLines) {
			if (currentUnit == null || unitIndex >= connectedUnits.Count) {
				lr.enabled = false;
			}
			else {
				lr.enabled = true;
				Unit u = connectedUnits[unitIndex];
				Vector3 thisOffset = offset;
				if (u == bestConnectedUnit)
					thisOffset *= .5f;

				lr.SetPosition(0, currentUnit.transform.position + thisOffset);
				lr.SetPosition(1, u.transform.position + thisOffset);

				if (u == bestConnectedUnit) {
					lr.material = targetConnectionMat;
				}
				else {
					lr.material = ColorDatabase.S.lineMaterials[identity.id];
				}
				float percentDist = 1 - Vector3.Distance(this.transform.position, u.transform.position) / maxTransferDistance;
				float sizeMultiplier = lrWidthByDistPercent.Evaluate(percentDist);
				lr.endWidth = sizeMultiplier;
			}
			unitIndex += 1;
		}
	}

	void MoveToAnotherUnit(Vector2 dir) {
		if (bestConnectedUnit) {
			if (currentUnit) {
				currentUnit.SetSelected(false);
				currentUnit.TransferEnergy(bestConnectedUnit);
			}
			bestConnectedUnit.SetSelected(true);

			currentUnit = bestConnectedUnit;
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

	IEnumerator Reload() {
		reloaded = false;
		yield return new WaitForSeconds(maxMoveBufferTime);
		reloaded = true;
	}
}
