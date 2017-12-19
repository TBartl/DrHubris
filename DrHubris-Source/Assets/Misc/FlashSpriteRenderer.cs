using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashSpriteRenderer : MonoBehaviour {
	SpriteRenderer sr;
	public float flashTime = .5f;

	void Start() {
		sr = this.GetComponent<SpriteRenderer>();
		StartCoroutine(Flash());
	}

	IEnumerator Flash() {
		while (true) {
			for (float t = 0; t < flashTime * 2; t += Time.deltaTime) {
				float p = t / (flashTime * 2);
				p = Mathf.Sin(p * 2 * Mathf.PI) * .5f + .5f;
				sr.color = new Color(1, 1, 1, p);
				yield return null;
			}
			
		}
	}

}
