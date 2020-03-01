using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFontController : MonoBehaviour
{
    public FontStyle _style;
    public bool fromRight;
    public GameObject imagePrefab;
    UISpriteLoader spriteLoader;
    public float letterSpacing;
    [Min(0.01f)]
    public float letterScaling = 3;
    public bool caseInsensitive = true;
    public string defaultValue;

    //[HideInInspector]
    //public string objectName;

    string value;
    //private float lastUpdate;
    bool update;
    bool valid;

    void Start(){
        spriteLoader = GameObject.FindGameObjectWithTag("SpriteLoader").GetComponent<UISpriteLoader>();
        valid = spriteLoader.LoadSpriteSheet("HUD_Font", "Textures/HUD_Font");
        if(!string.IsNullOrEmpty(defaultValue))
            SetValue(defaultValue);
    }

    void Update(){
        //lastUpdate += Time.deltaTime;

        // only updating if we need to
        if(valid && update)
            UpdateText();
        
    }

    /*
     * Force refresh on the characters
     */
    public void Refresh(){
        update = true;
    }

    /*
     * Updates the onscreen text.
     */
    void UpdateText(){
        update = false;
        //lastUpdate = 0;
        HUDFont[] characters = FromStringToEnum(value, _style);
        
        if(characters == null)
            return;
        // if we're doing right to left, the characters must be reversed in order to not appear backwards
        if(fromRight)
            Array.Reverse(characters);

        SyncImageAmount(characters.Length);

        float startPos = 0;

        for(int i = 0; i < characters.Length; i++){
            Transform child = transform.GetChild(i);
            Image image = child.GetComponent<Image>();
            RectTransform rt = child.GetComponent<RectTransform>();
            Sprite sprite;
            HUDFont currentChar = characters[i];

            if(image == null)
                continue;

            // setting anchor on the right side if we're starting from the right (numbers and such)
            if(fromRight){
                rt.anchorMin = new Vector2(1, 0.5f);
                rt.anchorMax = new Vector2(1, 0.5f);
            }
            else{
                rt.anchorMax = new Vector2(0, 0.5f);
                rt.anchorMin = new Vector2(0, 0.5f);
            }

            // defined whitespace size by 0, as all HUDFonts have a 0 and is reasonble size to become a space.
            if(currentChar == HUDFont.WhiteSp){
                 if(!Enum.TryParse(_style.ToString() + "0", out currentChar))
                    continue;
                // disabling the image so we don't see a 0 in place of where white space should exist.
                image.enabled = false;
            }
            else{
                image.enabled = true;
            }

            
            sprite = spriteLoader.RetrieveSprite("HUD_Font", "HUD_Font_" + (int)currentChar);

            if(sprite == null)
                continue;

            image.sprite = sprite;
            // multiplying the rect by letter scaling so characters aren't too small
            rt.sizeDelta = new Vector2(sprite.rect.size.x * letterScaling, sprite.rect.size.y * letterScaling);
            // position on the x axis is defined by the center of the image
            float xPos = (rt.sizeDelta.x / 2) + startPos;
            rt.localScale = new Vector3(1, 1, 1); 

            if(fromRight)
                xPos *= -1;

            rt.anchoredPosition = new Vector2(xPos, 0);

            // shifting the start position of each letter by the last + the spacing
            startPos += rt.sizeDelta.x + letterSpacing;
        }        
        
    }

    /*
     * Synchronizes the number of image children we have to the character length requested.
     */
    void SyncImageAmount(int characterLength){
        if(transform.childCount > characterLength)
            DisableChildren(transform.childCount - characterLength);
        else if(transform.childCount < characterLength)
            CreateChildren(characterLength - transform.childCount);
            
    }

    /*
     * Creating any missing children we might be needing
     */
    void CreateChildren(int amountOfChildren){
        for(int i = 0; i < amountOfChildren; i++){
            var child = Instantiate(imagePrefab, new Vector3(0, 0, 0), Quaternion.identity);
            child.transform.SetParent(this.transform);
        }
    }

    /*
     * Disabling children instead of deleting any unused ones, as deleting is an asynchronous operation meaning it can lead to issues, and it's generally faster to just disable versus delete/create.
     */
    void DisableChildren(int amountOfChildren){
        for(int i = 0; i < transform.childCount; i++){
            Image toHide;
            int childIndex = (transform.childCount - 1) - i;

            //if(!fromRight)
            //childIndex = ;

            if(transform.GetChild(childIndex).TryGetComponent<Image>(out toHide)){
                toHide.enabled = false;
                amountOfChildren--;
            }

            if(amountOfChildren == 0)
                return;
        }
        

    }

    /*
     * Converts each of the characters into the given enumerators
     */
    HUDFont[] FromStringToEnum(string input, FontStyle style = FontStyle.SR){
        if(input == null)
            return null;

        char[] charArray = input.ToCharArray();
        HUDFont[] returnArray = new HUDFont[charArray.Length];
        for(int i = 0; i < charArray.Length; i++){
            string charName = charToName(charArray[i]);
            HUDFont result;
            // if we cannot TryParse, and it fails, we add a ? in place of that value
            // unless that font doesn't contain a ?, in which case we're going to use a WhiteSp
            if(Enum.TryParse<HUDFont>(style.ToString() + charName, out result))
                returnArray[i] = result;
            else
                returnArray[i] = (HUDFont)Enum.Parse(typeof(HUDFont), "WhiteSp"); // in the event there's no known symbol to match, we simply use whitespace
        }

        return returnArray;
    }

    string charToName(char input){
        if(char.IsWhiteSpace(input))
            return "WhiteSp";

        if(char.IsLetterOrDigit(input)){
            if(caseInsensitive)
                return input.ToString().ToUpper();
            else
                return input.ToString();
        }
            

        // in the event that we have to convert symbols, these are the related and relevant names
        // there is not every symbol, not even close, but these are the ones available.
        switch(input){
            case '-': return "Neg"; // negative/dash & percentage are first, as they're the most likely outside of letters, numbers and whitepsace
            case '%': return "Pcnt";
            case '\'': return "Quo"; 
            case '.': return "Prd";
            case ';': return "SCol";
            case ':': return "Col";
            case '|': return "Pipe";
            case '[': return "SqbL";
            case ']': return "SqbR";
            case '(': return "ParL"; 
            case ')': return "ParR";
            case '=': return "Equ"; 
            case '+': return "Plus"; 
            case '<': return "CarL"; 
            case '>': return "CarR"; 
            case '"': return "Dblq";
            case '#': return "Pnd"; 
            case '$': return "Dol"; 
            case '@': return "At"; 
            case '*': return "Ast"; 
            case '\\': return "Bsl"; 
            case '/': return "Fsl"; 
            case '^': return "Pow"; 
            case '&': return "Amp";
            case '_': return "Undsc"; 
            case '?': return "Qst"; 
            case '!': return "Exc"; 
        }
        // if there's a symbol that is unrecognized, it becomes a questionmark
        return "Qst";
    }

    public void SetValue(string valueToSet){
        if(valueToSet == value)
            return;
        //print("Setting new value on " + gameObject.name + ", value is " + valueToSet);
        value = valueToSet;
        update = true;
    }

    public void ChangeFontStyle(FontStyle newFontStyle){
        _style = newFontStyle;
        Refresh();
    }

    public string GetValue(){
        return value;
    }
}
