using UnityEngine;
using System.Collections;

public class FlagCollisionEffect : MonoBehaviour {

	
	void OnEnable(){
		StartCoroutine(AnimateCollision());
	}

	IEnumerator AnimateCollision(){
		
		MeshRenderer quadRenderer = GetComponent<MeshRenderer>();

		Color originalColor = quadRenderer.material.color;
		originalColor.a = 0.0f;
		Color targetColor = new Color(originalColor.r* 1.5f, originalColor.g* 1.5f, originalColor.b* 1.5f, 1.0f);

		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / 0.05f){
			quadRenderer.material.color = Color.Lerp(originalColor, targetColor, t);
			yield return null;
		}
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / 0.05f){
			quadRenderer.material.color = Color.Lerp(targetColor, originalColor, t);
			yield return null;
		}
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / 0.05f){
			quadRenderer.material.color = Color.Lerp(originalColor, targetColor, t);
			yield return null;
		}
		for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / 0.3f){
			quadRenderer.material.color = Color.Lerp(targetColor, originalColor, t);
			yield return null;
		}

		quadRenderer.material.color = originalColor;
		enabled = false;
	}

}
