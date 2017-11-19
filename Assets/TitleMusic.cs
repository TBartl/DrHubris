using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMusic : MonoBehaviour {

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this.gameObject);
		SceneManager.activeSceneChanged += OnSceneChanged;
	}

	void OnSceneChanged(Scene from, Scene to) {
		if (to.buildIndex > 1) {
			Destroy(this.gameObject);
		}

	}
}
