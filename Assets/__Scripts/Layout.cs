using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public class SlotDef{
	public float x,y;
	public bool faceUp=false;
	public string layerName="default", type="slot";
	public int layerID=0,id;
	public List<int> hiddenBy = new List<int> ();
	public Vector2 stagger;
}


public class Layout : MonoBehaviour {
	
	public PT_XMLReader xmlr; //to read the slot layout xml
	public PT_XMLHashtable xml;
	public Vector2 multiplier; //sets the spacing of the tableau
	//references for SlotDef
	public List<SlotDef> slotDefs;
	public SlotDef drawPile,discardPile;
	//list of all possible names for the layers
	public string[] sortingLayerNames = new string[]{"Row0","Row1","Row2","Row3","Discard","Draw"};
	
	public void ReadLayout(string xmlText){
		xmlr = new PT_XMLReader ();
		xmlr.Parse (xmlText);//parse the xml file
		xml = xmlr.xml ["xml"] [0]; //shortcut for ease of use
		
		//read multiplier for card spacing
		multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
		multiplier.y = float.Parse(xml["multiplier"][0].att("y"));
		
		//read in the slots
		SlotDef tSD;
		//shortcut to slots
		PT_XMLHashList slotsX = xml ["slot"];
		
		for(int i = 0; i < slotsX.Count; i++){
			tSD = new SlotDef();
			if (slotsX[i].HasAtt("type")) {
				// If this <slot> has a type attribute parse it
				tSD.type = slotsX[i].att("type");
			} else {
				// If not, set its type to "slot"; it's a tableau card 
				tSD.type = "slot";
			}
			//get position
			tSD.x = float.Parse( slotsX[i].att("x") );
			tSD.y = float.Parse( slotsX[i].att("y") );
			//get layer info
			tSD.layerID = int.Parse( slotsX[i].att("layer") );
			tSD.layerName = sortingLayerNames[ tSD.layerID ];
			
			switch(tSD.type){
			case "slot":
				tSD.faceUp = (slotsX[i].att("faceup")=="1");
				tSD.id =  int.Parse(slotsX[i].att("id"));
				if(slotsX[i].HasAtt("hiddenby")){
					string[] hiding = slotsX[i].att("hiddenby").Split(',');
					foreach(string s in hiding){
						tSD.hiddenBy.Add(int.Parse(s));
					}
				}
				slotDefs.Add(tSD);
				break;
			case "drawpile":
				tSD.stagger.x=float.Parse(slotsX[i].att("xstagger"));
				drawPile = tSD;
				break;
			case "discardpile":
				discardPile=tSD;
				break;
			}
			
			
			
		}
	}
}
