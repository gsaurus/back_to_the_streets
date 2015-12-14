using System;


namespace RetroBread{


	// Define accessor methods to input
	// Example 1: controller based on standard input events
	// Example 2: controller based on artificial inteligence
	public interface GameEntityInputProvider{

		FixedFloat GetInputAxis(Model model);

	}


}
