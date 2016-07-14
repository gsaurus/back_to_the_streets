using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System.Linq;


namespace RetroBread{

	public class OpenNewPanel : MonoBehaviour {

		// References to relevant objects
		public GameObject filesList;
		public GameObject openButton;
		public GameObject closeButton;

		// Simplified references
		private SingleSelectionList _filesList;
	

		// Use this for initialization
		void Awake() {
			_filesList = filesList.GetComponent<SingleSelectionList>();
		}


		void OnEnable(){
			SetupFilesList();
			// can only close popup if a hud is already selected
			closeButton.GetComponent<Button>().interactable = HUDEditor.Instance.hud != null;
			// can only open a hud if any exists
			openButton.GetComponent<Button>().interactable = _filesList.OptionsCount > 0;
		}
			


		private void SetupFilesList(){
			DirectoryInfo hudsModelsDir = new DirectoryInfo(HUDEditor.hudsModelsPath);
			FileInfo[] hudFiles = hudsModelsDir.GetFiles().Where(fileInfo => !fileInfo.Name.Contains(".")).ToArray<FileInfo>();
			List<string> fileNames = new List<string>(hudFiles.Length);
			foreach (FileInfo fileInfo in hudFiles) {
				fileNames.Add(fileInfo.Name);
			}
			_filesList.Options = fileNames;
		}


		public void OnOpen(){
			
			HUDEditor.Instance.LoadHud(_filesList.SelectedOption);
			ClosePanel();
		}


		public void ClosePanel(){
			this.gameObject.SetActive(false);
		}


	}



}