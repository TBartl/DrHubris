using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

	public static List<List<Unit>> allUnits;

	Rigidbody2D rigid;
	PlayerIdentity identity;

	public SpriteRenderer playerSprite;
	public SpriteRenderer selectionSprite;
	public SpriteRenderer invulnSprite;
	public Text energyText;

	public float speed;
	public float fireAngle = 15;

	public float maxInvulnTime = 1f;
	public float invulnSpeedMultiplier = 2f;
	bool invuln = false;

	Vector2 direction;
	int energy = 0;

	void Awake() {
		if (allUnits == null) {
			allUnits = new List<List<Unit>>();
			for (int i = 0; i < PlayerIdentity.numPlayers; i++) {
				allUnits.Add(new List<Unit>());
			}
		}
		rigid = this.GetComponent<Rigidbody2D>();
		identity = this.GetComponent<PlayerIdentity>();
		
	}

	void Start() {
		playerSprite.sprite = SpriteDatabase.S.playerSprites[identity.id];
		selectionSprite.gameObject.SetActive(false);
		invulnSprite.gameObject.SetActive(false);
		UpdateEnergy(0);
		gameObject.layer += identity.id;

		allUnits[identity.id].Add(this);
	}

	void OnDestroy() {
		allUnits[identity.id].Remove(this);
	}

	void Update() {
		this.rigid.velocity = (Vector3)direction * GetSpeed();
		playerSprite.transform.rotation = Quaternion.Euler(0,0,Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
	}

	public void SetSelected(bool selected) {
		selectionSprite.gameObject.SetActive(selected);
	}

	public void CommandMoveInDirection(Vector2 direction) {
		this.direction = direction;
	}

	public void CommandFire(Vector2 direction) {
		float middle = (energy - 1) / 2f;
		for (int i = 0; i < energy; i++) {
			Unit clone = Instantiate(CloneDatabase.S.unitPrefab, this.transform.position, Quaternion.identity).GetComponent<Unit>();
			clone.identity.id = identity.id;
			clone.direction = RotateVec2(direction, (i - middle) * fireAngle);
			clone.StartInvuln();
		}
		UpdateEnergy(0);
	}

	Vector2 RotateVec2(Vector2 vec, float degrees) {
		return Quaternion.Euler(0, 0, degrees) * vec;
	}

	void OnCollisionEnter2D(Collision2D collision) {
		if (collision.gameObject.tag == "Unit")
		{
			Unit other = collision.gameObject.GetComponent<Unit>();
			if (!invuln && other.GetID() != this.GetID())
				Destroy(this.gameObject);
			if (invuln && other.invuln)
				direction = Vector2.Reflect(direction, collision.contacts[0].normal).normalized;

		} else if (collision.gameObject.tag == "Wall") {
			direction = Vector2.Reflect(direction, collision.contacts[0].normal).normalized;
		}
	}

	void OnTriggerEnter2D(Collider2D collision) {
		if (collision.tag == "Pickup") {
			Destroy(collision.gameObject);
			UpdateEnergy(energy + 1);
		}
	}

	float GetSpeed() {
		float returnSpeed = speed;
		if (invuln)
			returnSpeed *= invulnSpeedMultiplier;
		return returnSpeed;
	}

	void UpdateEnergy(int amount) {
		energy = amount;
		energyText.text = energy.ToString();
	}

	public int GetID() {
		return identity.id;
	}

	public void StartInvuln() {
		StartCoroutine(Invuln());
	}

	IEnumerator Invuln() {
		invuln = true;
		for (float t = 0; t < maxInvulnTime; t += Time.deltaTime) {
			invulnSprite.gameObject.SetActive(true);
			yield return null;
		}
		invuln = false;
		invulnSprite.gameObject.SetActive(false);
	}

}
