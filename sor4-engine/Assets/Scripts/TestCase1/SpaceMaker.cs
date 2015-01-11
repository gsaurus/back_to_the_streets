using UnityEngine;
using System.Collections;


public class SpaceMaker : MonoBehaviour
{

	// Use this for initialization
	void Start (){
		StateManagerSetup setup = new StateManagerSetup(new SpaceModel());
		StateManager.Instance.Setup(setup);
	}
	
	void LateUpdate(){
		StateManager.Instance.Update(Time.deltaTime);
	}
}

