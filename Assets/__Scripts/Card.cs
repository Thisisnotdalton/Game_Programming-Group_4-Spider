using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class Card : MonoBehaviour {
	public string suit;
	public int rank;
	public Color color = Color.black;
	public string colS = "Black";
	
	public List<GameObject> decGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();
	
	public GameObject back;
	public CardDefinition def;


	//movement variables
	private float startTime=0, timeDuration=0;
	private Vector3 endPos=Vector3.zero;
	
	public SpriteRenderer[] spriteRenderers;
	
	virtual public void OnMouseUpAsButton(){
		//print (name);
	}
	
	public bool faceUp{
		get{
			return(!back.activeSelf);		
		}
		set{
			back.SetActive(!value);
		}
	}
	void Start () {
		SetSortOrder (0);
		
	}
	
	private void PopulateSpriteRenderers(){
		//if there are no sprite renderers
		if (spriteRenderers == null || spriteRenderers.Length == 0) {
			//get the sprite renderer components
			//spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
			List<SpriteRenderer> spriteList = new List<SpriteRenderer>();
			foreach (Transform t in GetComponentsInChildren<Transform>()) {
				if(t.GetComponent<SpriteRenderer>()!=null){
					spriteList.Add(t.GetComponent<SpriteRenderer>());
			
				}
			}
			spriteList.Add(transform.Find("back").GetComponent<SpriteRenderer>());
			spriteRenderers=spriteList.ToArray();

		}
	}
	
	public void SetSortingLayerName(string tSLN){
		PopulateSpriteRenderers ();
		//print ("Set sorting layer name to " + tSLN);
		foreach (SpriteRenderer tSR in spriteRenderers) {
			tSR.sortingLayerName=tSLN;	

			//print("Sorting layer name set to "+tSR.name+" - "+tSR.sortingLayerName);
		}

	}

	public string GetSortingOrderLayerName(){
		PopulateSpriteRenderers ();
		return spriteRenderers[0].sortingLayerName;
	}

	public void Move(Vector3 newPosition, float duration){
		endPos = newPosition;
		startTime = Time.time;
		timeDuration = duration;
	}
	
	public void Update(){
		if (Time.time < startTime + timeDuration) {
			//transform.localPosition = Vector3.Lerp (transform.localPosition, endPos, (Time.time-startTime) / timeDuration);
			transform.position=Vector3.Lerp(transform.position,endPos,(Time.time-startTime) / timeDuration);
		}
	}
	
	public void SetSortOrder(int sOrd){
		PopulateSpriteRenderers ();
		//iterate through all sprite renderers and set to following order:
		//white background is bottom, sOrd
		//next is pips, decorators, etc, sOrd+1
		//then the back is covering everything when visible, sOrd+2
		foreach (SpriteRenderer tSR in spriteRenderers) {
			if(tSR.gameObject == this.gameObject){
				//this is the white background
				tSR.sortingOrder=sOrd;
				continue;
			}
			switch(tSR.gameObject.name){
			case "back":
				tSR.sortingOrder=sOrd+2;
				break;
			case "face":
			default:
				tSR.sortingOrder=sOrd+1;
				break;
			}
		}
	}
	
}

[System.Serializable]
public class Decorator{
	public string type;
	public Vector3 loc;
	public bool flip=false;
	public float scale=1f;
}

[System.Serializable]
public class CardDefinition{
	public string face;
	public int rank;
	public List<Decorator> pips = new List<Decorator>();
	
}