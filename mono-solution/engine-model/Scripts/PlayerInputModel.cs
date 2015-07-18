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
		public FixedVector3[] axis;

		// Need to store action coolers to recognize
		// button presses and releasees during a few frames
		[ProtoMember(3)]
		public uint[] actionPressedCoolers;
		[ProtoMember(4)]
		public uint[] actionReleasedCoolers;
		[ProtoMember(5)]
		public bool[] actionsHold;


		#region Constructors

		// Default constructor
		public PlayerInputModel(){
			// Nothing to do
		}

		// Constructor
		public PlayerInputModel(uint playerId, int numAxis, int numButtons)
		:this(playerId, DefaultVCFactoryIds.PlayerInputControllerFactoryId, null, DefaultUpdateOrder.InputUpdateOrder, numAxis, numButtons)
		{}

		// Constructor
		public PlayerInputModel(uint playerId, int updatingOrder, int numAxis, int numButtons)
			:this(playerId, DefaultVCFactoryIds.PlayerInputControllerFactoryId, null, updatingOrder, numAxis, numButtons)
		{}

		// Constructor
		public PlayerInputModel(uint playerId,
		                        string controllerFactoryId,
		                        string viewFactoryId,
		                        int updatingOrder,
		                        int numAxis,
		                        int numButtons
		):base(controllerFactoryId, viewFactoryId, updatingOrder){
			this.playerId = playerId;
			axis = new FixedVector3[numAxis];
			actionPressedCoolers = new uint[numButtons];
			actionReleasedCoolers = new uint[numButtons];
			actionsHold = new bool[numButtons];
		}


		// Copy fields from other model
		protected override void AssignCopy(PlayerInputModel other){
			base.AssignCopy(other);
			playerId = other.playerId;
			axis = other.axis;
			actionPressedCoolers = (uint[]) other.actionPressedCoolers.Clone();
			actionReleasedCoolers = (uint[]) other.actionReleasedCoolers.Clone();
			actionsHold = (bool[]) other.actionsHold.Clone();
		}

		#endregion


	}


}

