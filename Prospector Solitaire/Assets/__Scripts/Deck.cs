using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [Header("Set in Inspector")]
    public bool startFaceUp = false;
    //Palos
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpades;

    public Sprite[] faceSprite;
    public Sprite[] rankSprite;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    //Prefabas
    public GameObject prefabCard;
    public GameObject prefabSprite;

    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    //InitDeck es llamado por Prospector cuando esta por comenzar
    public void InitDeck(string deckXMLText)
    {
       
        //Esto crea el anchor de todas las cartas GameObject 
        if (GameObject.Find("_Deck") == null) {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        //Inicia el diccionario dictSuits
        dictSuits = new Dictionary<string, Sprite>() {
           {"C", suitClub },
           {"D", suitDiamond },
           {"H", suitHeart },
           {"S", suitSpades }
        };
        ReadDeck(deckXMLText);
        MakeCards();
    }

    //ReadDeck analiza el archivo XML que le entrega CardDefinition
    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader(); //Crea un nuevo PT_XMLReader
        xmlr.Parse(deckXMLText);//Usa el PT_XMLReader para analizar el texto

        //string s = "xml[0] decorator[0] ";
        //s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        //s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        //s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        //s += " z=" + xmlr.xml["xml"][0]["decorator"][0].att("z");
        //print(s);

        //Lee los decorators de todas las cartas
        decorators = new List<Decorator>();
        //Toma un PT_XMLHashList de todos los <decorators> ene l archivo XML
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for (int i = 0; i < xDecos.Count; i++)
        {
            //Por cada <decorator> en el XML
            deco = new Decorator();
            //Copia los atributos del <decorator> en deco
            deco.type = xDecos[i].att("type");
            //bool deco.flip es verdadero si el texto de flip es "1"
            deco.flip = (xDecos[i].att("flip") == "1");
            //floats necesitan ser analizados ya que son String
            deco.scale = float.Parse(xDecos[i].att("scale"));
            //Vector3 loc se inizialiso en [0,0,0], entonces solo se necesita ajustar con el XML
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            //Añade el deco temporal a la lista de decorators
            decorators.Add(deco);
        }

        //Lee los simbolos de adentro de la carta (pip) de cada carta con numero
        cardefs = new List<CardDefinition>();
        //Toma un PT_XMLHashList por todas las cartas en el archivo XML
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefs.Count; i++)
        {
            //Por cada <card> en el XML
            CardDefinition cDef = new CardDefinition();
            //Copia los atributos del <card> en cDef
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            //Toma un PT_XMLHashList para todos los <pip> de esta carta
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    //Por cada <pip>s en el XML
                    deco = new Decorator();
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                }
            }
            //Si es una carta con cara (J,Q,K) tiene el atributo de cara
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }
            cardefs.Add(cDef);
        }
    }

    //busca la definicion apropieada en base al rango (1 al 14)
    public CardDefinition GetCardDefinitionByRank(int rnk) {
        //Busca por todos los CardDefinition
        foreach (CardDefinition cd in cardefs) {
            //Si el rango es correcto, devuelve la definicion
            if (cd.rank == rnk) {
                return (cd);
            }
        }
        return null;
    }

    //Crea el GameObject de la carta
    public void MakeCards() {
        //cardName sera el nmbre de la carta (C1 al C14 para Clubs (Trboles))
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters){
            for (int i = 0; i < 13; i++){
                cardNames.Add(s + (i + 1));
            }
        }

        //Se hace una lista para contener todas las cartas
        cards = new List<Card>();

        //Recorre por todos los nombre de las cartas que se han creado
        for (int i = 0; i < cardNames.Count; i++){
            //Hace la carta y la añade al Deck de cartas
            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum) {
        //Crea una nueva Card Gameobject
        GameObject cgo = Instantiate(prefabCard) as GameObject;
        //Pone el tranform.parent como el anchor
        cgo.transform.parent = deckAnchor;
        //Se recoge el Componenete Card de la carta
        Card card = cgo.GetComponent<Card>();
        //Esta linea hace que se acomoden de una mejor forma
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        //Asigna los valores basicos a la Carta
        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if (card.suit == "D" || card.suit == "H") {
            card.colS = "Red";
            card.color = Color.red;
        }
        //Obtiene la CardDefinition de esta carta
        card.def = GetCardDefinitionByRank(card.rank);
        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    //Estas variables privadas se usaran bastante para la comodidad
    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card) {
        //Añade los decorators
        foreach (Decorator deco in decorators){
            if (deco.type == "suit"){
                //Crea el Sprite GameObject
                _tGO = Instantiate(prefabSprite) as GameObject;
                //Toma el Sprite Component
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                //Pone el Sprite apropiado al palo
                _tSR.sprite = dictSuits[card.suit];
            }
            else {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                //Toma el sprite para el rango apropiado (A-K)
                _tSp = rankSprite[card.rank];
                //Asigna el Sprite de Rango al SpriteRenderer+
                _tSR.sprite = _tSp;
                //Pone el color apopiado al sprite
                _tSR.color = card.color;
            }
            //Hace el deco una capa mas arriba de la carta
            _tSR.sortingOrder = 1;
            //Hace al Decorator Srpite Hijo de la carta
            _tGO.transform.SetParent(card.transform);
            //Pone la posicion basado en el archivo XML
            _tGO.transform.localPosition = deco.loc;
            //Flip el decorator si es necesdario
            if (deco.flip) {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            //Ajusta la escala para evitar que los decos sean muy grandes
            if (deco.scale != 1) {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }
            //Nombrar a este GameObject para que sea facil de ver
            _tGO.name = deco.type;
            //Añade este deco GameObject a la lista
            card.decoGos.Add(_tGO);
        }
    }

    private void AddPips(Card card) {
        //Por cada pip en la CardDefinition
        foreach  (Decorator pip in card.def.pips){
            //Instaciar un Sprite GameObject
            _tGO = Instantiate(prefabSprite) as GameObject;
            //Pone de pariente a la carta GameObject
            _tGO.transform.SetParent(card.transform);
            //Poner la posicion dicha por el Xml
            _tGO.transform.localPosition = pip.loc;
            //Rotar si es necesario
            if (pip.flip) {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (pip.scale != 1) {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }
            //Se le da un nombre a este GameObject
            _tGO.name = "pip";
            //Toma el SpriteRenderer del GameObject
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            //Aplicar el Sprite render inidcado al palo
            _tSR.sprite = dictSuits[card.suit];
            //Aplica el sortingOrder para que el pip se renderise encima del card front
            _tSR.sortingOrder = 1;
            //Añade esta carta a la lista
            card.pipGOs.Add(_tGO);
        }
    }

    private void AddFace(Card card) {
        if (card.def.face == "") {
            return;
        }

        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        //Tomar el nombre verdadero para darcelo a GetFace
        _tSp = GetFace(card.def.face + card.suit);
        _tSR.sprite = _tSp; //asigna el Sprite a a _tSR
        _tSR.sortingOrder = 1;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tGO.name = "face";
    }

    private void AddBack(Card card) {
        //Añade el reverso de la carta
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = cardBack;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        //Este tiene el sorting Order mas alto
        _tSR.sortingOrder = 2;
        _tGO.name = "back";
        card.back = _tGO;
        card.faceUP = true;
    }

    //Encuentra el Sprite de cara apropiada 
    private Sprite GetFace(string faceS) {
        foreach (Sprite _tSP in faceSprite){
            if (_tSP.name == faceS) {
                return (_tSP);
            }
        }
        return null;
    }

    //Mezcla las cartas en Deck.cards
    static public void Shuffle(ref List<Card> oCards) {
        //Crea una lista temporal para mantener el nuevo orden
        List<Card> tCards = new List<Card>();
        int ndx; //Este mantendra el index de las cartas que se moveran
        tCards = new List<Card>();
        while (oCards.Count > 0) {
            //Toma el index de una carta aleatoria
            ndx = Random.Range(0, oCards.Count);
            //Añade esa carta a la lista temporal
            tCards.Add(oCards[ndx]);
            //Y eliminia la carta de la lista original
            oCards.RemoveAt(ndx);
        }
        //Remplazar la lista origianl no la lista temporal
        oCards = tCards;
        //oCards cuenta como la lista original por el (ref) asi que de esta froma queda bien la lista de cartas
    }
}
