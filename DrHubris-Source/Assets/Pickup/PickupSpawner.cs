using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour {

	Collider2D coll;

	public List<GameObject> pickups;

	public float spawnTime = 1f;


	void Awake() {
		coll = this.GetComponent<BoxCollider2D>();
	}

	void Start() {
		StartCoroutine(SpawnPickups());
	}
	IEnumerator SpawnPickups() {
		while (true) {
			foreach (GameObject pickup in pickups) {
				Bounds bounds = coll.bounds;
				Vector2 pos = this.transform.position;
				pos.x += Random.Range(-bounds.extents.x, bounds.extents.x);
				pos.y += Random.Range(-bounds.extents.y, bounds.extents.y);
				Instantiate(pickup, pos, Quaternion.identity);
				Instantiate(pickup, -pos, Quaternion.identity);
				yield return new WaitForSeconds(spawnTime);
			}			
		}
	}
}
