using System;
using ProtoBuf;



namespace RetroBread{


	// Model storing real player input information (based on input events)
	[ProtoContract]
	public class PlayerInputModel:Model<PlayerInputModel>{

		// The player controlling this input
		[ProtoMember(1)]
		public uint playerId;

		// axis control (normalized)
		[ProtoMember(2)]
		public FixedFloat axis;


		#region Constructors

		// Default constructor
		public PlayerInputModel(){
			axis = 0;
		}

		// Constructor
		public PlayerInputModel(uint playerId)
		:this(playerId, DefaultVCFactoryIds.PlayerInputControllerFactoryId, null, DefaultUpdateOrder.InputUpdateOrder)
		{}

		// Constructor
		public PlayerInputModel(uint playerId,
		                        string controllerFactoryId,
		                        string viewFactoryId,
		                        int updatingOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			this.playerId = playerId;
			this.axis = 0;
		}
	

		#endregion


	}


}
