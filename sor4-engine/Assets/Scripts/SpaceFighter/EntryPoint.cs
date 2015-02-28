using UnityEngine;
using System.Collections;


public class EntryPoint : MonoBehaviour
{

	// Use this for initialization
	void Start (){
		StateManagerSetup setup = new StateManagerSetup(new WorldModel());
		StateManager.Instance.Setup(setup);
	}
	
	void LateUpdate(){
		StateManager.Instance.Update(Time.deltaTime);
	}
}

