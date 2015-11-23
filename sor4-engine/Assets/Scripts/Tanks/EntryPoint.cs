using UnityEngine;
using System.Collections;
using RetroBread;


public class EntryPoint : MonoBehaviour
{
	// TODO: public options to setup the world on the restart method


	// Use this for initialization
	void Start(){
		RetroBread.Debug.Instance = new UnityDebug();
		TanksVCFactories.RegisterFactories();
		Restart();
	}
	
	void LateUpdate(){
		StateManager.Instance.Update(Time.deltaTime);
	}

	public void Restart(){
		StateManagerSetup setup = new StateManagerSetup(new WorldModel());
		StateManager.Instance.Setup(setup);
	}
}

