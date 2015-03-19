using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Scoreboard : MonoBehaviour {
	public static Scoreboard S;
	public GameObject prefabFloatingScore;

	[SerializeField]
	private int _score = 0;
	private string _scoreString;

	//field for score
	public int score{
		get{ return _score;}
		set{_score=value;
			scoreString=Utils.AddCommasToNumber(_score);
		}
	}

	public string scoreString{
		get{ return _scoreString;}
		set{
			_scoreString=value;
			GetComponentInChildren<Text>().text = _scoreString;
		}
	}

	void Awake(){
		S = this;
	}

	void Update(){
		S.GetComponentInChildren<RectTransform> ().anchoredPosition = new Vector2 (Screen.width*0.5f,Screen.height*0.65f);
	}

	//when called by SendMessage, this adds the fs.score to this score
	public void FSCallback(FloatingScore fs){
		score += fs.score;
	}

	//This creates a new FloatingScore
	public FloatingScore CreateFloatingScore(int amt, List<Vector3> pts){
		GameObject go = Instantiate (prefabFloatingScore) as GameObject;
		FloatingScore fs = go.GetComponentInChildren<FloatingScore> ();
		fs.score = amt;
		fs.reportFinishTo = this.gameObject;
		fs.Init (pts);
		return fs;
	}

}
