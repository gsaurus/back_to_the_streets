using UnityEngine;
using System.Collections;

public class FireAndForgetBehaviour : MonoBehaviour {

	public float lifetime;

	void Start(){
		GameObject.Destroy(this.gameObject, lifetime);
	}

}
