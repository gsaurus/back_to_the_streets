using UnityEngine;
using System;
using System.Collections;

public class GuiChat : MonoBehaviour
{

	private NetworkChat chat;
	private string currentMessage = string.Empty;
	private Vector2 scrollPosition;
	private int lastNumMessages;


	void Start(){
		chat = GetComponent<NetworkChat>();
	}

	void OnGUI(){

		if ((!NetworkCenter.Instance.IsConnected() && !NetworkMaster.Instance.IsAnouncingServer)){
			return;
		}

		GUI.skin.toggle.normal.textColor = Color.white;
		GUI.skin.toggle.hover.textColor = Color.blue;
		GUI.skin.toggle.active.textColor = Color.white;
		GUI.skin.label.normal.textColor = Color.white;
//		GUI.skin.toggle.normal.textColor = Color.black;
//		GUI.skin.toggle.hover.textColor = Color.blue;
//		GUI.skin.toggle.active.textColor = Color.black;
//		GUI.skin.label.normal.textColor = Color.black;

		if (StateManager.state.Keyframe > WorldController.totalGameFrames + 16){
			GameObject entryPointObject = GameObject.Find("EntryPoint");
			if (entryPointObject != null){
				EntryPoint entryPoint = entryPointObject.GetComponent<EntryPoint>();
				if (entryPoint != null){
					entryPoint.Restart();
				}
			}
			StateManager.Instance.SetPaused(true);
		}

		bool isReady = GUI.Toggle(new Rect(Screen.width-100, Screen.height - 40, 80, 27), NetworkSync.Instance.IsPlayerReady(), "READY");

		if (isReady && StateManager.state != null && StateManager.state.Keyframe > WorldController.totalGameFrames){

		}
		NetworkSync.Instance.SetReady(isReady);

		if (NetworkSync.Instance.IsEveryoneReady()) {
			return;
		}

		//GUI.Label(new Rect(0, Screen.height - 25, 80, 20), "Type here:", style);
		currentMessage = GUI.TextField(new Rect(5, Screen.height - 25, Screen.width - 250, 20), currentMessage);

		if (UnityEngine.Event.current.keyCode == KeyCode.Return){
			currentMessage = currentMessage.Trim();
			if (!string.IsNullOrEmpty(currentMessage)){
				chat.SendTextMessage(currentMessage);
				currentMessage = string.Empty;
			}
		}

		float lagTime = NetworkSync.Instance.GetLagTime();
		uint framesLagged = (uint) Math.Ceiling(NetworkSync.lagCompensationRate * lagTime / StateManager.Instance.UpdateRate);
		GUI.Label(new Rect(Screen.width - 240, Screen.height - 27, 180, 27), "tt: " + (int)(lagTime * 1000) + "ms, frames: " + framesLagged);

		scrollPosition = GUILayout.BeginScrollView(scrollPosition,
		                                           GUILayout.Width(Screen.width),
		                                           GUILayout.Height(Screen.height - 50)
		                                           );
		int count = 0;
		foreach(NetworkChatMessage message in chat.ChatHistory){
			if (message.type == NetworkChatMessageType.botMessage) {
				GUILayout.Label(message.timeStamp.ToShortTimeString() + "-BOT: "  + message.text);
			}else {
				GUILayout.Label(message.timeStamp.ToShortTimeString() + "-" + message.senderName + ": "  + message.text);
			}
			++count;
		}
		int newNumMessages = count - lastNumMessages;
		lastNumMessages = count;
		// force scroll to go to bottom
		if (newNumMessages > 0){
			scrollPosition = new Vector2(scrollPosition.x, float.MaxValue);
		}

		GUILayout.EndScrollView();

	}


}

