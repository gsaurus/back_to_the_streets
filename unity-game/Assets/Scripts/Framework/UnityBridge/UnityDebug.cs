using System;

namespace RetroBread{

	public class UnityDebug: RetroBread.Debug{

		// Default constructor
		public UnityDebug(){
			// Nothing to do
		}

		protected override void InternalLog(string message){
			UnityEngine.Debug.Log(message);
		}
		
		protected override void InternalLogWarning(string message){
			UnityEngine.Debug.LogWarning(message);
		}
		
		protected override void InternalLogError(string message){
			UnityEngine.Debug.LogError(message);
		}
		
		protected override void InternalStartProfiling(string tag){
			UnityEngine.Profiler.BeginSample(tag);
		}
		
		protected override void InternalStopProfiling(){
			UnityEngine.Profiler.EndSample();
		}

	}
}

