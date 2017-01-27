using System;
using System.Collections.Generic;

namespace RetroBread{


// Obtains a list of models in evaluation (of type T)
public class EventSubject<T>{
	public delegate List<T> GetSubjectsDelegate(T model, List<T>[] subjectModels);
	public delegate List<T> ReevaluateSubjectsDelegate(List<T> subjects);

	private GetSubjectsDelegate getSubjectsDelegate;
	private ReevaluateSubjectsDelegate reevaluateSubjectsDelegate;

	public EventSubject(GetSubjectsDelegate getSubjectsDelegate, ReevaluateSubjectsDelegate reevaluateSubjectsDelegate){
		this.getSubjectsDelegate = getSubjectsDelegate;
		this.reevaluateSubjectsDelegate = reevaluateSubjectsDelegate;
	}

	public List<T> GetSubjects(T model, List<T>[] subjectModels){
		return getSubjectsDelegate(model, subjectModels);
	}

	public List<T> ReevaluateSubjects(List<T> subjects){
		return reevaluateSubjectsDelegate(subjects);
	}
}


}