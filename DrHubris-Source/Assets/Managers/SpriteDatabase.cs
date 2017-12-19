using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDatabase : MonoBehaviour {

	public static SpriteDatabase S;

	public List<Sprite> playerSprites;

	void Awake() {
		S = this;
	}
}
