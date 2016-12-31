using System;
using System.Collections.Generic;

namespace RetroBread{


public static class CharacterSubjectsBuilder {
	
	// Subjects getter delegate builders indexed by type directly on array
	private delegate EventSubject<GameEntityModel>.GetSubjectsDelegate BuilderAction(Storage.GenericParameter param, Storage.Character character);
	private static BuilderAction[] builderActions = {
		BuildKeyFrame,							// 0: frame = 4
	};
		

	// The public builder method
	public static List<EventSubject<GameEntityModel>> Build(Storage.Character charData, int[] subjectIds){
		List<EventSubject<GameEntityModel>> subjects = new List<EventSubject<GameEntityModel>>(subjectIds.Length);
		EventSubject<GameEntityModel> subject;
		foreach (int subjectId in subjectIds) {
			subject = BuildFromParameter(charData.genericParameters[subjectId], charData);
			if (subject != null) {
				subjects.Add(subject);
			}
		}
		return subjects;
	}


	// Build a single subject
	private static EventSubject<GameEntityModel> BuildFromParameter(Storage.GenericParameter parameter, Storage.Character character){
		EventSubject<GameEntityModel>.GetSubjectsDelegate getSubjectDelegate;
		EventSubject<GameEntityModel>.ReevaluateSubjectsDelegate reevaluateSubject;
		int callIndex = parameter.type;
		if (callIndex < builderActions.Length) {
			getSubjectDelegate = builderActions[callIndex](parameter, character);
			reevaluateSubject = GetReevaluateSubject(parameter);
			return new EventSubject<GameEntityModel>(getSubjectDelegate, reevaluateSubject);
		}
		Debug.Log("CharacterSubjectsBuilder: Unknown subject type: " + parameter.type);
		return null;
	}

	// Get the reevaluate delegate
	private static EventSubject<GameEntityModel>.ReevaluateSubjectsDelegate GetReevaluateSubject(Storage.GenericParameter parameter){
		// TODO
		return null;
	}


#region Builders


	// frame = 4
	private static EventSubject<GameEntityModel>.GetSubjectsDelegate BuildKeyFrame(Storage.GenericParameter parameter,  Storage.Character character){
		// TODO
		parameter.SafeInt(0);
		return null;
	}


#endregion


}

}
