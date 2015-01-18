using UnityEngine;
using System.Collections;



// Simple Singleton pattern
// Assumes a public default constructor
public class Singleton<T> where T : class, new(){
	
	private static T instance;
	
	public static T Instance {
		get {
			if (instance == null)	{			
				instance = new T();
			}
			return instance;
		}
	}
	
}


//// One of many possible singleton implementations
//// Like all implementations, it's not perfect,
//// This one assumes a public default constructor,
//// meaning it can still be constructed from outside
//// This implementation is based on http://neutrofoton.com/generic-singleton-pattern-in-c/
//public sealed class Singleton<T> where T : class, new(){
//
//	// private constructor
//	Singleton(){}
//
//	// singleton instance
//	public static T Instance{
//		get{
//			return SingletonInstantiator.instance;
//		}
//	}
//	
//	// private singleton instantiator class
//	class SingletonInstantiator{
//
//		// Explicit static constructor
//		static SingletonInstantiator(){}
//
//		/// Static instance variable
//		internal static readonly T instance = new T();
//	}
//}

