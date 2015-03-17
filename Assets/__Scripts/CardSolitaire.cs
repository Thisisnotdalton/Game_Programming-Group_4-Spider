using UnityEngine;
using System.Collections;

public class CardSolitaire : Card {
	//these refer to the card above or below this one in rank for this stack
	private CardSolitaire lesserCard,greaterCard;

	//boolean to determine if the card is still active/moveable
	private bool active=true;




	//Returns true if the other card may be placed on this card, i.e. the other card is a lesser adjacent rank
	bool LesserRank (CardSolitaire other) {
		return (other.rank == this.rank - 1);
	}

	//Split this card from the stack it's resting on while still keeping track of the others
	void Split(){
		//unlink card above from this card
		greaterCard.lesserCard = null;
		//unlink it from the card above it
		greaterCard = null;
		//remove the parenting of this card from the greater card
		transform.parent = null;
	}

	//Join this card to a stack with a card of higher rank
	void Join(CardSolitaire other){
		//link greater card to us
		other.lesserCard = this;
		//link ourselves to the greater card
		greaterCard = other;
		//set this card to be a child of the previous card
		transform.parent = greaterCard.transform;
	}

	///check if we have a complete stack of cards
	void CheckForCompleteStack(){
		//ints to track the end points of our stack
		int highestRank, LowestRank;
		//card object for traversing linked list of cards
		CardSolitaire currentCard = this;
		//find greatest card
		while (currentCard.greaterCard!=null) {
			currentCard=currentCard.greaterCard;
		}
		//assign rank to greatest
		highestRank = currentCard.rank;
		//traverse list again
		currentCard = this;
		//look for least rank
		while (currentCard.lesserCard!=null) {
			currentCard=currentCard.lesserCard;
		}
		//assign to lowest
		LowestRank = currentCard.rank;

		if (highestRank == 13 && LowestRank == 1) {
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

}
