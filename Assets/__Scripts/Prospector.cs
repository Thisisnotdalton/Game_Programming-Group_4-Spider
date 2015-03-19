using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public enum ScoreEvent{
	draw,
	mine,
	mineGold,
	gameWin,
	gameLoss
}

public class Prospector : MonoBehaviour {
	static public Prospector S;
	static public int SCORE_FROM_PREV_ROUND = 0, HIGH_SCORE = 0;
	public Vector3 fsPosMid, fsPosRun, fsPosMid2, fsPosEnd;
	public Deck deck;
	public TextAsset deckXML;

	//ui display
	public Text GTGameOver, GTRoundResult;

	public Layout layout;
	public TextAsset layoutXML;
	public Vector3 layoutCenter;
	public float xOffset = 3, yOffset = -2.5f, reloadDelay=1f;
	public Transform layoutAnchor;


	public CardProspector target;
	
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;
	public List<CardProspector> drawPile;

	//score info
	public int chain = 0, scoreRun = 0, score = 0;
	public FloatingScore fsRun;
	public void Awake(){
		S = this;
		//check for player prefs
		if (PlayerPrefs.HasKey ("ProspectorHighScore")) {
			HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
		}
		//add the score from the previous round, if the player won
		score = SCORE_FROM_PREV_ROUND;
		SCORE_FROM_PREV_ROUND = 0;
		
		GTGameOver = GameObject.Find ("GameOver").GetComponent<Text> ();
		GTRoundResult = GameObject.Find ("RoundResult").GetComponent<Text> ();
	
		ShowResultGTs (false);

		GameObject.Find ("HighScore").GetComponent<Text> ().text = "High Score:" + Utils.AddCommasToNumber (HIGH_SCORE);
	}

	private void ShowResultGTs(bool show){
		GTGameOver.gameObject.SetActive (show);
		GTRoundResult.gameObject.SetActive (show);
	}

	void Start () {
		Scoreboard.S.score = score;
		fsPosMid = new Vector3 (0.5f * Screen.width, 0.90f * Screen.height, 0); 
		fsPosRun = new Vector3 ((Screen.width * 0.5f), (Screen.height * 0.75f), 0);
		fsPosMid2 = new Vector3 (0.5f * Screen.width, 0.5f * Screen.height, 0);
		fsPosEnd = new Vector3(1.0f*Screen.width,0.65f*Screen.height,0);
		deck = GetComponent<Deck> ();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle (ref deck.cards);
		layout = GetComponent<Layout> ();
		layout.ReadLayout (layoutXML.text);

		drawPile = ConvertListCardsToListCardProspectors (deck.cards);
		LayoutGame ();
	}


	public CardProspector Draw(){
		CardProspector cd = drawPile [0];
		drawPile.RemoveAt (0);
		return cd;
	}

	CardProspector FindCardByLayoutID(int layoutID){
		foreach (CardProspector tCP in tableau) {
			if(tCP.layoutID==layoutID){
				return tCP;
			}
		}
		return null;
	}

	public void CardClicked(CardProspector cd){
		switch (cd.state) {
		case CardState.drawpile:
			MoveToDiscard(target);
			MoveToTarget(Draw());
			ScoreManager(ScoreEvent.draw);
			UpdateDrawPile();
			break;
		case CardState.tableau:
			bool validMatch=(cd.faceUp&& AdjacentRank(cd,target));
			if(validMatch){
				tableau.Remove(cd);
				MoveToTarget(cd);
				SetTableauFaces();
				ScoreManager(ScoreEvent.mine);
			}
			break;
		case CardState.target:
			break;
		}
		CheckForGameOver ();
	}

	private void CheckForGameOver(){
		//check if you cleared the tableau
		if (tableau.Count == 0) {
			GameOver (true);
			return;
		}
		//check if you can draw cards
		if (drawPile.Count > 0) {
			return;
		}
		//check if there are valid moves left
		foreach (CardProspector cd in tableau) {
			if(AdjacentRank(cd, target)){
				return;
			}
		}

		GameOver (false);
	}


	private void GameOver(bool win){
				if (win) {
						ScoreManager (ScoreEvent.gameWin);
				} else {
						ScoreManager (ScoreEvent.gameLoss);
				}
		Invoke ("ReloadLevel", reloadDelay);
	}


	private void ReloadLevel(){
				Application.LoadLevel ("_Scene_0");
	}
	private bool AdjacentRank(CardProspector c0, CardProspector c1){
		if(!c0.faceUp||!c1.faceUp){return false;}

		if(Mathf.Abs(c0.rank-c1.rank)==1){
			return true;
		}

		if((c0.rank==1 && c1.rank==13)||(c0.rank==13&&c1.rank==1))return true;

		return false;
	}

	private void MoveToDiscard(CardProspector cd){
		//discard card
		cd.state = CardState.discard;
		discardPile.Add (cd);
		cd.transform.parent = layoutAnchor;
		//cd.transform.localPosition = new Vector3 (layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);
		cd.Move (new Vector3 (layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f), 0.75f);
		cd.faceUp = true;
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortOrder (discardPile.Count - 100);
	}

	private void MoveToTarget(CardProspector cd){
		//if there was already a target, discard it
		if (target != null) {
			MoveToDiscard(target);		
		}
		target = cd;
		cd.state = CardState.target;
		cd.transform.parent = layoutAnchor;
		//cd.transform.localPosition = new Vector3 (layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
		cd.Move (new Vector3 (layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID),0.75f);
		cd.faceUp = true;
		cd.SetSortingLayerName (layout.discardPile.layerName);
		cd.SetSortOrder (0);
	}

	private void SetTableauFaces(){
		foreach (CardProspector cd in tableau) {
			bool fup=true;
			foreach(CardProspector cover in cd.hiddenBy){
				if(cover.state==CardState.tableau){
					fup=false;
				}
			}
			cd.faceUp=fup;
		}
	}

	private void UpdateDrawPile(){
		CardProspector cd;
		//iterate over draw pile
		for (int i = 0; i < drawPile.Count; i++) {
			cd=drawPile[i];
			cd.transform.parent=layoutAnchor;
			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(
				layout.multiplier.x*(layout.drawPile.x+i*dpStagger.x),
				layout.multiplier.y*(layout.drawPile.y+i*dpStagger.y),
				layout.drawPile.layerID+0.1f*i);
			cd.faceUp=false;
			cd.state=CardState.drawpile;
			//set sort order
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10*i);
		}
	}
	private void LayoutGame(){
		//if there is no empty anchor object, make one
		if (layoutAnchor == null) {
			GameObject tGO = new GameObject("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}
		CardProspector cp;
		//follow the layout
		foreach (SlotDef tSD in layout.slotDefs) {
			cp = Draw();
			cp.faceUp = tSD.faceUp;
			cp.transform.parent = layoutAnchor;
			cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y,-tSD.layerID);
			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			cp.state = CardState.tableau;
			cp.SetSortingLayerName(tSD.layerName);
			tableau.Add(cp);
		}

		//set which cards are hiding others
		foreach (CardProspector tCP in tableau) {
			foreach(int hid in tCP.slotDef.hiddenBy){
				cp=FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
			}		
		}
		//draw a card and update pile
		MoveToTarget (Draw ());
		UpdateDrawPile ();
	}

	private List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD){
		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;
		foreach(Card tCD in lCD){
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}
		return lCP;
	}

	//score manager to handle score events
	void ScoreManager(ScoreEvent sEvt){
		List<Vector3> fsPts;
		switch (sEvt) {
		//if the player draws a card, loses, or wins, their combo ends.
		case ScoreEvent.draw:
		case ScoreEvent.gameWin:
		case ScoreEvent.gameLoss:
			chain = 0;
			score+= scoreRun;
			scoreRun=0;
			if(fsRun!=null){
				fsPts = new List<Vector3>();
				fsPts.Add(fsPosRun);
				fsPts.Add(fsPosMid2);
				fsPts.Add(fsPosEnd);
				fsRun.reportFinishTo=Scoreboard.S.gameObject;
				fsRun.Init(fsPts,0,1);
				//adjust font size
				fsRun.fontSizes=new List<float>(new float[]{28,36,4});
				fsRun = null;
			}
			break;
		//increase chain and scoreRun
		case ScoreEvent.mine:
			chain++;
			scoreRun+=chain;
			//create a FloatingScore object for this run
			FloatingScore fs;
			Vector3 p0 = Input.mousePosition;
			fsPts = new List<Vector3>();
			fsPts.Add(p0);
			fsPts.Add(fsPosMid);
			fsPts.Add(fsPosRun);
			fs = Scoreboard.S.CreateFloatingScore(chain,fsPts);
			if(fsRun==null){
				fsRun=fs;
				fsRun.reportFinishTo=null;
			}else{
				fs.reportFinishTo=fsRun.gameObject;
			}
			break;
		}

		//handle game loss/win
		switch (sEvt) {
		case ScoreEvent.gameWin:
			//if we won, add score to next round
			Prospector.SCORE_FROM_PREV_ROUND=score;
			//update and display the ui to tell the player how they did
			GTGameOver.text="Round Over";
			GTRoundResult.text="You won the round with "+score+" points";
			ShowResultGTs(true);
			break;
		case ScoreEvent.gameLoss://you lose, check for high score
			GTGameOver.text="Game Over";
			if(Prospector.HIGH_SCORE<=score){
				GTRoundResult.text="You got the high score! High Score:"+score;
				Prospector.HIGH_SCORE=score;
				PlayerPrefs.SetInt("ProspectorHighScore",HIGH_SCORE);
			}else{
				GTRoundResult.text="Final Score:"+score;
			}
			ShowResultGTs(true);
			break;
		}
	}

}
