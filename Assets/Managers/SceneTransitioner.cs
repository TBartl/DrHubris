using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SceneTransitioner : MonoBehaviour {

	public static SceneTransitioner S;

	public RectTransform top;
	public RectTransform bottom;

	public float transitionTime = 5f;
	public AnimationCurve curve;

	bool changingScene = false;

	void Awake() {
		S = this;
	}

	void Start() {
		StartCoroutine(Open());
	}

	public void NextScene() {
		StartCoroutine(CloseThenChangeScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings));
	}
	public void ResetScene() {
		StartCoroutine(CloseThenChangeScene(SceneManager.GetActiveScene().buildIndex));
	}

	IEnumerator Open() {
		for (float t = 0; t < transitionTime; t += Time.deltaTime) {
			float p = t / transitionTime;
			p = curve.Evaluate(p);
			top.anchoredPosition = Vector3.Lerp(Vector3.zero, Vector3.up * top.rect.height * 2, p);
			bottom.anchoredPosition = Vector3.Lerp(Vector3.zero, Vector3.down * bottom.rect.height * 2, p);
			yield return null;
		}
	}

	IEnumerator CloseThenChangeScene(int scene) {
		if (changingScene)
			yield break;
		changingScene = true;
		for (float t = 0; t < transitionTime; t += Time.deltaTime) {
			float p = t / transitionTime;
			p = curve.Evaluate(p);
			p = 1 - p;
			top.anchoredPosition = Vector3.Lerp(Vector3.zero, Vector3.up * top.rect.height * 2, p);
			bottom.anchoredPosition = Vector3.Lerp(Vector3.zero, Vector3.down * bottom.rect.height * 2, p);
			yield return null;
		}
		SceneManager.LoadScene(scene);
	}
}
