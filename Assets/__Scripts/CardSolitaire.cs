using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CardState{
	drawpile,
	tableau,
	target,
	discard
}

public class CardSolitaire : Card {
	public int layoutID;
	public SlotDef slotDef;
	public CardState state = CardState.drawpile;
	//the hiddenBy list stores which other cards will keep this one face down
	public List<CardSolitaire> hiddenBy = new List<CardSolitaire> ();
	//these refer to the card above or below this one in rank for this stack
	private CardSolitaire lesserCard,greaterCard;
	//keep track of where the card was last in a valid position
	private Vector3 lastPosition;
	//reference to last card paired to
	private CardSolitaire lastCard;
	//the possible target card to drop this on
	private CardSolitaire targetCard;
	//keep track of which column this card is in
	public int row=0;

	//boolean to determine if the card is still active/moveable
	private bool active=true;

	void Awake(){
		lesserCard = null;
		greaterCard = null;
		targetCard = null;
	}


	public void OnMouseDown(){
		//pair card with mouse
		if (SpiderSolitaire.S.Card == null && faceUp) {
			if(greaterCard!=null){
				Split();
			}
			SpiderSolitaire.S.Card = this.gameObject;
			SetSortingLayerName("Draw");
			RejoinCards();
		}
	}

	public void OnMouseUp(){
		if (SpiderSolitaire.S.Card!= null && SpiderSolitaire.S.Card.name == name) {
			print ("This target card has been dropped");
			SpiderSolitaire.S.Card=null;
			if (targetCard == null || !targetCard.LesserRank (this)) {

				print ("Invalid placement.");
				if (lastCard != null) {
					Join (lastCard);
					print ("Joining to "+lastCard.name);
				}else{
					this.Move (lastPosition, .5f);
				}
			} else {
				if(lastCard!=null)lastCard.faceUp=true;
				Join (targetCard);
				//print ("Good job");
			}
		}
	}

	public void OnTriggerEnter(Collider other){
		//get CardSolitaire
		if (SpiderSolitaire.S.Card!= null && SpiderSolitaire.S.Card.name==name &&
		    other.GetComponent<CardSolitaire> ().lesserCard == null && other.GetComponent<CardSolitaire> ().faceUp ) {
			targetCard=other.GetComponent<CardSolitaire>();
			print("New target card:"+targetCard.name);
		}
	}

	public void OnTriggerLeave(Collider other){
		if (greaterCard == null && targetCard != null) {

			if (other.gameObject.GetComponent<CardSolitaire>()!=null && other.gameObject.GetComponent<CardSolitaire> () == targetCard) {
				targetCard = null;
			}
		}
	}

	//return the last card, furthest down in this stack
	public CardSolitaire BottomOfStack(){
		if (lesserCard == null) {
			return this;
		} else {
			return lesserCard.BottomOfStack();
		}
			
	}


	//Returns true if the other card may be placed on this card, i.e. the other card is a lesser adjacent rank
	bool LesserRank (CardSolitaire other) {
		return (other.rank == this.rank - 1);
	}
	
	//Split this card from the stack it's resting on while still keeping track of the others
	void Split(){
		//update last position and card
		lastPosition = transform.position;
		print ("Last position:" + lastPosition);
		lastCard = greaterCard;
		//unlink card above from this card
		greaterCard.lesserCard = null;
		//unlink it from the card above it
		greaterCard = null;
		//remove the parenting of this card from the greater card
		transform.parent = SpiderSolitaire.S.layoutAnchor;
	}
	
	//Join this card to a stack with a card of higher rank
	public void Join(CardSolitaire other){
		//link greater card to us
		other.lesserCard = this;
		//link ourselves to the greater card
		greaterCard = other;
		//update column index
		row = other.row+1;
		//set this card to be a child of the previous card
		transform.parent = greaterCard.transform;
		//align this card to the one above it
		Vector3 pos = new Vector3(0,-1,0.125f);
	/*
		pos.x = 0; //they are in line now
		pos.y -=1; //offset y so the card above is slightly visible
		pos.z += 1;//this card is one above the other card
	*/	transform.localPosition = pos; //store this into local position
		print ("Local position:" + transform.localPosition);

		if (greaterCard == null) {
			SetSortingLayerName("Draw");
		} else {
			SetSortingLayerName ((greaterCard.GetSortingOrderLayerName ().StartsWith ("Row") ? "Row" + row : "Draw"));

			if (GetSortingOrderLayerName ().Equals ("Draw")) {
				SetSortOrder (greaterCard.GetTopSortOrder () + 3); //set this card to be the layer above the card it's on top of			
			}
		}

		CheckForCompleteStack (); //check if the newly formed stack is complete
	}
	
	///check if we have a complete stack of cards
	void CheckForCompleteStack(){
		//ints to track the end points of our stack
		int highestRank, LowestRank;
		//card object for traversing linked list of cards
		CardSolitaire currentCard = this;
		
		bool[] cardsInStack=new bool[13];
		cardsInStack [rank-1] = true;
		//find greatest card
		while (currentCard.greaterCard!=null) {
			if(cardsInStack[currentCard.rank-1]&&currentCard.greaterCard.LesserRank(currentCard)){
				currentCard=currentCard.greaterCard;
				cardsInStack [currentCard.rank-1] = true;
			}else{break;}
		}
		//assign rank to greatest
		highestRank = currentCard.rank;
		//traverse list again
		currentCard = this;
		//look for least rank
		while (currentCard.lesserCard!=null) {
			if(cardsInStack[currentCard.rank-1]&&currentCard.LesserRank(currentCard.lesserCard)){
				currentCard=currentCard.lesserCard;
			cardsInStack [currentCard.rank-1] = true;
		}else{break;}
		}
		
		//assign to lowest
		LowestRank = currentCard.rank;
		
		if (cardsInStack[0]&&cardsInStack[cardsInStack.Length-1]){//(highestRank == 13 && LowestRank == 1) {
			//move cards out of way and remove stack
			while(currentCard.greaterCard!=null){
				//remove parent link to greater card/stack
				currentCard.transform.parent=null;
				//move card to discard area
				currentCard.Move(Vector3.zero,0.25f);
				//set card to inactive
				currentCard.active=false;
				//select next card
				currentCard=currentCard.greaterCard;
				
			}
			currentCard.active=false;
			currentCard.Move(Vector3.zero,0.25f);
		}
	}
/*
	void OnMouseEnter () {
		if (SpiderSolitaire.S.Card == null) {
			SpiderSolitaire.S.Card = this.gameObject;
		}
	}
*/
	//rejoin all lower cards
	private void RejoinCards(){
		CardSolitaire temp = this;
		while (temp.lesserCard!=null) {
			temp.lesserCard.Join(temp);
			temp=temp.lesserCard;
		}
	}


	public int GetTopSortOrder(){
		//iterate through all sprite renderers and find the lowest sprite renderer sorting order to find the top layer of this card
		/*int topLayer = spriteRenderers[0].sortingOrder;
		foreach (SpriteRenderer tSR in spriteRenderers) {
				topLayer=(tSR.sortingOrder<topLayer?topLayer:tSR.sortingOrder);
		}
		return topLayer;
	*/
		return transform.Find ("back").GetComponent<SpriteRenderer> ().sortingOrder;
	}
	
}