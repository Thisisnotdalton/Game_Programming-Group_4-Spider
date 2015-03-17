using UnityEngine;
using System.Collections;

public class SpiderMain : MonoBehaviour {
	//deck to hold cards for game
	private Deck deck;
	//public asset for the input/layout
	public TextAsset cardsXML, layoutXML;


	// Use this for initialization
	void Awake() {
		deck=GetComponent<Deck>();
		deck.InitDeck (cardsXML.text);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
