using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDef {
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int LayerID = 0;
    public int id;
    public List<int> hiddenBY = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour {
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;//El offset de la tabla
    //SlotDef renderece
    public List<SlotDef> slotDefs;//Todos los Slotdef de Row0 a Row3
    public SlotDef drawPile;
    public SlotDef discardPile;
    //Este mantiene todos los posibles nombres de los layers, empieza en 1
    public string[] sortingLayerNames = new string[] {
        "Row0", "Row1", "Row2", "Row3", "Discard", "Draw"
    };

    //Esta funcion se llama para leer el LayoutXML.xml
    public void ReadLayour(string xmlText) {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        //Lee en el multiplicador, el cual pone el espaciado de las cartas
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        //Lee en los slots
        SlotDef tSD;
        //slotsX se usa como una accesibilidad para tolods los slots de <slots>
        PT_XMLHashList slotsX = xml["slot"];
        for (int i = 0; i < slotsX.Count; i++){
            tSD = new SlotDef();
            if (slotsX[i].HasAtt("type")){
                tSD.type = slotsX[i].att("type");
            }
            else {
                tSD.type = "slot";
            }
            //Varios atributos se analizan en datos numericos
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.LayerID = int.Parse(slotsX[i].att("layer"));
            //Esto convierte el numero de Layer Id en un text Layer
            tSD.layerName = sortingLayerNames[tSD.LayerID];
            print(tSD.layerName + " " + tSD.LayerID);
            switch (tSD.type) {
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby")){
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding) {
                            tSD.hiddenBY.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                default:
                    print(tSD.LayerID);
                    break;
            }
        }
    }
}
