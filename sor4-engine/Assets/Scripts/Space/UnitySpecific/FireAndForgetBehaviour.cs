using UnityEngine;
using System.Collections;

public class FireAndForgetBehaviour : MonoBehaviour {

	public float lifetime;
	
	void Start(){
		StartCoroutine("FireAndForget");
	}

	IEnumerator FireAndForget(){
		yield return new WaitForSeconds(lifetime);
		GameObject.Destroy(this.gameObject);
	}

}
