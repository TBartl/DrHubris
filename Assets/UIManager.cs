using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public List<Text> scoreTexts;

	void Update() {
		for (int i = 0; i < scoreTexts.Count; i++) {
			scoreTexts[i].text = Unit.allUnits[i].Count.ToString();
		}
	}
}
