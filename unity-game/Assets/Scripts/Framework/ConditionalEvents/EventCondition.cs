using System;
using System.Collections.Generic;

namespace RetroBread{


// A condition applied to a certain subject
public class EventCondition<T>{
	public delegate bool EvaluationDelegate(T model, List<T>[] subjectModels);

	private EvaluationDelegate evaluationDelegate;
	public int SubjectId { get; private set; }

	public EventCondition(EvaluationDelegate evaluationDelegate, int subjectId){
		this.evaluationDelegate = evaluationDelegate;
		SubjectId = subjectId;
	}

	public bool Evaluate(T model,  List<T>[] subjectModels){
		return evaluationDelegate(model, subjectModels);
	}
}
	

}