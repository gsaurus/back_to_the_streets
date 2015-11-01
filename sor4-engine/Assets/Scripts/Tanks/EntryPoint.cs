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
		int[] theMap = new int[]{
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 9, 9, 0, 0, 3, 3, 3, 0, 2, 0, 0, 0,
			0, 9, 9, 0, 0, 3, 3, 3, 0, 0, 0, 0, 0,
			0, 9, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 9, 9, 1, 1, 2, 0, 2, 3, 1, 3, 3, 3,
			0, 9, 9, 9, 1, 2, 0, 2, 3, 3, 3, 3, 3,
			0, 9, 9, 1, 1, 2, 2, 2, 0, 0, 0, 3, 3,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 3,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 9,
			9, 2, 2, 0, 0, 0, 0, 1, 1, 0, 0, 9, 9,
			0, 0, 2, 9, 0, 0, 0, 1, 1, 0, 0, 2, 2,
			0, 0, 9, 9, 0, 3, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 9, 9, 0, 0, 0, 0, 0, 0, 0, 0, 0
		};

		StateManagerSetup setup = new StateManagerSetup(new WorldModel(theMap));
		StateManager.Instance.Setup(setup);
	}
}

