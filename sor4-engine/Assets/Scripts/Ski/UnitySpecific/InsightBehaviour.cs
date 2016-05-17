using UnityEngine;
using System.Collections;

public class InsightBehaviour : MonoBehaviour {

	float minValueClassic = 1.2f;
	float maxValueClassic = 20.0f;

	float minValueFree = 20.0f;
	float maxValueFree = 140.0f;

//	float minValueFree = 0.8f;
//	float maxValueFree = 2.5f;


	// Use this for initialization
	void Start() {
		UpdateSliderValue();
	}


	void SetupMinMaxValues(out float minValue, out float maxValue){
		if (RetroBread.MouseInputSource.useMouseAngle) {
			minValue = minValueClassic;
			maxValue = maxValueClassic;
		} else {
			minValue = minValueFree;
			maxValue = maxValueFree;
		}
	}


	public void UpdateSensivityBasedOnSlider(){
		OnSliderValueChange(GetComponent<UnityEngine.UI.Slider>().value);
	}

	public void OnSliderValueChange(float value){
		float minValue, maxValue;
		SetupMinMaxValues(out minValue, out maxValue);
		RetroBread.MouseInputSource.mouseSensivity = minValue + (value * (maxValue - minValue));
//		if (RetroBread.MouseInputSource.useMouseAngle) {
		RetroBread.MouseInputSource.mouseSensivity = Mathf.Sqrt(RetroBread.MouseInputSource.mouseSensivity);
//		}
	}


	void UpdateSliderValue(){
		float minValue, maxValue;
		SetupMinMaxValues(out minValue, out maxValue);
		float value = RetroBread.MouseInputSource.mouseSensivity;
		value *= value;
		value = (value - minValue) / (maxValue - minValue);
//		if (RetroBread.MouseInputSource.useMouseAngle) {
//			Mathf.Sqrt(value);
//		}
		GetComponent<UnityEngine.UI.Slider>().value = value;
	}

}
