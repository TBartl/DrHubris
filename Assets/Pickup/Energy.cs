using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour {
	public int amount = 1;
	public GameObject pickupParticles;

	public void Collect() {
		Destroy(this.gameObject);
		Instantiate(pickupParticles, this.transform.position, Quaternion.identity);
	}

}
