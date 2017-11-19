using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CallbackOnDestroy : MonoBehaviour {

	public UnityAction callback;

	void OnDestroy() {
		if (callback != null)
			callback();
	}
}
