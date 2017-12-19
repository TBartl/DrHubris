using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyTransfer : MonoBehaviour {

	public float transferTime = 3f;
	public AnimationCurve transferCurve;

	public void Transfer(Unit from, Unit to) {
		StartCoroutine(RunTransfer(from, to));
	}

	IEnumerator RunTransfer(Unit from, Unit to) {
		Vector3 fromPos = from.transform.position;
		float t = 0;
		while (to && t < transferTime) {
			if (from)
				fromPos = from.transform.position;
			Vector3 toPos = to.transform.position;
			float p = t / transferTime;
			this.transform.position = Vector3.Lerp(fromPos, toPos, transferCurve.Evaluate(p));
			t += Time.deltaTime;
			yield return null;
		}
		if (to)
			to.UpdateEnergy(to.energy + 1);
		Destroy(this.gameObject);
	}
}
