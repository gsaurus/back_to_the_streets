using System;
using System.Collections.Generic;
using ProtoBuf;



namespace RetroBread{
	namespace Network{



		// Base class for player data which is transversal between game sessions
		// It can contain things like avatar, rank, etc
		[ProtoContract]
		public class NetworkPlayerData{

			// Unique identifier, used for reconnections
			[ProtoMember(1)]
			public string uniqueId;

			// player name
			[ProtoMember(2)]
			public string playerName;


			public NetworkPlayerData(){
				// Nothing to do
			}

			// Constructor giving player id and name. Id can be email for instance
			public NetworkPlayerData(string playerId, string playerName){
				uniqueId = playerId;
				this.playerName = playerName;
			}

		}



	}
}
