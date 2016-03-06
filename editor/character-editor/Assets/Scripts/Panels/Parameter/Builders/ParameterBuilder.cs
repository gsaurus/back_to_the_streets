using UnityEngine;
using System.Collections;
using RetroBread.Editor;

namespace RetroBread{
	
	public interface ParameterBuilder{
		
		// Get a string array containing description of each available parameter type
		string[] TypesList();

		// Build the components of the parameter panel (parent), for the given type
		void Build(GameObject parent, GenericParameter parameter);

	}


}