using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour{
    [Header("Set in Inpsector")]
    //Palos
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprite;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;
    //Prefabs
    public GameObject prefabCard;
    public GameObject prefabSprite;


    [Header("Set Dynamically")]
    public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;
    //InitDeck es llamado cuando el Prospecto esta listo
    public void InitDeck(string deckXMLText) {
        //Esto crea un anchor para todas Card GameObject
        if (GameObject.Find("_Deck") == null) {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        //Inicializa el diccionario de SuitSprites con los Spretes necesarios
        dictSuits = new Dictionary<string, Sprite>() {
            {"C", suitClub },
            {"D", suitDiamond},
            {"H", suitHeart},
            {"S", suitSpade}
        };
        ReadDeck(deckXMLText);

        MakeCards();
    }

    //ReadDeck analiza el documento XML dentro de CardDefiniton
    public void ReadDeck(string deckXMLText) {
        xmlr = new PT_XMLReader(); //Crea un nuevo PT_XMLReader
        xmlr.Parse(deckXMLText);//Se usa el PT_XMLReader para anailzar el documento

        //Esto imprime una linea de prueba para mostrar como se puede usar xmlr
        string s = "xml[0] decorator[0] ";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);

        //Leer las decoraciones de todas la cartas
        decorators = new List<Decorator>();//Se inicia la lista de decoraciones
        //Se obtiene la PT_XMLHashList de toso los decoretors en el archivo XML
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for (int i = 0; i < xDecos.Count; i++){
            //Por cada <decorator> en el XML
            deco = new Decorator();//crea un nuevo decorator
            //Copia los atributos del <decorator> al nuevo Decorator
            deco.type = xDecos[i].att("type");
            //bool deco.flip es true si el texto del atributo flip es 1
            deco.flip = (xDecos[i].att("flip") == "1");
            //el float necesita ser analizado desde el atributo del string
            deco.scale = float.Parse(xDecos[i].att("scale"));
            //Vector 3 loc esta inicializado en [0,0,0], solo se necesita dumar las coordenadas
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            //Añadir el deco temporal a la lista de decorator
            decorators.Add(deco);
        }

        //Leer la localizacion del simbolo por cada numero de carta
        cardDefs = new List<CardDefinition>();//Inicializa la lista de cartas
        //Toma una PT_XMLHashList por todas las <cards> en el documento XML
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
        for (int i = 0; i < xCardDefs.Count; i++){
            //Por cada <card>
            //Crea una nueva definicion de CardDefiniton
            CardDefinition cDef = new CardDefinition();
            //Analiza los valores del atributo y los añade al cDef
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));
            //Toma un PT_XMLHashList de todos los <pip> en esta <card>
            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null){
                for (int j = 0; j <xPips.Count ; j++){
                    //Itera dentro de todos <pip>s
                    deco = new Decorator();
                    //<pip>s en <card> seran tomados por Decorator
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale")) {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                }
            }
            //Cartas con cara como (Jack,Queen, & King) obitnene el atributo de cara
            if (xCardDefs[i].HasAtt("face")) {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }

    }

    //Tomar la CardDefinition Apropiada en base al rango 
    public CardDefinition GetCardDefinitionByRank(int rnk) {
        //Buscar atraves de todas las CardDefinition
        foreach (CardDefinition cd in cardDefs) {
            //Si es rango es correcto, devuvle la definicion
            if (cd.rank == rnk) {
                return (cd);
            }
        }
        return (null);
    }
    //Hacer GameObjects de Card
    public void MakeCards() {
        //cardNames sera el nombre de la carta a construir
        //Cada palo ira de 1 a 14 (ej. C1 a C14 son treboles)
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters){
            for (int i = 0; i < 13; i++){
                cardNames.Add(s + (i + 1));
            }
        }
        //Hacer una lista para guardar todas las cartas
        cards = new List<Card>();

        //Iterar a traves de todos los nombres de las cartas que apenas se crearon
        for (int i = 0; i < cardNames.Count; i++){
            //Hacer la carta y agragarla al Deck
            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum) {
        //Crea una Card GameObject
        GameObject cgo = Instantiate(prefabCard) as GameObject;
        //Le da el tranform.parent a las nuevas cartas del anchor
        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();//Toma el card Component

        //Esta linea apila las cartas para que se va mejor
        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13);
        //Asigan los valores basicos a las cartas
        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));
        if (card.suit == "D" || card.suit == "h") {
            card.colS = "Red";
            card.color = Color.red;
        }
        //Encuentra la CardDefinition para esta carta
        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        return card;
    }
    //Estas variables privadas se reusara varias veces para ayudar
    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card) {
        //Añade el Decorators
        foreach (Decorator deco in decorators) {
            if (deco.type == "suit") {
                //instancia el Sprite GameObject
                _tGO = Instantiate(prefabSprite) as GameObject;
                //Toma el SpriteRenderer Component
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                //Pone El Sprite al palo apropiado
                _tSR.sprite = dictSuits[card.suit];
            }
            else {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                //Toma el Sprite apropiado para mostrar el rango
                _tSp = rankSprites[card.rank];
                //Asigna este Sprite de rango al SrpiteRenderer
                _tSR.sprite = _tSp;
                //Pone el color del rango que se iguala al palo
                _tSR.color = card.color;
            }
            //Hace el deco Sprites render por encima de la carta
            _tSR.sortingOrder = 1;
            //Hace el Decoretor Srpite un pariente de la carta
            _tGO.transform.SetParent(card.transform);
            //Escribe el localPosition basado en la localizacion del DeckXML
            _tGO.transform.localPosition = deco.loc;
            //Flip el decorator si es necesario
            if (deco.flip) {
                //una rotacion de Euler de 180° al rededor del eje-Z lo voletrar
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            //Escribe la escala a mantener los decos para evitar que sean muy grandes
            if (deco.scale != 1) {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }
            //Nombra este GameObject para poderlo ver facilmente
            _tGO.name = deco.type;
            //Añade este deco GameObject a la lista card.decoGOs
            card.decoGOs.Add(_tGO);
        }
    }
}
