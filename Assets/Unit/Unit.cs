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

	public GameObject energyTransferPrefab;
	public float energyTransferRate;

	public float speed;
	public float fireAngle = 15;

	public float maxInvulnTime = 1f;
	public float invulnSpeedIncrease = 2f;
	bool invuln = false;

	Vector2 direction;
	[HideInInspector] public int energy = 0;

	bool selected = false;
	public float selectedSpeedIncrease = 2f;

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
		this.selected = selected;
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

		if (energy > 0) {
			if (TutorialManager.S)
				TutorialManager.S.OnClone(identity.id);
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
		Energy e = collision.gameObject.GetComponent<Energy>();
		if (e != null) {
			Destroy(collision.gameObject);
			UpdateEnergy(energy + e.amount);
		}
	}

	float GetSpeed() {
		float returnSpeed = speed;
		if (invuln)
			returnSpeed += invulnSpeedIncrease;
		if (selected)
			returnSpeed += selectedSpeedIncrease;
		return returnSpeed;
	}

	public void UpdateEnergy(int amount) {
		energy = amount;
		energyText.text = energy.ToString();
		if (energy >= 10)
			energyText.fontSize = 72;
		else
			energyText.fontSize = 90;
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

	public void TransferEnergy(Unit to) {
		StartCoroutine(RunTransferEnergy(to));
	}

	IEnumerator RunTransferEnergy(Unit to) {
		if (this.energy == 0)
			yield return new WaitForSeconds(energyTransferRate * 2);
		while (this.energy > 0 && to) {
			GameObject g = Instantiate(energyTransferPrefab);
			this.UpdateEnergy(this.energy - 1);
			g.GetComponent<EnergyTransfer>().Transfer(this, to);
			yield return new WaitForSeconds(energyTransferRate);
		}
	}

}
