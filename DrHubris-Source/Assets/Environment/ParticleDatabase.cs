using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDatabase : MonoBehaviour {

	public static ParticleDatabase S;

	public List<GameObject> blood;

	void Awake() {
		S = this;
	}
}
