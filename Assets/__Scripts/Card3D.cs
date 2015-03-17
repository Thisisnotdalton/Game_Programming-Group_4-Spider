using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Card3D : MonoBehaviour {
	//whether the card is revealed
	private bool _faceUp = false;
	//Card identification data
	public string suit;
	public int rank;
	public Color color = Color.black;
	public string colS = "Black";

	//list of decorator objects for card
	public List<GameObject> decGOs = new List<GameObject>();

	//list of card pip objects for card
	public List<GameObject> pipGOs = new List<GameObject>();

	//card definition for identification
	public CardDefinition def;
	
	//movement variables for smooth movement of cards
	private float startTime=0, timeDuration=0;
	private Vector3 endPos=Vector3.zero;
	
	public SpriteRenderer[] spriteRenderers;
	
	virtual public void OnMouseUpAsButton(){
		print (name);
	}
	
	public bool faceUp{
		get{
			return(!_faceUp);		
		}
		set{
			_faceUp=value;
			transform.rotation= Quaternion.Euler(transform.rotation.x,transform.rotation.y+(_faceUp?180:0),transform.rotation.z);
		}
	}
	
	private void PopulateSpriteRenderers(){
		//if there are no sprite renderers
		if (spriteRenderers == null || spriteRenderers.Length == 0) {
			//get the sprite renderer components
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		}
	}


	//start movign the card from it's current position to the next over a set amount of time
	public void Move(Vector3 newPosition, float duration){
		endPos = newPosition;
		startTime = Time.time;
		timeDuration = duration;
	}

	//update the card and move it clsoer to its destination if needed
	public void Update(){
		if (Time.time < startTime + timeDuration) {
			transform.localPosition = Vector3.Lerp (transform.localPosition, endPos, (Time.time-startTime) / timeDuration);
		}
	}

}
/*
[System.Serializable]
public class Decorator{
	public string type;
	public Vector3 loc;
	public bool flip=false;
	public float scale=1f;
}

[System.Serializable]
public class CardDefinition{
	public string face="";
	public int rank;
	public List<Decorator> pips = new List<Decorator>();
	
}
*/