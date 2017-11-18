using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneDatabase : MonoBehaviour {

	public static CloneDatabase S;

	public GameObject unitPrefab;

	void Awake() {
		S = this;
	}
}
