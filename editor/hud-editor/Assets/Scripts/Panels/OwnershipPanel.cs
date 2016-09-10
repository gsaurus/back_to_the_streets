using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using RetroBread.Editor;


namespace RetroBread{

	public class OwnershipPanel : MonoBehaviour {

		public GameObject teamObj;
		public GameObject playerObj;
		public GameObject attackAnchorObj;

		InputField _teamObj;
		InputField _playerObj;
		Toggle _attackAnchorObj;

		void Awake(){
			_teamObj = teamObj.GetComponent<InputField>();
			_playerObj = playerObj.GetComponent<InputField>();
			_attackAnchorObj = attackAnchorObj.GetComponent<Toggle>();
			HUDEditor.Instance.OnObjectChangedEvent += OnObjectChangedEvent;
		}

		// Use this for initialization
		void OnEnable() {
			UpdateGuiItems();
		}


		void UpdateGuiItems(){
			HUDObject hudObj = HUDEditor.Instance.CurrentObject();
			if (hudObj == null) return;
			_teamObj.text = hudObj.teamId + "";
			_playerObj.text = hudObj.playerId + "";
			_attackAnchorObj.isOn = hudObj.attackAndGrabDelegation;
		}


		void OnObjectChangedEvent(){
			UpdateGuiItems();
		}


		public void OnTeamChanged(string value){
			HUDObject hudObj = HUDEditor.Instance.CurrentObject();
			if (hudObj == null) return;
			hudObj.teamId = int.Parse(value);
		}

		public void OnPlayerChanged(string value){
			HUDObject hudObj = HUDEditor.Instance.CurrentObject();
			if (hudObj == null) return;
			hudObj.playerId = int.Parse(value);
		}

		public void OnAttackAndAnchoringToggleChanged(bool value){
			HUDObject hudObj = HUDEditor.Instance.CurrentObject();
			if (hudObj == null) return;
			hudObj.attackAndGrabDelegation = value;
		}

		public void OnPortraitSelectionChanged(bool value){
			HUDObject hudObj = HUDEditor.Instance.CurrentObject();
			if (hudObj == null) return;
			hudObj.usePortraitSprite = value;
		}

		public void OnCharacterTextToggleChanged(bool value){
			HUDObject hudObj = HUDEditor.Instance.CurrentObject();
			if (hudObj == null) return;
			hudObj.useCharacterText = value;
		}



	}

}