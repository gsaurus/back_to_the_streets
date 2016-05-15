using UnityEngine;
using System.Collections;

public class CreditsScript : MonoBehaviour {

	static int creditsSeen = 4;


	void OnEnable () {
		if (creditsSeen % 5 == 0) {
			GetComponent<Animator>().Play("credits_animation");
		}
		++creditsSeen;
	}

}
