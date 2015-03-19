using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//an enum to track the possible states of a FloatingScore
public enum FSState{
	idle,
	pre,
	active,
	post
}

//FloatingScore can move itself on screen following a Bezier curve
public class FloatingScore : MonoBehaviour {
	public FSState state=FSState.idle;
	[SerializeField]
	private int _score=0;//the score field
	private string scoreString;

	//the scorre property also sets scoreString when set
	public int score{
		get{
			return(_score);
		}
		set{
			_score=value;
			scoreString=Utils.AddCommasToNumber(_score);
			GetComponent<GUIText>().text=scoreString;
		}
	}

	public List<Vector3> bezierPts;//Bezier points for movement
	public List<float> fontSizes;//Bezier points for font scaling
	public float timeStart = -1f;
	public float timeDuration=1f;
	public string easingCurve = Easing.InOut;//Uses Easing in Utils.cs

	//The GameObject that will receive the SendMessage when this is done moving
	public GameObject reportFinishTo = null;

	//Set up the FloatingScore and movement
	//Note the use of parameter defaults for eTimeS & eTimeD
	public void Init(List<Vector3> ePts, float eTimeS=0,float eTimeD=1){
		bezierPts = new List<Vector3> (ePts);

		if (ePts.Count == 1) {//if there's only one point
						//...then just go there
			transform.position=ePts[0];
			return;
		}
		//if eTimeS is the default, just start at the current time
		if (eTimeS == 0)
						eTimeS = Time.time;
		timeStart = eTimeS;
		timeDuration = eTimeD;

		state = FSState.pre;//set it to the pre state, ready to start moving
	}

	public void FSCallback(FloatingScore fs){
		//when this callback is called by SendMessage,
		//add the score from the calling FloatingScore
		score += fs.score;
	}

	//update is called once per frame
	void Update(){
				//if this is not moving, just return
				if (state == FSState.idle)
						return;
				//get u from the current time and duration
				//u ranges from 0 to 1 (usually)
				float u = (Time.time - timeStart) / timeDuration;
				//use Easing class from Utils to curve the u value
				float uC = Easing.Ease (u, easingCurve);
				if (u < 0) {//if u<0, then we shouldn't move yet
						state = FSState.pre;
						//move to the initial point
						transform.position = bezierPts [0];
				} else {
						if (u >= 1) {//if u>=1, we're done moving
								uC = 1;//set uC=1 so we don't overshoot
								state = FSState.post;
								if (reportFinishTo != null) {//if there's a callback GameObject use SendMessage
										//to call the FSCallback method with this as the parameter.
										reportFinishTo.SendMessage ("FSCallback", this);
										//now that the message has been sent, destroy this gameObject
										Destroy (gameObject);
								} else//if there's nothing to callback
							//then don't destroy this.  Just let it stay still.
										state = FSState.idle;
						} else
					//0<=u<1, which means that this is active and moving
								state = FSState.active;
						//use Bezier curve to move this to the right point
						Vector3 pos = Utils.Bezier (uC, bezierPts);
						transform.position = pos;
						if (fontSizes != null && fontSizes.Count > 0) {
								//if fontSizes has values in it, then adjust the fontSize of this GUIText
								int size = Mathf.RoundToInt (Utils.Bezier (uC, fontSizes));
								GetComponent<GUIText> ().fontSize = size;
						}
				}
		}
}