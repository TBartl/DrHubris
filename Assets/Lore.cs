using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using UnityEngine.SceneManagement;

public class Lore : MonoBehaviour {
	public List<GameObject> backgrounds;

	List<bool> wasPressed = new List<bool>();

	public float moveTime;
	public AnimationCurve moveCurve;
	AudioSource audioSource;

	void Start() {
		audioSource = this.GetComponent<AudioSource>();
		for (int i = 0; i < PlayerIdentity.numPlayersReal; i++) {
			wasPressed.Add(true);
		}
		StartCoroutine(Run());
	}

	IEnumerator Run() {
		for (int i = 0; i < backgrounds.Count; i++) {
			bool cont = false;
			while (!cont) {
				for (int j = 0; j < PlayerIdentity.numPlayersReal; j++) {
					bool isPressed = GamePad.GetState((PlayerIndex)j).Buttons.A == ButtonState.Pressed;
					if (isPressed && !wasPressed[j]) {
						cont = true;
					}
					wasPressed[j] = isPressed;
				}
				yield return null;
			}
			audioSource.PlayOneShot(audioSource.clip);
			yield return null;
			if (i + 1 < backgrounds.Count) {
				StartCoroutine(MoveCamToNextBackground(backgrounds[i+1]));
			}
		}
		TutorialManager.disableOnAwake = false;
		SceneTransitioner.S.NextScene();
	}

	IEnumerator MoveCamToNextBackground(GameObject next) {
		Vector3 from = Camera.main.transform.position;
		Vector3 to = next.transform.position;
		to.z = Camera.main.transform.position.z;

		for (float t = 0; t < moveTime; t += Time.deltaTime) {
			float p = t / moveTime;
			Camera.main.transform.position = Vector3.Lerp(from, to, moveCurve.Evaluate(p));
			yield return null;
		}
	}
}
