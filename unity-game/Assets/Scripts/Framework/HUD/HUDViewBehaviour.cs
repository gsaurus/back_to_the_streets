using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace RetroBread{

	// HUDView is a MonoBehaviour because it interacts directly with the HUD game object
	public class HUDViewBehaviour : MonoBehaviour{

		// The logical information about this hud element
		public Storage.HUDObject hudObjectData;

		// Events defined at the HUD Editor
		public List<GenericEvent<HUDViewBehaviour>> events;

		// One coroutine per animator's variable being updated
		private Dictionary<string, Coroutine> animatorVariableCoroutines = new Dictionary<string, Coroutine>();



		public void ScheduleVariableUpdate(string paramName, float newValue, float delay, float duration){
			if (animatorVariableCoroutines.ContainsKey(paramName)) {
				// stop current update
				StopCoroutine(animatorVariableCoroutines[paramName]);
			}
			animatorVariableCoroutines[paramName] = StartCoroutine(VariableUpdate(paramName, newValue, delay, duration));
		}

		private IEnumerator VariableUpdate(string paramName, float newValue, float delay, float duration){
			Animator animator = GetComponent<Animator>();
			if (animator == null) yield break;
			if (delay > 0) {
				yield return new WaitForSeconds(delay);
			}
			if (duration > 0) {
				float oldValue = animator.GetFloat(paramName);
				float elapsedTime = 0;
				while (elapsedTime < duration) {
					yield return new WaitForEndOfFrame();
					elapsedTime += Time.deltaTime;
					animator.SetFloat(paramName, Mathf.Lerp(oldValue, newValue, elapsedTime / duration));
				}
			}
			animator.SetFloat(paramName, newValue);
			animatorVariableCoroutines.Remove(paramName);
		}
		
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

