using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//an enum to handle all the possible scoring events
public enum ScoreEvent{
	draw,
	mine,
	mineGold,
	gameWin,
	gameLoss
}

public class SpiderSolitaire : MonoBehaviour {
	static public SpiderSolitaire S;
	static public int SCORE_FROM_PREV_ROUND=0;
	static public int HIGH_SCORE = 0;
	
	public float reloadDelay = 1f;//the delay between rounds
	
	public Vector3 fsPosMid = new Vector3 (.5f, .9f, 0);
	public Vector3 fsPosRun=new Vector3(.5f,.75f,0);
	public Vector3 fsPosMid2 = new Vector3 (.5f, .5f, 0);
	public Vector3 fsPosEnd = new Vector3 (1.0f, .65f, 0);
	
	public Deck deck;
	public TextAsset deckXML;
	
	public Layout layout;
	public TextAsset layoutXML;
	public Vector3 layoutCenter;
	public float xOffset=3;
	public float yOffset = -2.5f;
	public Transform layoutAnchor;

	public float speed = 1.5f;
	private Vector3 cardPos;
	
	public CardSolitaire target;
	public List<CardSolitaire> tableau;
	public List<CardSolitaire> discardPile;
	public List<CardSolitaire> drawPile;
	public GameObject Card;

	public CardSolitaire[] firstRowOfCards;
	public Transform[] slotPositions;
	void Awake(){
		S = this; //Set up a Singleton for Prospector
		
	}
	
	//Fields to track score info
	public int chain=0;//of cards in this run
	public int scoreRun=0;
	public int score=0;
	public FloatingScore fsRun;
	
	void Start(){
		cardPos = transform.position;
		deck = GetComponent<Deck> ();//Get the Deck
		deck.InitDeck (deckXML.text);//Pass DeckXML to it
		Deck.Shuffle (ref deck.cards);//this shuffles the deck
		//the ref keyword passes a reference to deck.cards, which allows deck.cards to be modified by Deck.Shuffle()
		
		layout = GetComponent<Layout> ();//get the layout
		layout.ReadLayout (layoutXML.text);//pass LayoutXML to it
		drawPile = ConvertListCardsToListCardSolitaires (deck.cards);
		LayoutGame ();
	}

	void Update (){
		if (Input.GetMouseButton(0)) {
			cardPos = Input.mousePosition;

			cardPos = Camera.main.ScreenToWorldPoint(cardPos);
			if (Card != null) {
				cardPos.z = Card.transform.position.z;
				Card.transform.position = cardPos;
			}
		}
		/*
		if (Input.GetMouseButtonUp (0)) {
			Card = null;
		}*/
	}
	

	//the draw function will pull a single card from the drawPile and return it
	CardSolitaire Draw(){
		CardSolitaire cd = drawPile [0];//pull the 0th CardSolitaire
		drawPile.RemoveAt (0);//then remove it from List<> drawPile
		return(cd);//and return it
	}
	
	//convert from the layoutID int to the CardSolitaire with that ID
	CardSolitaire FindCardByLayoutID(int layoutID){
		foreach (CardSolitaire tCP in tableau) {
			//search through all cards in the tableau List<>
			if (tCP.layoutID == layoutID) {
				//if the card has the same ID, return it
				return(tCP);
			}
		}
		//if it's not found, return null
		return(null);
	}
	
	//LayoutGame() positions the initial tableau of cards, a.k.a. the "mine"
	void LayoutGame(){
		//Create an empty GameObject to serve as an anchor for the tableau
		if (layoutAnchor == null) {
			GameObject tGO=new GameObject("_LayoutAnchor");
			//^ Create an empty GameObject named _LayoutAnchor in the Hierarchy
			layoutAnchor=tGO.transform;//grab its Tranform
			layoutAnchor.transform.position=layoutCenter;//position it
		}
		
		CardSolitaire cp;
		//initialize first row of cards array
		slotPositions = new Transform[10];
		firstRowOfCards = new CardSolitaire[slotPositions.Length];
		int slotCards = 0;
		//follow the layout
		foreach (SlotDef tSD in layout.slotDefs) {
			//^Iterate through all the SlotDefs in the layout.slotDefs as tSD
			cp=Draw ();//pull a card from the top (beginning) of the drawPile
			if(tSD.type=="slot"){
				firstRowOfCards[slotCards]=cp;
				slotPositions[slotCards]=cp.transform;
				cp.faceUp=false;
				slotCards++;
			}
			cp.faceUp=tSD.faceUp;//set its faceUp to the value in slotDef
			cp.transform.parent=layoutAnchor;//make its parent layoutAnchor
			//this replaces the previous parent: deck.deckAnchor, which appears as _Deck in the Hierarchy when the scene is playing.
			cp.transform.localPosition=new Vector3(layout.multiplier.x*tSD.x,layout.multiplier.y*tSD.y,-tSD.layerID);
			//^Set the localPosition of the card based on slotDef
			cp.layoutID=tSD.id;
			cp.slotDef=tSD;
			cp.state=CardState.tableau;
			//CardSolitaires in the tableau have the state CardState.tableau
			
			cp.SetSortingLayerName(tSD.layerName);//set the sorting layers
			
			tableau.Add(cp);//add this CardSolitaire to the List<> tableau
		}

		
		/*
		//Set which cards are hiding others
		foreach (CardSolitaire tCP in tableau) {
			foreach(int hid in tCP.slotDef.hiddenBy){
				cp=FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
			}
		}
		*/

		//add other cards to stack
		for (int i = 0; i < 3 * firstRowOfCards.Length + 4; i++) {
			//get bottom of current card
			CardSolitaire bottomCard=firstRowOfCards[i%firstRowOfCards.Length].BottomOfStack();
			//flip this card over
			bottomCard.faceUp=false;
			//add this card to that stack
			CardSolitaire tempCard = Draw();
			//tempCard.transform.Find("back").GetComponent<SpriteRenderer>().sortingLayerName = bottomCard.GetSortingOrderLayerName();
			tempCard.SetSortingLayerName(bottomCard.GetSortingOrderLayerName());
			//set its row
			tempCard.Join(bottomCard);
		} 


		//set up the Draw pile
		UpdateDrawPile ();
	}
	
	List<CardSolitaire> ConvertListCardsToListCardSolitaires(List<Card> lCD){
		List<CardSolitaire> lCP=new List<CardSolitaire>();
		CardSolitaire tCP;
		foreach (Card tCD in lCD) {
			tCP = tCD as CardSolitaire;
			lCP.Add (tCP);
		}
		return(lCP);
	}


	//cardClicked is called any time a card in the game is clicked
	public void CardClicked(CardSolitaire cd){
		//the reaction is determined by the stated of the clicked card
		switch (cd.state) {
		case CardState.target:
			//clicking the target card does nothing
			break;
		case CardState.drawpile:
			//clicking any card in the drawPile will draw the nesxt card
			MoveToDiscard(target);//moves the target to the discard pile
			UpdateDrawPile();//Restacks the drawPile
			ScoreManager(ScoreEvent.draw);
			break;
		case CardState.tableau:
			//clicking a card in the tableau will check if it's a valid play
			bool validMatch=true;
			if(!cd.faceUp){
				//if the card is face-down, it's not valid
				validMatch=false;
			}
			if(!AdjacentRank(cd,target)){
				//if it's not an adjacent rank, it's not valid
				validMatch=false;
			}
			if(!validMatch)return;//return if not valid
			//yay!  it's a valid card.
			tableau.Remove(cd);//Remove it from the tableau List
			SetTableauFaces();//update tableau card face-ups
			ScoreManager(ScoreEvent.mine);
			break;
		}
		//check to see wheether the game is over or not
		CheckForGameOver ();
	}
	
	//moves the current target to the discardPile
	void MoveToDiscard(CardSolitaire cd){
		//set the state of the card to discard
		cd.state = CardState.discard;
		discardPile.Add (cd);//add it to the discardPile List<>
		cd.transform.parent = layoutAnchor;//update its transform parent
		cd.transform.localPosition = new Vector3 (layout.multiplier.x * layout.discardPile.x,
		                                          layout.multiplier.y * layout.discardPile.y,
		                                          -layout.discardPile.layerID + 0.5f);
		//^position it on the discard pile
		cd.faceUp = true;
		//Place it on top of the pile for depth sorting
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortOrder (-100 + discardPile.Count);
	}
	
	//arranges allthe cards of the drawPile to show how many are left
	void UpdateDrawPile(){
		CardSolitaire cd;
		//go through all the cards of the drawPile
		for (int i=0; i<drawPile.Count; i++) {
			cd=drawPile[i];
			cd.transform.parent=layoutAnchor;
			//position it correctly with the layout.drawPile.stagger
			Vector2 dpStagger=layout.drawPile.stagger;
			cd.transform.localPosition=new Vector3(layout.multiplier.x*(layout.drawPile.x+i*dpStagger.x),
			                                       layout.multiplier.y*(layout.drawPile.y+i*dpStagger.y),
			                                       -layout.drawPile.layerID+0.1f*i);
			cd.faceUp=false;//make them all face-down
			cd.state=CardState.drawpile;
			//set depth sorting
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10*i);
		}
	}
	
	//Return true if the two cards are adjacent in rank (A & K wrap around)
	public bool AdjacentRank(CardSolitaire c0,CardSolitaire c1){
		//if either card is face-down, it's not adjacent
		if (!c0.faceUp || !c1.faceUp)
			return(false);
		//if they are 1 apart, they are adjacent
		if (Mathf.Abs (c0.rank - c1.rank) == 1) {
			return(true);
		}
		//if one is A and the other King, they're adjacent
		if (c0.rank == 13 && c1.rank == 1)
			return(true);
		if (c0.rank == 1 && c1.rank == 13)
			return(true);
		//Otherwise, return false
		return(false);
	}
	
	//this turns cards in the Mine fadce-up or face-down
	void SetTableauFaces(){
		foreach (CardSolitaire cd in tableau) {
			bool fup=true;//assume the card will be face-up
			foreach(CardSolitaire cover in cd.hiddenBy){
				//if either of the covering cards are in the tableau
				if(cover.state==CardState.tableau){
					fup=false;//then this card is face-down
				}
			}
			cd.faceUp=fup;//set the value on the card
		}
	}
	
	//test whether the game is over
	void CheckForGameOver(){
		//if the tableau is empty, the game is over
		if (tableau.Count == 0) {
			//call GameOver() with a win
			GameOver (true);
			return;
		}
		//if there are still cards in the draw pile, the game's not over
		if (drawPile.Count > 0) {
			return;
		}
		//check for remaining valid plays
		foreach(CardSolitaire cd in tableau){
			if(AdjacentRank(cd, target)){
				//if there is a valid play, the game's not over
				return;
			}
		}
		//since there are no valid plays, the game is over
		//call GameOver() with a loss
		GameOver (false);
	}
	
	//called when the game is over. simple for now, but expandable
	void GameOver(bool won){
		if (won) {
			ScoreManager(ScoreEvent.gameWin);
		} else {
			ScoreManager(ScoreEvent.gameLoss);
		}
		//reload the scene in reloadDelay seconds
		//this will give the score a moment to travel
		Invoke ("ReloadLevel", reloadDelay);
		
	}
	
	void ReloadLevel(){
		//reload the scene, resetting the game
		Application.LoadLevel ("__Prospector_Scene_0");
	}
	
	//ScoreManager handles all of the scoring 
	void ScoreManager(ScoreEvent sEvt){
		List<Vector3> fsPts;
		switch (sEvt) {
			//same things need to happen whether it's a draw, a win, or a loss
		case ScoreEvent.draw://drawing a card
		case ScoreEvent.gameWin:
		case ScoreEvent.gameLoss:
			chain=0;//resets the score chain
			score+= scoreRun;//add scorRun to total score
			scoreRun=0;//resets scoreRun
			//add fsRun to the _Scoreboard score
			if(fsRun !=null){
				//create points for the Bezier curve
				fsPts = new List<Vector3>();
				fsPts.Add(fsPosRun);
				fsPts.Add(fsPosMid2);
				fsPts.Add(fsPosEnd);
				fsRun.reportFinishTo=Scoreboard.S.gameObject;
				fsRun.Init(fsPts,0,1);
				//Also adjust the fontSize
				fsRun.fontSizes=new List<float>(new float[] {28,36,4});
				fsRun=null;//clear fsRun so it's created again
			}
			break;
		case ScoreEvent.mine://remove min card
			chain++;//increase score chain
			scoreRun+=chain;//add score for this card to run
			//create a FloatingScore for this score
			FloatingScore fs;
			//Move it from the mousPosition to fsPosRun
			Vector3 p0=Input.mousePosition;
			p0.x /=Screen.width;
			p0.y /=Screen.height;
			fsPts=new List<Vector3>();
			fsPts.Add(p0);
			fsPts.Add(fsPosMid);
			fsPts.Add(fsPosRun);
			fs=Scoreboard.S.CreateFloatingScore(chain,fsPts);
			fs.fontSizes=new List<float>(new float[] {4,50,28});
			if(fsRun==null){
				fsRun=fs;
				fsRun.reportFinishTo=null;
			}else{
				fs.reportFinishTo=fsRun.gameObject;
			}
			break;
		}
		
	}
}
