using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck3D : MonoBehaviour {
	//data for deck
	public TextAsset deckXML;

	//materials for planes
	public Material suitClub, suitDiamond, suitHeart, suitSpade;
	public Texture[] faceTextures;
	public Sprite rankMats;
	
	//prefabs
	public GameObject prefabPlane, prefabCard;
	
	//other
	private PT_XMLReader xmlr;
	private List<string> cardNames;
	public List<Card3D> cards;
	private List<Decorator> decorators;
	private List<CardDefinition>	cardDefs;
	private Transform deckAnchor;
	private Dictionary<string,Material> dictSuits;

	public void Start(){
		InitDeck (deckXML.text);
		Shuffle (ref cards);
	}


	public void InitDeck (string deckXMLText)
	{
		//create an anchor for the deck
		if (GameObject.Find ("_Deck") == null) {
			GameObject anchorGO = new GameObject ("_Deck");
			deckAnchor = anchorGO.transform;
		}
		
		//initialize the dictionary of suitSprites with correct Sprites
		dictSuits = new Dictionary<string, Material> (){
			{"C", suitClub},
			{"D",suitDiamond},
			{"H",suitHeart},
			{"S",suitSpade}
		};
		
		
		
		
		ReadDeck (deckXMLText);
		MakeCards ();
	}
	
	static public void Shuffle(ref List<Card3D> oCards){
		//empty list of cards to hold new card order
		List<Card3D> tCards = new List<Card3D> ();
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
		cards = new List<Card3D> ();
		
		//reusable temp vars for card making
		GameObject tGO = null;
		
		for (int i=0; i<cardNames.Count; i++) {
			GameObject cgo = Instantiate (prefabCard) as GameObject;
			cgo.transform.parent = deckAnchor;
			Card3D card = cgo.GetComponent<Card3D> ();
			
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
					tGO = Instantiate (prefabPlane) as GameObject;
					//give sprite the correct suit
					tGO.renderer.material = dictSuits [card.suit];
				} else {//the decorator is a rank
					tGO = Instantiate (prefabPlane) as GameObject;
					//get the correct rank sprite
					//tS = rankSprites [card.rank];
					tGO.renderer.material.color = card.color;
					Texture2D tex = new Texture2D((int)rankMats.textureRect.height,(int)rankMats.textureRect.height);
					tex.SetPixels(rankMats.texture.GetPixels((card.rank)*(int)rankMats.textureRect.height,0,(int)rankMats.textureRect.height,(int)rankMats.textureRect.height));
					tex.Apply();
					tGO.renderer.material.mainTexture=tex;
				}
				//arent this to the card
				tGO.transform.parent = cgo.transform;
				//set local position from XML
				//tGO.transform.localPosition = deco.loc;2.75f*
				tGO.transform.localPosition = new Vector3(-deco.loc.x*tGO.transform.localScale.magnitude*cgo.transform.localScale.magnitude,tGO.transform.localScale.y,-deco.loc.y*tGO.transform.localScale.magnitude*cgo.transform.localScale.magnitude);
				
				tGO.transform.localScale = new Vector3 (2.5f*deco.scale*tGO.transform.localScale.x,tGO.transform.localScale.y,2.5f*deco.scale*tGO.transform.localScale.z);
				//tGO.transform.localPosition = new Vector3(-deco.loc.x/2*prefabPlane.transform.localScale.x/2,tGO.transform.localScale.y,-deco.loc.y/2*prefabPlane.transform.localScale.z/2);
				//flip decorator as needed
				tGO.transform.localRotation=Quaternion.identity;

				if (deco.flip) {
					//flip it
					tGO.transform.Rotate(0,180,0);
				}

				//scale down deco as needed
				//if (deco.scale != 1) {
				//tGO.transform.localScale = new Vector3 (2.75f*deco.scale*tGO.transform.localScale.x,tGO.transform.localScale.y,2.75f*deco.scale*tGO.transform.localScale.z);

				//}
				//name this decorator for easy finding later
				tGO.name = deco.type;
				//add complete deco to list for card
				card.decGOs.Add (tGO);
			}
			
			//add the pips
			foreach (Decorator pip in card.def.pips) {
				//instantiate a game object for it
				tGO = Instantiate (prefabPlane) as GameObject;
				//parent it to the card
				tGO.transform.parent = cgo.transform;
				//set the position
				tGO.transform.localPosition = new Vector3(-pip.loc.x*cgo.transform.localScale.magnitude*tGO.transform.localScale.magnitude,tGO.transform.localScale.y/2,-pip.loc.y*cgo.transform.localScale.magnitude*tGO.transform.localScale.magnitude);//flip as needed
				tGO.transform.localScale = new Vector3 (5f*pip.scale*tGO.transform.localScale.x,tGO.transform.localScale.y,5f*pip.scale*tGO.transform.localScale.z);

				tGO.transform.localRotation=Quaternion.identity;

				if (pip.flip) {
					tGO.transform.Rotate(0,180,0);
				}

				//scale as needed
				//if (pip.scale != 1) {
				//}
				tGO.name = "pip";

				//get SpriteRender
				tGO.renderer.material = dictSuits [card.suit];
				card.pipGOs.Add (tGO);
			}
			
			//take care of face cards
			if (card.def.face != "") {
				tGO = Instantiate (prefabPlane) as GameObject;
				tGO.renderer.material.mainTexture = GetFace (card.def.face + card.suit);
				//parent it to the card
				tGO.transform.parent = cgo.transform;
				//set the position
				tGO.transform.localRotation=Quaternion.identity;
				tGO.transform.localScale=new Vector3(cgo.transform.localScale.x/2*tGO.transform.localScale.x,tGO.transform.localScale.y,cgo.transform.localScale.z*tGO.transform.localScale.z);
				tGO.transform.localPosition = new Vector3(0,tGO.transform.localScale.y,0);
				tGO.name = "face";
				
			}else{
			}
			
			
			//flip the card over
			//card.faceUp = false;
			
			//add complete card to deck
			cards.Add (card);
		}
		
		
		
		
	}

	private Texture GetFace (string faceS)
	{
		foreach (Texture tT in faceTextures) {
			if (tT.name == faceS) {
				return tT;
			}
		}		
		return null;
	}
}