using UnityEngine;
using System.Collections;

public class LostHelmetBehaviour : MonoBehaviour {


	void Start(){
		Rigidbody body = this.gameObject.GetComponent<Rigidbody>();
		body.AddForce(new Vector3(Random.Range(-200f, 200f), Random.Range(250f, 350f), 0f));
		body.AddTorque(new Vector3(0f,1f,Random.Range(-200, -120f)));
		StartCoroutine("FireAndForget");
	}

	IEnumerator FireAndForget(){
		yield return new WaitForSeconds(3);
		GameObject.Destroy(this.gameObject);
	}

}
