using UnityEngine;
using System;
using System.Collections.Generic;



namespace RetroBread{
namespace Network{



// Base class for player data which is transversal between game sessions
// It can contain things like avatar, rank, etc
[Serializable]
public class NetworkPlayerData{

	// Unique identifier, used for reconnections
	public string uniqueId { get; private set; }

	// Player's name
//	public string playerName;

	// debug purposes
	private string _playerName;
	public string playerName { get{ return _playerName; } set{ _playerName = uniqueId = value; }}


	// Constructor giving player id and name. Id can be email for instance
	public NetworkPlayerData(string playerId, string playerName){
		uniqueId = playerId;
		this.playerName = playerName;
	}

	// Constructor giving playerName. Device Unique Identifier is used as uniqueId
	public NetworkPlayerData(string playerName = null){
		uniqueId = SystemInfo.deviceUniqueIdentifier;
		this.playerName = playerName;
	}

}



}}
