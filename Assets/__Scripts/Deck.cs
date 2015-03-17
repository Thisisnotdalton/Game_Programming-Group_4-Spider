using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour
{
		//sprites
		public Sprite suitClub, suitDiamond, suitHeart, suitSpade;
		public Sprite[] faceSprites, rankSprites;
		public Sprite cardBack, cardFront;

		//prefabs
		public GameObject prefabSprite, prefabCard;

		//other
		private PT_XMLReader xmlr;
		private List<string> cardNames;
		public List<Card> cards;
		private List<Decorator> decorators;
		private List<CardDefinition>	cardDefs;
		private Transform deckAnchor;
		private Dictionary<string,Sprite> dictSuits;

		public void InitDeck (string deckXMLText)
		{
				//create an anchor for the deck
				if (GameObject.Find ("_Deck") == null) {
						GameObject anchorGO = new GameObject ("_Deck");
						deckAnchor = anchorGO.transform;
				}

				//initialize the dictionary of suitSprites with correct Sprites
				dictSuits = new Dictionary<string, Sprite> (){
			{"C", suitClub},
			{"D",suitDiamond},
			{"H",suitHeart},
			{"S",suitSpade}
		};




				ReadDeck (deckXMLText);
				MakeCards ();
		}
		
	static public void Shuffle(ref List<Card> oCards){
		//empty list of cards to hold new card order
		List<Card> tCards = new List<Card> ();
		//temp index for card to be moved
		int index; 
		//keep shuffling until the cards are shuffled
		while (oCards.Count>0) {
			index = Random.Range (0, oCards.Count);
			tCards.Add(oCards[index]);
			oCards.RemoveAt(index);
		}

		//replace the old deck
		oCards = tCards;
	}



		public void ReadDeck (string deckXMLText)
		{
				xmlr = new PT_XMLReader ();
				xmlr.Parse (deckXMLText);
				decorators = new List<Decorator> ();
				PT_XMLHashList xDecos = xmlr.xml ["xml"] [0] ["decorator"];
				Decorator deco;

				for (int i = 0; i<xDecos.Count; i++) {
						deco = new Decorator ();
						deco.type = xDecos [i].att ("type");
						deco.flip = (xDecos [i].att ("flip") == "1");
						deco.scale = float.Parse (xDecos [i].att ("scale"));
						deco.loc.x = float.Parse (xDecos [i].att ("x"));
						deco.loc.y = float.Parse (xDecos [i].att ("y"));
						deco.loc.z = float.Parse (xDecos [i].att ("z"));
						decorators.Add (deco);
				}

				cardDefs = new List<CardDefinition> ();
				PT_XMLHashList xCardDefs = xmlr.xml ["xml"] [0] ["card"];

				for (int i = 0; i < xCardDefs.Count; i++) {
						CardDefinition cDef = new CardDefinition ();
						cDef.rank = int.Parse (xCardDefs [i].att ("rank"));
						PT_XMLHashList xPips = xCardDefs [i] ["pip"];
						if (xPips != null) {
								for (int j = 0; j<xPips.Count; j++) {
										deco = new Decorator ();
										deco.type = "pip";
										deco.flip = (xPips [j].att ("flip") == "1");
										deco.loc.x = float.Parse (xPips [j].att ("x"));
										deco.loc.y = float.Parse (xPips [j].att ("y"));
										deco.loc.z = float.Parse (xPips [j].att ("z"));
										if (xPips [j].HasAtt ("scale")) {
												deco.scale = float.Parse (xPips [j].att ("scale"));
										}
										cDef.pips.Add (deco);
								}

						}

						if (xCardDefs [i].HasAtt ("face")) {
								cDef.face = xCardDefs [i].att ("face");
						}
						cardDefs.Add (cDef);

				}
	
		}

		public CardDefinition GetCardDefinitionByRank (int rank)
		{
				foreach (CardDefinition cd in cardDefs) {
						if (cd.rank == rank) {
								return cd;
						}
				}
				return null;
		}

		private void MakeCards ()
		{
				//cardNames to hold the names of cards to build
				cardNames = new List<string> ();
				string[] letters = new string[]{"C","D","H","S"};
				foreach (string s in letters) {
						for (int i = 1; i <14; i++) {
								cardNames.Add (s + i);
						}
				}

				//create a list to hold the objects
				cards = new List<Card> ();

				//reusable temp vars for card making
				Sprite tS = null;
				GameObject tGO = null;
				SpriteRenderer tSR = null;

				for (int i=0; i<cardNames.Count; i++) {
						GameObject cgo = Instantiate (prefabCard) as GameObject;
						cgo.transform.parent = deckAnchor;
						Card card = cgo.GetComponent<Card> ();

						cgo.transform.localPosition = new Vector3 ((i % 13) * 3, i / 13 * 4, 0);

						card.name = cardNames [i];
						card.suit = card.name [0].ToString ();
						card.rank = int.Parse (card.name.Substring (1));
						if (card.suit == "D" || card.suit == "H") {
								card.colS = "Red";
								card.color = Color.red;
						}

						//get the card's CardDefinition
						card.def = GetCardDefinitionByRank (card.rank);


						//add the decorators
						foreach (Decorator deco in decorators) {
								if (deco.type == "suit") {//if this decorator is a suit
										//instantiate a new sprite
										tGO = Instantiate (prefabSprite) as GameObject;
										//get its rendder component
										tSR = tGO.GetComponent<SpriteRenderer> ();
										//give sprite the correct suit
										tSR.sprite = dictSuits [card.suit];
								} else {//the decorator is a rank
										tGO = Instantiate (prefabSprite) as GameObject;
										tSR = tGO.GetComponent<SpriteRenderer> ();
										//get the correct rank sprite
										tS = rankSprites [card.rank];
										tSR.sprite = tS;
										tSR.color = card.color;
								}
								//make sure decorators have the right layering order
								tSR.sortingOrder = 1;
								//arent this to the card
								tGO.transform.parent = cgo.transform;
								//set local position from XML
								tGO.transform.localPosition = deco.loc;
								//flip decorator as needed
								if (deco.flip) {
										//flip it
										tGO.transform.rotation = Quaternion.Euler (0, 0, 180);
								}

								//scale down deco as needed
								if (deco.scale != 1) {
										tGO.transform.localScale = Vector3.one * deco.scale;
								}
								//name this decorator for easy finding later
								tGO.name = deco.type;
								//add complete deco to list for card
								card.decGOs.Add (tGO);
						}
			
						//add the pips
						foreach (Decorator pip in card.def.pips) {
								//instantiate a game object for it
								tGO = Instantiate (prefabSprite) as GameObject;
								//parent it to the card
								tGO.transform.parent = cgo.transform;
								//set the position
								tGO.transform.localPosition = pip.loc;
								//flip as needed
								if (pip.flip) {
										tGO.transform.rotation = Quaternion.Euler (0, 0, 180);
								}
								//scale as needed
								if (pip.scale != 1) {
										tGO.transform.localScale = Vector3.one * pip.scale;
								}
								tGO.name = "pip";
								//get SpriteRender
								tSR = tGO.GetComponent<SpriteRenderer> ();
								tSR.sprite = dictSuits [card.suit];
								tSR.sortingOrder = 1;
								card.pipGOs.Add (tGO);
						}

						//take care of face cards
						if (card.def.face != "") {
								tGO = Instantiate (prefabSprite) as GameObject;
								tSR = tGO.GetComponent<SpriteRenderer> ();
								tS = GetFace (card.def.face + card.suit);
								tSR.sprite = tS;
								tSR.sortingOrder = 1;
								//parent it to the card
								tGO.transform.parent = cgo.transform;
								//set the position
								tGO.transform.localPosition = Vector3.zero;
								tGO.name = "face";

						}
						//add card back
						tGO = Instantiate (prefabSprite) as GameObject;
						tSR = tGO.GetComponent<SpriteRenderer> ();
						tSR.sprite = cardBack;
						
						tGO.transform.parent = card.transform;
						tGO.transform.localPosition = Vector3.zero;
						tSR.sortingOrder = 2;
						tGO.name="back";
						card.back = tGO;


						//flip the card over
						card.faceUp = false;

						//add complete card to deck
						cards.Add (card);
				}




		}

		private Sprite GetFace (string faceS)
		{
				foreach (Sprite tS in faceSprites) {
						if (tS.name == faceS) {
								return tS;
						}
				}		
				return null;
		}
}
