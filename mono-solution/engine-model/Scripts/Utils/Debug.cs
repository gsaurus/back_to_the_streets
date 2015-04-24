//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18063
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;


namespace RetroBread{

	public class Debug{

		private static Debug instance = new Debug();
		public static Debug Instance { get{return instance;} set{instance = value;} }

		// Static convenience methods
		public static void Log(string message){
			instance.InternalLog(message);
		}
		
		public static void LogWarning(string message){
			instance.InternalLogWarning(message);
		}
		
		public static void LogError(string message){
			instance.InternalLogError(message);
		}
		
		public static void StartProfiling(string tag){
			instance.InternalStartProfiling(tag);
		}
		
		public static void StopProfiling(){
			instance.InternalStopProfiling();
		}


		// Debug methods

		protected virtual void InternalLog(string message){
			// Nothing by default
		}

		protected virtual void InternalLogWarning(string message){
			// Nothing by default
		}

		protected virtual void InternalLogError(string message){
			// Nothing by default
		}

		protected virtual void InternalStartProfiling(string tag){
			// Nothing by default
		}

		protected virtual void InternalStopProfiling(){
			// Nothing by default
		}

	}
}

