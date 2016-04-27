using RetroBread;
using System;
using System.Collections.Generic;
using ProtoBuf;

[ProtoContract]
public class NetworkSorPlayerData:RetroBread.Network.NetworkPlayerData{

	// What character the player is using
	[ProtoMember(1)]
	public int selectedCharacter;

	public NetworkSorPlayerData():base(){
		// Nothing to do
	}

	// Constructor giving player id and name. Id can be email for instance
	public NetworkSorPlayerData(string playerId, string playerName, int selectedCharacter):base(playerId, playerName){
		this.selectedCharacter = selectedCharacter;
	}
		

}

