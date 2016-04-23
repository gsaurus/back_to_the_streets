using System;


namespace RetroBread{


	// Define accessor methods to input
	// Example 1: controller based on standard input events
	// Example 2: controller based on artificial inteligence
	public interface GameEntityInputProvider{

		FixedVector3 GetInputAxis(Model model);

		bool IsButtonPressed(Model model, uint buttonId);

		bool IsButtonHold(Model model, uint buttonId);

		bool IsButtonReleased(Model model, uint buttonId);

		void ConsumePress(Model model, uint buttonId);

		void ConsumeRelease(Model model, uint buttonId);

	}


}
