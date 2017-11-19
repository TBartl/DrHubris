using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using XInputDotNetPure;

public class GameManager : MonoBehaviour {

	bool restarting = false;
	public List<Texture2D> winGroundTextures;
	public List<Texture2D> winWallTextures;

	public MeshRenderer ground;
	public MeshRenderer wall;
	public int restartTime = 10;

	public GameObject endPanel;
	public Text endPanelText;

	void Start() {
		endPanel.SetActive(false);
	}

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
			GamePadState pad = GamePad.GetState((PlayerIndex)i);
			if (pad.DPad.Left == ButtonState.Pressed) {
				TutorialManager.disableOnAwake = false;
				SceneTransitioner.S.ResetScene();
			}
			if (pad.DPad.Right == ButtonState.Pressed) {
				TutorialManager.disableOnAwake = true;
				SceneTransitioner.S.ResetScene();
			}
		}
	}

	IEnumerator Restart() {
		restarting = true;
		int winner = -1;
		for (int i = 0; i < Unit.allUnits.Count; i++) {
			if (Unit.allUnits[i].Count > 0) {
				ground.material.mainTexture = winGroundTextures[i];
				wall.material.mainTexture = winWallTextures[i];
				winner = i;
			}
		}
		endPanel.SetActive(true);
		string endText = "";
		if (winner == -1)
			endText += "<b>Draw!</b>\n";
		else if (winner == 0)
			endText += "<b>Blue Win!</b>\n";
		else if (winner == 1)
			endText += "<b>Red Win!</b>\n";

		endText += "Quick Restart:        \n";
		endText += "Returning to menu in\n";

		for (float t = restartTime; t > 0; t -= Time.deltaTime) {
			for (int i = 0; i < PlayerIdentity.numPlayers; i++) {
				if (GamePad.GetState((PlayerIndex)i).Buttons.X == ButtonState.Pressed) {
					SceneTransitioner.S.ResetScene();
				}
			}
			endPanelText.text = endText + Mathf.CeilToInt(t).ToString() + "...";
			yield return null;
		}
		SceneTransitioner.S.NextScene();
	}
}
