using System;
using System.Collections.Generic;

namespace RetroBread{


//
// - Have a list of EventSubject to get the models in evaluation;
// - Have a list of conditions
// - Have a list of actions to execute if the conditions are met
public class ConditionalEvent<T>{

	// Subjects to obtain the models in evaluation
	private List<EventSubject<T>> subjects;

	// List of conditions
	private List<EventCondition<T>> conditions;

	// List of actions
	private List<EventAction<T>> actions;


	// Constructor, takes the full data
	public ConditionalEvent(
		List<EventSubject<T>> subjects,
		List<EventCondition<T>> conditions,
		List<EventAction<T>> actions
	){
		this.subjects = subjects;
		this.conditions = conditions;
		this.actions = actions;
	}

	public void Evaluate(T model){
		// First get the lists of subjects
		List<T>[] allSubjectsModels = new List<T>[subjects.Count];
		for (int i = 0 ; i < subjects.Count; ++i) {
			allSubjectsModels[i] = subjects[i].GetSubjects(model);
		}

		// Evaluate conditions
		foreach (EventCondition<T> condition in conditions){
			if (condition.SubjectId < 0 || condition.SubjectId >= allSubjectsModels.Length){
				// Invalid subject, abort
				Debug.LogWarning("Invalid condition subject ID " + condition.SubjectId);
				return;
			}
			List<T> subjectModels = allSubjectsModels[condition.SubjectId];
			if (subjectModels == null || subjectModels.Count == 0){
				// Subject is empty, abort
				return;
			}
			// Evaluate condition for each model in the subject
			List<T> modelsThatMeetCondition = new List<T>();
			foreach (T subjectModel in subjectModels){
				if (condition.Evaluate(subjectModel, allSubjectsModels)) {
					modelsThatMeetCondition.Add(subjectModel);
				}
			}
			if (modelsThatMeetCondition.Count == 0){
				// Condition wasn't met by any subject, abort
				return;
			}
			// Update subjects with only the ones that met the condition
			allSubjectsModels[condition.SubjectId] = modelsThatMeetCondition;
		}

		// If we got here, conditions are met for some models
		// Reevaluate subjects
		for (int i = 0; i < subjects.Count; ++i){
			subjects[i].ReevaluateSubjects(allSubjectsModels[i]);
		}

		// Finally execute the events
		foreach (EventAction<T> action in actions){
			if (action.SubjectId < 0 || action.SubjectId >= allSubjectsModels.Length){
				// Invalid subject, ignore
				Debug.LogWarning("Invalid action subject ID " + action.SubjectId);
				continue;
			}
			// Execute action for each model in its subject
			foreach(T subjectModel in allSubjectsModels[action.SubjectId]) {
				action.Execute(subjectModel, allSubjectsModels);
			}
		}

	}

}



}