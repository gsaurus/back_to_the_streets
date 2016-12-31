using System;
using System.Collections.Generic;

namespace RetroBread{


// An action applied to a certain subject
public class EventAction<T>{
	public delegate void ExecutionDelegate(T model, List<T>[] subjectModels);

	private ExecutionDelegate executionDelegate;
	public int SubjectId { get; private set; }

	public EventAction(ExecutionDelegate executionDelegate, int subjectId){
		this.executionDelegate = executionDelegate;
		SubjectId = subjectId;
	}

	public void Execute(T model,  List<T>[] subjectModels){
		executionDelegate(model, subjectModels);
	}
}


}