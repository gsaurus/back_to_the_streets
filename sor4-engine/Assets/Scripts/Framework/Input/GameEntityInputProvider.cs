
using System;


// Define accessor methods to input
public interface GameEntityInputProvider{

	FixedVector3 GetInputAxis(Model model);

	bool IsButtonPressed(Model model, uint buttonId);

	bool IsButtonHold(Model model, uint buttonId);

	bool IsButtonReleased(Model model, uint buttonId);

}

