using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

//tracks the states of
public enum FSState
{
		idle,
		pre,
		active,
		post
}

public class FloatingScore : MonoBehaviour
{
		public FSState state = FSState.idle;
		[SerializeField]
		private int
				_score = 0;
		public string scoreString;


		//create a property to handle changing the string when score is changed
		public int score {
				get{ return _score;}
				set {
						_score = value;
						scoreString = Utils.AddCommasToNumber (_score);
						GetComponentInChildren<Text> ().text = scoreString;
				}
		}

		public List<Vector3> bezierPts;
		public List<float>	fontSizes;
		public float timeStart = -1f, timeDuration = 1f;
		public string easingCurve = Easing.InOut;

		//this will receive the SendMessage
		public GameObject reportFinishTo = null;

		public void Init (List<Vector3> ePts, float eTimeS=0, float eTimeD=1)
		{
				
				//initialize list
				bezierPts = new List<Vector3> (ePts);

				if (ePts.Count == 1) {//if only one point exists
						//set transofrm to first point and get out
						GetComponentInChildren<RectTransform> ().anchoredPosition = ePts [0];
						return;
				}
				//start at current time if at default
				if (eTimeS == 0)
						eTimeS = Time.time;

				timeStart = eTimeS;
				timeDuration = eTimeD;

				//set it to pre state, before it starts moving
				state = FSState.pre;
		}

		public void FSCallback (FloatingScore fs)
		{
				//when callback is called by send message, add the score from the calling to FloatingScore
				score += fs.score;
		}

		public void Update ()
		{
				
				//if not moving, get out
				if (state == FSState.idle) {
						GetComponentInChildren<RectTransform> ().anchoredPosition = bezierPts[bezierPts.Count-1];
						return;
				}
				//get u from current time (0-1) usually
				float u = (Time.time - timeStart) / timeDuration;

				float uC = Easing.Ease (u, easingCurve);
				if (u < 0) {//if u<0, we shouldn't move yet
						state = FSState.pre;
			
					
						GetComponentInChildren<RectTransform> ().anchoredPosition = bezierPts [0];


				} else {
						if (u >= 1) {//we're done moving
								uC = 1;
								state = FSState.post;
								if (reportFinishTo != null) {//if we have a callback object
										//SendMessage to call FSCallback
										reportFinishTo.SendMessage ("FSCallback", this);
										//message sent, now destroy this game object
										Destroy (gameObject);
								} else {//there is nothing to call back, idle
										state = FSState.idle;
								
								}
						} else { //0<=u<=1, we are actively moving
								state = FSState.active;
						}
						//use bezier curve to move this to right point
						Vector3 pos = Utils.Bezier (uC, bezierPts);
						GetComponentInChildren<RectTransform> ().anchoredPosition = pos;
						if (fontSizes != null && fontSizes.Count > 0) {//if we have font sizes
								int size = Mathf.RoundToInt (Utils.Bezier (uC, fontSizes));
								GetComponentInChildren<Text> ().fontSize = size;
						}
					
				}
		}
}
