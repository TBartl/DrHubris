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
	bool bWasPressed;

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

	public GameObject arrowPrefab;
	List<SpriteRenderer> arrows = new List<SpriteRenderer>();
	public int arrowPoolSize = 10;
	public float arrowSeparation;
	public float arrowSpeed;

	CameraRumble camRumble;
	int lastUnitCount = 2;

	void Awake() {
		identity = this.GetComponent<PlayerIdentity>();
		padState = GamePad.GetState((PlayerIndex)identity.id);
		camRumble = this.GetComponentInChildren<CameraRumble>();
	}

	void Start() {
		cameraCo = StartCoroutine(FollowUnit());
		//currentUnit = Unit.allUnits[identity.id][0];
		if (currentUnit)
			currentUnit.SetSelected(true);

		for (int i = 0; i < connectionLinePoolSize; i++) {
			connectionLines.Add(Instantiate(connectionLinePrefab, this.transform).GetComponent<LineRenderer>());
		}
		for (int i = 0; i < arrowPoolSize; i++) {
			arrows.Add(Instantiate(arrowPrefab, this.transform).GetComponent<SpriteRenderer>());
		}
	}

	void Update() {
		padState = GamePad.GetState((PlayerIndex)identity.id);

		Vector2 leftJoystick = GetLeftJoystickDir();

		UpdateBestUnit(leftJoystick.normalized);

		bool aIsPressed = padState.Buttons.A == ButtonState.Pressed;
		bool bIsPressed = padState.Buttons.B == ButtonState.Pressed;

		if (aIsPressed && !aWasPressed) {
			moveBufferTime = maxMoveBufferTime;
		}
		if ((moveBufferTime > 0 || currentUnit == null) && reloaded) {
			MoveToAnotherUnit(leftJoystick.normalized);
			StartCoroutine(Reload());
			moveBufferTime = 0;
		}
		aWasPressed = aIsPressed;
		moveBufferTime -= Time.deltaTime;

		DrawConnections();
		DrawArrows();
		UpdateRumble();

		if (!currentUnit)
			return;

		if (leftJoystick.magnitude > .5f) {
			lastLeftStickMovement = leftJoystick;
			currentUnit.CommandMoveInDirection(leftJoystick);

			if (TutorialManager.S)
				TutorialManager.S.OnMoved(identity.id);
		}
		if (bIsPressed && !bWasPressed) {
			currentUnit.CommandFire(lastLeftStickMovement);
		}
		bWasPressed = bIsPressed;
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

	void DrawArrows() {

		if (currentUnit == null || bestConnectedUnit == null) {
			foreach (SpriteRenderer arrow in arrows) {
				arrow.enabled = false;
			}
			return;
		}

		float totalDistBetween = Vector3.Distance(currentUnit.transform.position, bestConnectedUnit.transform.position);
		Vector3 diff = (bestConnectedUnit.transform.position - currentUnit.transform.position).normalized;
		for (int i = 0; i < arrows.Count; i++) {
			SpriteRenderer arrow = arrows[i];
			float dist = (i + (Time.time * arrowSpeed) % 1) * arrowSeparation;
			if (dist < totalDistBetween) {
				arrow.enabled = true;
				arrow.transform.position = currentUnit.transform.position + diff * dist;
				arrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg - 90);
				arrow.color = ColorDatabase.S.lineMaterials[identity.id].color;
				arrow.transform.localScale = Vector3.one * lrWidthByDistPercent.Evaluate(1-dist / maxTransferDistance);
			}
			else {
				arrow.enabled = false;
			}
		}
	}

	void UpdateRumble() {
		int newCount = 0;
		for (int i = 0; i < PlayerIdentity.numPlayersReal; i++) {
			newCount += Unit.allUnits[i].Count;
		}
		camRumble.Add(Mathf.Abs(newCount - lastUnitCount));
		lastUnitCount = newCount;
	}

	void MoveToAnotherUnit(Vector2 dir) {
		if (bestConnectedUnit) {
			if (currentUnit) {
				currentUnit.SetSelected(false);
				currentUnit.TransferEnergy(bestConnectedUnit);

				if (TutorialManager.S)
					TutorialManager.S.OnTransferred(identity.id);
			}
			bestConnectedUnit.SetSelected(true);

			currentUnit = bestConnectedUnit;
			StopCoroutine(cameraCo);
			cameraCo = StartCoroutine(MoveToNewUnit());
			AudioManager.S.OnTransfer();
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
