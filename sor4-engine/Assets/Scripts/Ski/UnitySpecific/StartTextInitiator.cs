using UnityEngine;
using System.Collections;

public class StartTextInitiator : MonoBehaviour {

	
	void OnEnable(){
		Animator animator = GetComponent<Animator>();
		animator.Play("StartTextAnimation");
		AudioSource audio = GetComponent<AudioSource>();
		audio.Play();
	}
}
