using UnityEngine;
using System.Collections;
using RetroBread;


public class EntryPoint : MonoBehaviour
{

	// Use this for initialization
	void Start(){
		RetroBread.Debug.Instance = new UnityDebug();
		ShooterVCFactories.RegisterFactories();
	}
	
	void LateUpdate(){
		StateManager.Instance.Update(Time.deltaTime);
	}
		
}

