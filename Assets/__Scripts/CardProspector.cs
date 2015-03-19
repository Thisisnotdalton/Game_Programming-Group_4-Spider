using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/*
//enum for the limited possible values of the cards' placement
public enum CardState{
	drawpile,tableau,target,discard
}
*/
public class CardProspector:Card{
	public CardState state = CardState.drawpile;
	public List<CardProspector> hiddenBy = new List<CardProspector> ();
	public int layoutID;
	public SlotDef slotDef;

	override public void OnMouseUpAsButton(){
		//call card clicked method on the Prospector singleton
		//Prospector.S.CardClicked (this);
	}
}