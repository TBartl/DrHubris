using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure; // Required in C#

public class PlayerSelector : MonoBehaviour {

	//public delegate void OnArrowFinished(Vector2 direction);

	PlayerIdentity identity;	
	SpriteRenderer sr;

	public SpriteRenderer selectionSprite;
	public SpriteRenderer arrowSprite;

	public float speed;
	public float radius;

	List<Unit> selected = new List<Unit>();
	bool selecting = false;

	GamePadState padState;

	void Awake() {
		sr = this.GetComponent<SpriteRenderer>();
		identity = this.GetComponent<PlayerIdentity>();
		padState = GamePad.GetState((PlayerIndex)identity.id);
	}

	void Update() {
		padState = GamePad.GetState((PlayerIndex)identity.id);

		if (padState.Buttons.X != ButtonState.Pressed && padState.Buttons.B != ButtonState.Pressed) {
			UpdateMovement();
			arrowSprite.enabled = false;
		} else {
			arrowSprite.enabled = true;
			if (GetLeftJoystickDir().magnitude > .5f) {
				Vector2 inputDir = GetLeftJoystickDir().normalized;
				float angle = Mathf.Atan2(inputDir.y, inputDir.x) * Mathf.Rad2Deg - 90;
				arrowSprite.transform.rotation = Quaternion.Euler(0, 0, angle);
			}
		}
		if (padState.Buttons.A == ButtonState.Pressed && !selecting) {
			StartCoroutine(SelectNewGroup());
		}
		if (GetLeftJoystickDir().magnitude > .5f) {
			if (padState.Buttons.X == ButtonState.Pressed) {
				IssueMoveToSelected(GetLeftJoystickDir());
			}
			if (padState.Buttons.B == ButtonState.Pressed) {
				IssueFireToSelected(GetLeftJoystickDir());
			}
		}
	}

	IEnumerator SelectNewGroup() {
		selecting = true;
		ClearSelected();
		this.transform.localScale = Vector3.one * radius;

		while (padState.Buttons.A == ButtonState.Pressed) {
			Collider2D[] colliders = Physics2D.OverlapCircleAll(this.transform.position, radius / 2f);
			foreach (Collider2D collider in colliders) {
				Unit unit = collider.GetComponent<Unit>();
				if (unit && unit.GetID() == identity.id && !selected.Contains(unit)) {
					unit.SetSelected(true);
					selected.Add(unit);
				}
			}
			yield return new WaitForFixedUpdate();
		}
		this.transform.localScale = Vector3.one;
		selecting = false;
	}

	void UpdateMovement() {
		this.transform.position += (Vector3)GetLeftJoystickDir() * speed * Time.deltaTime;
	}

	void IssueMoveToSelected(Vector2 direction) {
		foreach (Unit unit in selected) {
			if (unit)
				unit.GetComponent<Unit>().CommandMoveInDirection(direction);
		}
	}

	void IssueFireToSelected(Vector2 direction) {
		foreach (Unit unit in selected) {
			if (unit)
				unit.GetComponent<Unit>().CommandFire(direction);
		}
	}


	void ClearSelected() {
		foreach (Unit unit in selected) {
			if (unit) {
				unit.SetSelected(false);
			}
		}
		selected = new List<Unit>();
	}

	Vector2 GetLeftJoystickDir() {
		return new Vector2(padState.ThumbSticks.Left.X, padState.ThumbSticks.Left.Y);
	}
}
