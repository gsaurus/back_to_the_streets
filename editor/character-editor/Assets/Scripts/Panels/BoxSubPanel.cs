﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;
using System.Collections.Generic;


namespace RetroBread{

	public class BoxSubPanel : MonoBehaviour {

		public GameObject x1InputField;
		public GameObject y1InputField;
		public GameObject z1InputField;

		public GameObject x2InputField;
		public GameObject y2InputField;
		public GameObject z2InputField;

		public UnityEvent onValueChanged;


		private InputField _x1Field;
		private InputField _y1Field;
		private InputField _z1Field;

		private InputField _x2Field;
		private InputField _y2Field;
		private InputField _z2Field;


		void Awake () {
			_x1Field = x1InputField.GetComponent<InputField>();
			_y1Field = y1InputField.GetComponent<InputField>();
			_z1Field = z1InputField.GetComponent<InputField>();

			_x2Field = x2InputField.GetComponent<InputField>();
			_y2Field = y2InputField.GetComponent<InputField>();
			_z2Field = z2InputField.GetComponent<InputField>();
		}
		
		public void SetPoints(FixedVector3 point1, FixedVector3 point2){
			_x1Field.text = "" + (point1.X*15);
			_y1Field.text = "" + (point1.Y*15);
			_z1Field.text = "" + (point1.Z*15);
			_x2Field.text = "" + ((point1.X - point2.X)*15);
			_y2Field.text = "" + ((point1.Y - point2.Y)*15);
			_z2Field.text = "" + ((point1.Z - point2.Z)*15);
		}

		public FixedVector3 GetPoint1(){
			float x, y, z;
			StringFieldUtils.TryEvaluate(_x1Field.text, out x);
			StringFieldUtils.TryEvaluate(_y1Field.text, out y);
			StringFieldUtils.TryEvaluate(_z1Field.text, out z);
			x /= 15; y /= 15; z /= 15;
			return new FixedVector3(x, y, z);
		}

		public FixedVector3 GetPoint2(){
			float x, y, z;
			StringFieldUtils.TryEvaluate(_x2Field.text, out x);
			StringFieldUtils.TryEvaluate(_y2Field.text, out y);
			StringFieldUtils.TryEvaluate(_z2Field.text, out z);
			x /= 15; y /= 15; z /= 15;
			FixedVector3 p1 = GetPoint1();
			x = (float)p1.X - x; y = (float)p1.Y - y; z = (float)p1.Z - z;
			return new FixedVector3(x, y, z);
		}


		public void OnFieldChanged(string text){
			onValueChanged.Invoke();
		}

		public void SetInteractible(bool interactible){
			_x1Field.interactable = 
			_y1Field.interactable =
			_z1Field.interactable =
			_x2Field.interactable =
			_y2Field.interactable =
			_z2Field.interactable =
				interactible
			;
		}


	}


}
