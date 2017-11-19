using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour {
	
	Bounds bounds;

	public GameObject pickup;

	public float spawnTime = 1f;


	void Awake() {
		Collider2D coll = this.GetComponent<BoxCollider2D>();
		bounds = coll.bounds;
	}

	void Start() {
		StartCoroutine(SpawnPickups());
	}
	IEnumerator SpawnPickups() {
		while (true) {
			Vector2 pos = this.transform.position;
			pos.x += Random.Range(-bounds.extents.x, bounds.extents.x);
			pos.y += Random.Range(-bounds.extents.y, bounds.extents.y);
			Instantiate(pickup, pos, Quaternion.identity);
			Instantiate(pickup, -pos, Quaternion.identity);
			yield return new WaitForSeconds(spawnTime);
		}
	}
}
