
using System;


// Model storing real player input information (based on input events)
[Serializable]
public class PlayerInputModel:Model<PlayerInputModel>{

	public static readonly uint ActionCoolerFrames = 12;
	public static readonly uint NumButtonsSupported = 6;

	// The player controlling this input
	public uint playerId;
	// axis control (normalized)
	public FixedVector3 axis;

	// Need to store action coolers to register a button press
	// during a few frames after it was first pressed
	public uint[] actionCoolers;
	public bool[] actionsHolded;



	// Constructor
	// TODO: updating order before animations
	public PlayerInputModel(uint playerId, int updatingOrder = 0):
		base(updatingOrder)
	{
		this.playerId = playerId;
		actionCoolers = new uint[NumButtonsSupported];
		actionsHolded = new bool[NumButtonsSupported];
	}


	protected override Controller<PlayerInputModel> CreateController(){
		// Get the controller that corresponds to the current animation name
		return new PlayerInputController();
	}

}

