using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prospector : MonoBehaviour{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    void Awake(){
        S = this;    
    }

    void Start(){
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        //Toma el Layout Component
        layout.ReadLayour(layoutXML.text);
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD) {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD) {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return (lCP);
    }
    //La funcion Draw tomara una carta de la pila de cartas
    CardProspector Draw() {
        CardProspector cd = drawPile[0];//Saca la carta en la posicion 0
        drawPile.RemoveAt(0);//Quita la carta de la lista
        return (cd);//Y la devuelve
    }

    //LayoutGame da la posicion inicial del tablero, y posiciona las cartas enmcima de este
    void LayoutGame() {
        //Crea un EmptyGameObject que funciona como anchor del tablero
        if (layoutAnchor == null) {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        CardProspector cp;
        //Sigue el Layout
        foreach (SlotDef tSD in layout.slotDefs){
            cp = Draw();//Saca una carta de arriba para el inicio
            cp.faceUP = tSD.faceUp;//Voltea la carta 
            cp.transform.parent = layoutAnchor;//Lo hace pariente
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.LayerID);
            //^Pone el localPosition en la definicion del slot
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);
            tableau.Add(cp);
        }

        //Poner que carta esta cubriendo a que carta
        foreach (CardProspector tCP in tableau) {
            foreach (int hid in tCP.slotDef.hiddenBY){
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        //Se pone un Target Inicial
        MoveToTarget(Draw());
        //Pone la pila donde recoge cartas
        UpdateDrawPile();
    }

    //Conviertte con el LayoutId a la CardProspector correspondiente
    CardProspector FindCardByLayoutID(int layoutID) {
        foreach (CardProspector tCP in tableau){
            //Busca dentro de todas las cartas en el tableu List<>
            if (tCP.layoutID == layoutID) {
                return (tCP);
            }
        }
        return null;
    }

    //Este metodo voltea las cartas
    void SetTableuFaces() {
        foreach(CardProspector cd in tableau){
            bool faceUp = true; //Se asume que la carta estara boca-arriba
            foreach (CardProspector cover in cd.hiddenBy){
                //si alguno de las cartas que lo cubren estan
                if (cover.state == eCardState.tableau) faceUp = false;
            }
            cd.faceUP = faceUp;

        }
    }

    //Mueve el objetivo actual a la pila de descarte
    void MoveToDicard(CardProspector cd) {
        //Pone el state de la carta en discard
        cd.state = eCardState.discard;
        discardPile.Add(cd);//Añade la carta a la lista de descarte
        cd.transform.parent = layoutAnchor; //Acutaliza el tranform

        //Posciicona esta carta en la pila de descarte
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.LayerID + 0.5f);
        cd.faceUP = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    // Hace de cd el nuevo objetivo
    void MoveToTarget(CardProspector cd) {
        //Si existe un Target, lo mueve la pila de descarte
        if (target != null) MoveToDicard(target);
        target = cd;// Es el nuevo Target
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;
        //Mueve a la posicicon de target
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.LayerID);

        cd.faceUP = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    //Toma todas las cartas de la drawPile y muestra cuentas quedan
    void UpdateDrawPile() {
        CardProspector cd;
        //Va dentro de todas las cartass en la pila
        for (int i = 0; i < drawPile.Count; i++){
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            //Lo posiciona
            Vector2 dpSragger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpSragger.x),layout.multiplier.y * (layout.drawPile.y + i * dpSragger.y),-layout.drawPile.LayerID + 0.1f*i);
            cd.faceUP = false;
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    //Esta funcion es llamada cada vez que una carta es clickeada
    public void CardClicked(CardProspector cd) {
        switch (cd.state) {
            case eCardState.target:
                //Hacer click en el target no hace nada
                break;
            case eCardState.drawpile:
                //Hacer click en cualquier carta que este dentro de la pila de recoger dara una carta
                MoveToDicard(target);//Mueve el Target a la pila de descarte
                MoveToTarget(Draw());//Mueve el nuevo objetivo desde la pilade de recoger
                UpdateDrawPile();//Actuliza la pila de de recogar con las cartas
                break;
            case eCardState.tableau:
                //Hacer click dentro del tablero revisara si la carta es mayor o menor en 1
                bool validMatch = true;
                if (!cd.faceUP) {
                    //Si la carta esta boca-abajo no es valido
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target)) {
                    //Si no es un rango adyacente arriba o abajo, no es valido
                    validMatch = false;
                }
                if (!validMatch) return;//Devuelve si validMatch = false
                //Si pasa entonces entonces es una carta valida
                tableau.Remove(cd);//Elimina la carta de la lista de tableu
                MoveToTarget(cd);//Lo hace el nuevo Target
                SetTableuFaces();
                break;
        }
        //Revisar si no es game over o no
        CheckForGameOver();
    }

    //Devuelve si es dos cartas son adyacentes en rango
    public bool AdjacentRank(CardProspector c0, CardProspector c1) {
        //si alguna de las carta esta boca-abajo, entonces no son adjacentes
        if (!c0.faceUP || !c1.faceUP) return (false);

        //Si el valor absoluto de su resta en rango es 1, entonces son adjacentes
        if (Mathf.Abs(c0.rank - c1.rank) == 1) return (true);

        //si uno es un As y otro un Rey, entonces tambien son adyacentes
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c1.rank == 1 && c0.rank == 13) return (true);

        //Si nada es deovlver falso
        return (false);
    }

    void CheckForGameOver() {
        //Si tableu esta vacio, es game over
        if (tableau.Count == 0) {
            //Llama el Game over con una victoria
           GameOver(true);
            return;
        }
        //Si aun tiene cartas en la pila de recoger, no es game over
        if (drawPile.Count > 0) {
            return;
        }

        //Revisar por jugadas validas
        foreach (CardProspector cd in tableau){
            if (AdjacentRank(cd, target)) return;
        }
        //Como no existe ninguna jugada valida, game over
        GameOver(false);
    }

    //Se llama a GameOver
    void GameOver(bool won) {
        if (won) {
            print("Game Over, GANASTE <|^O^<|");
        }
        else {
            print("Game Over, Perdiste :c");
        }
        SceneManager.LoadScene("__Prospector_Scene_0");
    }
}
