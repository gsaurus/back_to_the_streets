using UnityEngine;
using System.Collections.Generic;


namespace RetroBread{

	// HUDView is a MonoBehaviour because it interacts directly with the HUD game object
	public class HUDViewBehaviour : MonoBehaviour{

		public List<GenericEvent<HUDViewBehaviour>> events;


		// TODO: list of condition-event triggers
		
		// Update is called once per frame
		void Update(){
			ProcessGeneralEvents();
		}


		// Process general animation events
		private void ProcessGeneralEvents(){
			foreach (GenericEvent<HUDViewBehaviour> e in events){
				e.Evaluate(this);
			}
		}
	}

}

