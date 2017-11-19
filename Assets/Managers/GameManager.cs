using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

public class GameManager : MonoBehaviour {

	bool restarting = false;
	public List<Sprite> winGroundSprites;

	void Update() {
		int numRemaining = 0;
		for (int i = 0; i < Unit.allUnits.Count; i++) {
			if (Unit.allUnits[i].Count > 0)
				numRemaining++;
		}
		if (!restarting && numRemaining <= 1) {
			StartCoroutine(Restart());
		}
		for (int i = 0; i < PlayerIdentity.numPlayers; i++) {
			if (GamePad.GetState((PlayerIndex)i).DPad.Down == ButtonState.Pressed) {
				SceneManager.LoadScene(0);
			}
			if (GamePad.GetState((PlayerIndex)i).DPad.Left == ButtonState.Pressed) {
				TutorialManager.disableOnAwake = false;
			}
			if (GamePad.GetState((PlayerIndex)i).DPad.Right == ButtonState.Pressed) {
				TutorialManager.disableOnAwake = true;
			}
		}
	}

	IEnumerator Restart() {
		restarting = true;
		AudioManager.S.SetDone();
		for (int i = 0; i < Unit.allUnits.Count; i++) {
			if (Unit.allUnits[i].Count > 0)
				GameObject.Find("Ground").GetComponent<SpriteRenderer>().sprite = winGroundSprites[i];
		}
		yield return new WaitForSeconds(2);
		SceneManager.LoadScene(0);
	}
}
