
using System;


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



	// Constructor
	// TODO: updating order before animations
	public PlayerInputModel(uint playerId, int updatingOrder = 0):
		base(updatingOrder)
	{
		this.playerId = playerId;
		actionPressedCoolers = new uint[NumButtonsSupported];
		actionReleasedCoolers = new uint[NumButtonsSupported];
		actionsHold = new bool[NumButtonsSupported];
	}


	protected override Controller<PlayerInputModel> CreateController(){
		// Get the controller that corresponds to the current animation name
		return new PlayerInputController();
	}

}

