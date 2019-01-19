using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//enum define una varible con algunos nombre-prescritos
public enum eCardState {
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card{
    [Header("Set Dynamically: CardProspector")]
    //Aca se va a usar el enum eCardState
    public eCardState state = eCardState.drawpile;
    //El hiddenby es una lista que guarda que otras cartas estan por encima que la mantienen boca abajo
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    //El LayoutID usa esta carta para poscicionarla con respecto al XML 
    public int layoutID;
    //El SlotDef guarda la informacion que llega de Layout XML
    public SlotDef slotDef;
    //Esto permite que la carta reaccione al ser click
    public override void OnMouseUpAsButton(){
        //Llama a CardClicker del Prospector 
        Prospector.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }
}
