using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
  *      For use to recreate the Doom Font with 100% accuracy based on the image scaling within Unity.
  *      Removes the necessity to interact with the FontControllers directly, and we only need a reference
  *      to this object to successfully update UI Text.
  */
public class GameUIController : MonoBehaviour
{
    List<UIFontController> fontControllers;

    void Awake(){
        CollectUIControllers();
    }

    public bool HasControllers(){
        return fontControllers != null && fontControllers.Count > 0;
    }

    /*
     * Collecting all of the UI Controllers within the child elements and storing them in a list for further use
     */
    public void CollectUIControllers(){
        fontControllers = new List<UIFontController>(transform.GetComponentsInChildren<UIFontController>());
    }

    /*
     * Gets the requested font controller; returns null if it doesn't exist
     */
    public UIFontController GetFontController(string UIElementName){
        if(!HasControllers()){
            Debug.LogWarning("There is no controllers, this likely isn't attached to the right gameObject!");
            return null;
        }
        
        for (int i = 0; i < fontControllers.Count; i++){
            UIFontController fc = fontControllers[i];
            if(fc.name == UIElementName)
                return fc;
        }

        return null;
    }

    /*
     * Setting or changing the value on the font controller requested.
     */
    public void SetValue(string UIElementName, string value){
        UIFontController fc = GetFontController(UIElementName);

        if(fc == null){
            Debug.LogWarning($"Cannot set {value} on font controller {UIElementName}, as it doesn't exist or cannot be found.");
            return;
        }

        fc.SetValue(value);
    }

    /*
     * Changing the style of the font on the font controller requested.
     */
    public void ChangeStyle(string UIElementName, FontStyle style){
        UIFontController fc = GetFontController(UIElementName);

        if(fc == null){
            Debug.LogWarning($"Cannot set font style {style} on font controller {UIElementName}, as it doesn't exist or cannot be found.");
            return;
        }
        fc.ChangeFontStyle(style);

    }
}
