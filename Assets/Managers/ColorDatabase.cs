using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorDatabase : MonoBehaviour {
	public static ColorDatabase S;

	public List<Material> lineMaterials;

	void Awake() {
		S = this;
	}
}
