using System;



namespace RetroBread{


	// Model storing real player input information (based on input events)
	[Serializable]
	public class PlayerInputModel:Model<PlayerInputModel>{

		public static readonly uint ActionPressedCoolerFrames = 12;
		public static readonly uint ActionReleasedCoolerFrames = 12;
		public static readonly uint NumButtonsSupported = 6;

		// The player controlling this input
		public uint playerId;
		// axis control (normalized)
		public FixedVector3 axis;

		// Need to store action coolers to recognize
		// button presses and releasees during a few frames
		public uint[] actionPressedCoolers;
		public uint[] actionReleasedCoolers;
		public bool[] actionsHold;


		#region Constructors

		// Constructor
		public PlayerInputModel(uint playerId)
		:this(playerId, DefaultVCFactoryIds.PlayerInputControllerFactoryId, null, DefaultUpdateOrder.InputUpdateOrder)
		{}

		// Constructor
		public PlayerInputModel(uint playerId, int updatingOrder)
			:this(playerId, DefaultVCFactoryIds.PlayerInputControllerFactoryId, null, updatingOrder)
		{}

		// Constructor
		public PlayerInputModel(uint playerId,
		                        string controllerFactoryId,
		                        string viewFactoryId,
		                        int updatingOrder
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			this.playerId = playerId;
			actionPressedCoolers = new uint[NumButtonsSupported];
			actionReleasedCoolers = new uint[NumButtonsSupported];
			actionsHold = new bool[NumButtonsSupported];
		}

		#endregion


	}


}

