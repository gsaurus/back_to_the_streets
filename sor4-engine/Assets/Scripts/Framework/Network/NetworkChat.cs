using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace RetroBread{
namespace Network{


public enum NetworkChatMessageType{
	normalMessage,
	privateMessage,
	botMessage
}

// Chat Message = sender, text message, the time it arrived, and if it's private or not
public class NetworkChatMessage{
	public string senderGuid	{ get; private set; }
	public string senderName	{ get; private set; }
	public string text			{ get; private set; }
	public DateTime timeStamp	{ get; private set; }
	public NetworkChatMessageType type { get; private set; }

	public NetworkChatMessage(string senderGuid, string text, DateTime timeStamp, NetworkChatMessageType type){
		this.senderGuid	= senderGuid;
		NetworkPlayerData playerData = NetworkCenter.Instance.GetPlayerData(senderGuid);
		this.senderName = playerData == null ? string.Empty : playerData.playerName;
		this.text		= text;
		this.timeStamp	= timeStamp;
		this.type		= type;
	}
}

// A simple network chat (logics only)
// Each message contains the sender, the text message, the time it arrived, and if it's a PM
public class NetworkChat: MonoBehaviour{

	// Maximum chat history size
	private int maxHistorySize = 1000;
	public int MaxHistorySize
	{
		get{
			return maxHistorySize;
		}
		set{
			maxHistorySize = value;
			chatHistory.Size = maxHistorySize;
		}
	}


	// Notify when a message is received
	public delegate void OnMessageReceivedDelegate(NetworkChatMessage message);
	[NonSerialized]
	public OnMessageReceivedDelegate onMessageReceivedDelegate;

	// Get a bot message for when a player connects / disconnects
	public delegate string GetBotMessageForPlayerDelegate(NetworkPlayerData player);
	[NonSerialized]
	public GetBotMessageForPlayerDelegate getBotEnterMessageForPlayerDelegate = DefaultBotEnterMessageForPlayer;
	[NonSerialized]
	public GetBotMessageForPlayerDelegate getBotLeaveMessageForPlayerDelegate = DefaultBotLeaveMessageForPlayer;
	[NonSerialized]
	public GetBotMessageForPlayerDelegate getBotPlayerReadyDelegate = DefaultBotReadyMessageForPlayer;
	[NonSerialized]
	public GetBotMessageForPlayerDelegate getBotPlayerNotReadyDelegate = DefaultBotNotReadyMessageForPlayer;



	// Chat history
	private FixedSizedQueue<NetworkChatMessage> chatHistory;
	public IEnumerable ChatHistory {
		get{
			return chatHistory;
		}
	}


	// Create chat with max num messages kept in history
	void Awake() {
		this.chatHistory = new FixedSizedQueue<NetworkChatMessage>(maxHistorySize);
	}

	// On enable register connection delegates on network center
	void OnEnable(){
		NetworkCenter.Instance.playerConnectedEvent += OnPlayerConnectionConfirmed;
		NetworkCenter.Instance.playerDisconnectedEvent += OnPlayerDisconnectionConfirmed;
		NetworkSync.Instance.playerReadyEvent += OnPlayerReady;
		NetworkSync.Instance.playerNotReadyEvent += OnPlayerNotReady;
	}

	// On disable unregister connection delegates
	void OnDisable(){
		NetworkCenter.Instance.playerConnectedEvent -= OnPlayerConnectionConfirmed;
		NetworkCenter.Instance.playerDisconnectedEvent -= OnPlayerDisconnectionConfirmed;
		NetworkSync.Instance.playerReadyEvent -= OnPlayerReady;
		NetworkSync.Instance.playerNotReadyEvent -= OnPlayerNotReady;
	}


	// Send a message to all
	public void SendTextMessage(string text) {
		GetComponent<NetworkView>().RPC("ChatMessageReceived", RPCMode.All, UnityEngine.Network.player.guid, text);
	}

	// Send a private message to all
	public void SendPrivateTextMessage(string text, NetworkPlayer player) {
		GetComponent<NetworkView>().RPC("PrivateChatMessageReceived", player, UnityEngine.Network.player.guid, text);
	}

	// Add a local bot message
	public void AddBotTextMessage(string text) {
		AddTextMessage(text, UnityEngine.Network.player.guid, NetworkChatMessageType.botMessage);
	}

	// Remove all messages from history
	public void ClearHistory(){
		chatHistory.Clear();
	}

	// A new player joined
	void OnPlayerConnectionConfirmed(string guid){
		NetworkPlayerData playerData = NetworkCenter.Instance.GetPlayerData(guid);
		if (playerData != null && getBotEnterMessageForPlayerDelegate != null) {
			string message = getBotEnterMessageForPlayerDelegate(playerData);
			if (message != null) {
				AddTextMessage(message, guid, NetworkChatMessageType.botMessage);
			}
		}
	}

	// A player left
	void OnPlayerDisconnectionConfirmed(string guid) {
		NetworkPlayerData playerData = NetworkCenter.Instance.GetPlayerData(guid);
		if (playerData != null) {
			string message = getBotLeaveMessageForPlayerDelegate(playerData);
			if (message != null) {
				AddTextMessage(message, guid, NetworkChatMessageType.botMessage);
			}
		}
	}


	// RPC when a message is received
	[RPC] public void ChatMessageReceived(string senderGuid, string text) {
		AddTextMessage(text, senderGuid, NetworkChatMessageType.normalMessage);
	}


	// RPC when a private message is received
	[RPC] public void PrivateChatMessageReceived(string senderGuid, string text, NetworkMessageInfo info) {
		AddTextMessage(text, senderGuid, NetworkChatMessageType.privateMessage);
	}


	// A player is ready
	void OnPlayerReady(string guid){
		NetworkPlayerData playerData = NetworkCenter.Instance.GetPlayerData(guid);
		if (playerData != null && getBotPlayerReadyDelegate != null) {
			string message = getBotPlayerReadyDelegate(playerData);
			if (message != null) {
				AddTextMessage(message, guid, NetworkChatMessageType.botMessage);
			}
		}
	}

	// A player is not ready
	void OnPlayerNotReady(string guid){
		NetworkPlayerData playerData = NetworkCenter.Instance.GetPlayerData(guid);
		if (playerData != null && getBotPlayerNotReadyDelegate != null) {
			string message = getBotPlayerNotReadyDelegate(playerData);
			if (message != null) {
				AddTextMessage(message, guid, NetworkChatMessageType.botMessage);
			}
		}
	}


	private void AddTextMessage(string text, string playerGuid, NetworkChatMessageType type) {
		NetworkChatMessage message = new NetworkChatMessage(playerGuid, text, DateTime.Now, type);
		chatHistory.Enqueue(message);
		if (onMessageReceivedDelegate != null){
			onMessageReceivedDelegate(message);
		}
	}


	// Default bot enter message
	private static string DefaultBotEnterMessageForPlayer(NetworkPlayerData playerData) {
		//int playerNum = NetworkCenter.Instance.GetPlayerNumber(playerData);
		return playerData.playerName + " joined"; //with player number: " + NetworkCenter.Instance.GetPlayerNumber(playerData);
	}

	// Default bot leave message
	private static string DefaultBotLeaveMessageForPlayer(NetworkPlayerData playerData) {
		return playerData.playerName + " left";
	}

	// Default bot ready message
	private static string DefaultBotReadyMessageForPlayer(NetworkPlayerData playerData) {
		//int playerNum = NetworkCenter.Instance.GetPlayerNumber(playerData);
		return playerData.playerName + " is ready";
	}
	
	// Default bot not ready message
	private static string DefaultBotNotReadyMessageForPlayer(NetworkPlayerData playerData) {
		return playerData.playerName + " is not ready";
	}
	
}



}}
