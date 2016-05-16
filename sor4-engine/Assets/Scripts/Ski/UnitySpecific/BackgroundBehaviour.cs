using UnityEngine;
using System.Collections;

public class BackgroundBehaviour : MonoBehaviour {

	public GameObject settingsPanel;
	public GameObject settingsButton;


	// Use this for initialization
	void OnEnable() {
		settingsButton.SetActive(true);
		settingsPanel.GetComponent<Animator>().Play("SettingsInactive");
	}

}
