using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TutorialState {

	public Transform electricSpawnPosition;
	public List<Transform> aiSpawnPositions;
	public SpriteRenderer instructions;
	public GameObject firstPickup;

	public bool moved;
	public bool gathered;
	public bool cloned;
	public bool transferred;
	public int remainingClones;
}

public class TutorialManager : MonoBehaviour {

	public static TutorialManager S;
	public static bool disableOnAwake = false;

	public List<TutorialState> states;

	public Sprite moveInstruction;
	public Sprite gatherInstruction;
	public Sprite cloneInstruction;
	public Sprite transferInstruction;
	public List<Sprite> killInstruction;
	public Sprite waitInstruction;

	public List<GameObject> walls;
	public GameObject pickup;

	public Sprite check;
	public float checkTime;

	PickupSpawner spawner;

	int tutorialsRemaining;

	void Awake() {
		if (disableOnAwake) {
			gameObject.SetActive(false);
			return;
		}

		S = this;
		for (int i = 0; i < PlayerIdentity.numPlayersReal; i++) {
			StartCoroutine(RunTutorial(i));
			tutorialsRemaining += 1;
		}
		spawner = FindObjectOfType<PickupSpawner>();
		spawner.enabled = false;
	}

	void Start() {
		StartCoroutine(WaitForTutorialsToEnd());
	}

	IEnumerator WaitForTutorialsToEnd() {
		AudioManager.S.SetTutorial();
		while (tutorialsRemaining > 0)
			yield return null;

		spawner.enabled = true;
		foreach (GameObject wall in walls) {
			Destroy(wall);
		}
		foreach (TutorialState state in states) {
			//Instantiate(spawner.pickup, state.electricSpawnPosition.position, Quaternion.identity);
			state.instructions.sprite = null;
			state.firstPickup.SetActive(true);
		}
		AudioManager.S.SetGameplay();
	}

	IEnumerator RunTutorial(int i) {
		states[i].firstPickup.SetActive(false);

		states[i].instructions.sprite = moveInstruction;
		while (!states[i].moved)
			yield return null;
		states[i].instructions.sprite = check;
		yield return new WaitForSeconds(checkTime);

		GameObject lightning = Instantiate(pickup, states[i].electricSpawnPosition.position, Quaternion.identity);
		CallbackOnDestroy callback = lightning.AddComponent<CallbackOnDestroy>();
		callback.callback = () => { OnGather(i); };
		states[i].instructions.sprite = gatherInstruction;
		while (!states[i].gathered)
			yield return null;
		states[i].instructions.sprite = check;
		yield return new WaitForSeconds(checkTime);


		states[i].instructions.sprite = cloneInstruction;
		while (!states[i].cloned)
			yield return null;
		states[i].instructions.sprite = check;
		yield return new WaitForSeconds(checkTime);

		states[i].instructions.sprite = transferInstruction;
		while (!states[i].transferred)
			yield return null;
		states[i].instructions.sprite = check;
		yield return new WaitForSeconds(checkTime);

		states[i].instructions.sprite = killInstruction[i];
		foreach (Transform spawnLoc in states[i].aiSpawnPositions) {
			GameObject g = Instantiate(CloneDatabase.S.unitPrefab, spawnLoc.transform.position, Quaternion.identity);
			g.GetComponent<PlayerIdentity>().id = PlayerIdentity.numPlayers - 1; // Bot
			CallbackOnDestroy callback2 = g.AddComponent<CallbackOnDestroy>();
			callback2.callback = () => { OnCloneDie(i); };
			states[i].remainingClones += 1;
		}
		while (states[i].remainingClones > 0)
			yield return null;
		states[i].instructions.sprite = check;
		yield return new WaitForSeconds(checkTime);


		states[i].instructions.sprite = waitInstruction;
		tutorialsRemaining -= 1;
	}

	public void OnMoved(int i) {
		states[i].moved = true;
	}

	public void OnGather(int i) {
		states[i].gathered = true;
	}

	public void OnClone(int i) {
		states[i].cloned = true;
	}

	public void OnTransferred(int i) {
		states[i].transferred = true;
	}

	public void OnCloneDie(int i) {
		states[i].remainingClones -= 1;
	}

	public static bool Running() {
		if (S == null)
			return false;
		return S.tutorialsRemaining > 0;
	}
}
