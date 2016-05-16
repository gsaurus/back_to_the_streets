using UnityEngine;
using System.Collections;

public class SettingsButtonBehaviour : MonoBehaviour {

	public GameObject settingsPanel;

	public void OnButtonClicked(){
		settingsPanel.GetComponent<Animator>().Play("OpenSettings");
		gameObject.SetActive(false);
	}

}
