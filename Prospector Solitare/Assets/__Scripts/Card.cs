using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour{
    [Header("Set Dynamically")]
    public string suit; //Palo de las cartas (Picas, Diamantes, Corazones o Treboles)
    public int rank;//Rango de la cara (1-14)
    public Color color = Color.black; //Color que se pintara el simbolo
    public string colS = "Black";//o "Red", Nombre del color

    //Esta lista almacena todas los Decoretor GameObjects
    public List<GameObject> decoGOs = new List<GameObject>();
    //Esta lista contiene solo los Pips GameObjects
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back;//El GameObject de la parte de atras de la carta
    public CardDefinition def;//Analisao de DeckXML.xml
}

[System.Serializable]//Es posible esitarlo ene el Inspector

public class Decorator {
    //Esta clase guarda toda la informacion sobre el edecorador de las cartas
    public string type;//Para los simbolos de la cartas 
    public Vector3 loc;//La ubicacion del Sprite en ela carta
    public bool flip = false;//cuando toca voltear el Sprite de manera vertical
    public float scale = 1f;//La scala del Sprite

}

[System.Serializable]
public class CardDefinition {
    //Esta clase guarda la informacion del rango de cada carta
    public string face; //El Srpite para usar en cada cara de la carta
    public int rank; //El rango de (1-13) de estas cartas
    public List<Decorator> pips = new List<Decorator>();//Simbolos a usar
}

