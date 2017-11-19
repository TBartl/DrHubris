using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRumble : MonoBehaviour {

	public AnimationCurve burnoff;
	public AnimationCurve shake;

	float rumble = 0;

	public void Add(int add) {
		rumble += add;
	}

	void Update() {
		if (rumble > 0) {
			this.transform.localPosition = Random.insideUnitCircle * shake.Evaluate(rumble);
			rumble -= burnoff.Evaluate(rumble) * Time.deltaTime;
		}
	}
}
