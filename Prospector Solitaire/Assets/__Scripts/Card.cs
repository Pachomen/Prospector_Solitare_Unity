using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour{
    [Header("Set Dynamically")]
    public string suit; //Palo de la carta (C, H; D, o S)
    public int rank; //rank of the card (1-14)
    public Color color = Color.black; //Color para pintar los pip
    public string colS = "Black"; // o Red dependiendo del simbolo

    //Esta lista mantiene todos los deocrators del GameObject
    public List<GameObject> decoGos = new List<GameObject>();
    //Esta lista mantiene todos los pips del GameObject
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back; //El Gameobject del respaldo de la carta

    public CardDefinition def;
    //Lista de los SpriteRenderer Components de este Gameobject
    public SpriteRenderer[] spriteRenderers;

    void Start(){
        SetSortOrder(0);//Asegurarse que la carta empieza en el punto inicial
    }

    //Si es el SpriteRenderer no esta definido aun, esta funcion lo define
    public void PopulateSpriteRenderers() {
        //Si spriteRenderers es null o vacio
        if (spriteRenderers == null || spriteRenderers.Length == 0){
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    //Asigna el sortingLayerName en todos los sprite Renders components
    public void SetSortingLayerName(string tSLN) {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers){
            tSR.sortingLayerName = tSLN;
        }

    }

    //Pone el sortingOrder en todos los SpriteRenderer Components
    public void SetSortOrder(int sOrd) {
        PopulateSpriteRenderers();
        //Recorre todos los SpriteRenderers como tSR
        foreach (SpriteRenderer tSR in spriteRenderers){
            if (tSR.gameObject == this.gameObject){
                tSR.sortingOrder = sOrd;
                continue;
            }
            //Cada hijo de este GameObject esta nombrado
            //hacer un switche n base al nombre
            switch (tSR.gameObject.name) {
                case "back"://Si el nombre es back
                            //Ponerlo en el layer mas alto para cubrir los otros
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case "Card_Front":
                    GameObject tGO = tSR.gameObject;
                    if (tGO.name == "Card_Front") {
                        Vector3 tv = tGO.transform.position;
                        tv.z = -0.0001f;
                        tGO.transform.position = tv;
                        tv = Vector3.zero;
                    }
                break;
                case "face"://Si tiene el nombre de face
                default://O si es otra cosa
                    //Ponerlo en la mitad de todo pero arriba del fondo
                    tSR.sortingOrder = sOrd + 1;
                break;

            }
        }
    }

    public bool faceUP {
        get {
            return (!back.activeSelf);
        }
        set {
            back.SetActive(!value);
        }
    }

    virtual public void OnMouseUpAsButton() {
        print(name);
    }
}

[System.Serializable] //Una clase Seiañizable se puede  cambiar ene el Inspector
public class Decorator {
    //Esta clase guardara la informacion sobre cada decorator y pip de DeckXML
    public string type; //El tipo de este simbolo
    public Vector3 loc; //Posicicon del sprite en la carta
    public bool flip = false; //Si se tiene que voltear el o no el simbolor
    public float scale = 1f;//La scala del Sprite
}

[System.Serializable]
public class CardDefinition {
    //Esta clase guarda la informacion de cada rango de la carta
    public string face;//Spri a usar para cada  cara de la carta
    public int rank; //El rango (1-13) de esta carta
    public List<Decorator> pips = new List<Decorator>(); //Simbolos a usar
}