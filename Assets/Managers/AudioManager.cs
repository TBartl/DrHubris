using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public static AudioManager S;

	public AudioClip tutorial;
	public AudioClip gameplay;

	public AudioSource music;

	void Awake() {
		S = this;
	}

	public void SetTutorial() {
		music.clip = tutorial;
		music.Play();
	}
	public void SetGameplay() {
		StartCoroutine(WaitThenSwitchClip(gameplay));
	}
	public void SetDone() {
		StartCoroutine(WaitThenSwitchClip(tutorial));
	}

	IEnumerator WaitThenSwitchClip(AudioClip newClip) {
		float remainingTime = music.clip.length - music.time;
		yield return new WaitForSeconds(remainingTime);
		music.clip = newClip;
		music.Play();
	}

}
