using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;

public class TitleScript : MonoBehaviour {

	public List<GameObject> checks;
	public AudioSource enter;

	void Start() {
		foreach (GameObject check in checks) {
			check.SetActive(false);
		}
	}

	// Update is called once per frame
	void Update() {
		bool allInactive = true;
		for (int i = 0; i < checks.Count; i++) {
			if (GamePad.GetState((PlayerIndex)i).Buttons.A == ButtonState.Pressed && checks[i].activeInHierarchy == false) {
				checks[i].SetActive(true);
				enter.PlayOneShot(enter.clip);
			}
			if (checks[i].activeInHierarchy == false) {
				allInactive = false;
			}
		}
		if (allInactive) {
			Debug.Log("LETS GO");
		}
	}
}
