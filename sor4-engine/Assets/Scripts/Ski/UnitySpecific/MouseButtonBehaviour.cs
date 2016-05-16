using UnityEngine;
using System.Collections;

public class MouseButtonBehaviour : MonoBehaviour {

	public UnityEngine.UI.Text buttonText;

	// Use this for initialization
	void Start() {
		UpdateButtonLabel();
	}


	public void OnButtonClick(){
		RetroBread.MouseInputSource.useMouseAngle = !RetroBread.MouseInputSource.useMouseAngle;
		UpdateButtonLabel();
	}


	void UpdateButtonLabel(){
		if (RetroBread.MouseInputSource.useMouseAngle) {
			buttonText.text = "Classic";
		} else {
			buttonText.text = "Free Style";
		}
	}

}
