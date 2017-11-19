using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

public class GameManager : MonoBehaviour {

	bool restarting = false;
	public List<Texture2D> winGroundTextures;
	public List<Texture2D> winWallTextures;

	public MeshRenderer ground;
	public MeshRenderer wall;

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
				SceneManager.LoadScene(2);
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
			if (Unit.allUnits[i].Count > 0) {
				ground.material.mainTexture = winGroundTextures[i];
				wall.material.mainTexture = winWallTextures[i];
			}
		}
		yield return new WaitForSeconds(2);
		SceneManager.LoadScene(0);
	}
}
