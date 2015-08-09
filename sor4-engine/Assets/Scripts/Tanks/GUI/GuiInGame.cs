using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;

public class GuiInGame : MonoBehaviour
{
	
	void OnGUI(){

		if (!NetworkCenter.Instance.IsConnected() || StateManager.Instance.IsPaused){
			return;
		}

		int timeLeft = (int)((int)(WorldController.totalGameFrames - StateManager.state.Keyframe)*StateManager.Instance.UpdateRate);
		if (timeLeft < 0) timeLeft = 0;
		string formattedTime = (int)(timeLeft / 60) + ":" + (int)(timeLeft % 60);
		GUI.Label(new Rect(Screen.width-50, 10, 50, 27), formattedTime);

		WorldModel world = StateManager.state.MainModel as WorldModel;
		if (world == null) return;

		uint playerNumber = (uint)NetworkCenter.Instance.GetPlayerNumber();
//		ModelReference modelRef;
//		if (!world.players.TryGetValue(playerNumber, out modelRef)){
//			return;
//		}
//		ShooterEntityModel shooterModel = StateManager.state.GetModel(modelRef) as ShooterEntityModel;
//		if (shooterModel == null) return;
//
//		int rank = 1;
//		int balance = shooterModel.GetBalance();
//		ShooterEntityModel playerModel;
//		foreach (KeyValuePair<uint, ModelReference> pair in world.players){
//			playerModel = StateManager.state.GetModel(pair.Value) as ShooterEntityModel;
//			if (playerModel == null || playerModel == shooterModel) continue;
//			if (playerModel.GetBalance() > balance){
//				++rank;
//			}
//		}
//
//		GUI.Label(new Rect(10, 10, 100, 27), "KILLS: " + shooterModel.totalKills);
//		GUI.Label(new Rect(10, 40, 100, 27), "deaths: " + shooterModel.totalDeaths);
//		GUI.Label(new Rect(10, 70, 100, 27), "rank: #" + rank + " (" + (balance > 0 ? "+" : "") + balance + ")");
	}


}

