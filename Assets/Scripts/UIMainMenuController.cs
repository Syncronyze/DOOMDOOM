using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMainMenuController : MonoBehaviour{
    public static UIMainMenuController instanceOf;
    public Slider volumeSlider;
    public UIFontController volumeDisplay;
    public Material greyscaleImageMaterial;
    public UIEndScreenController endMenu;

    public GameObject loadButton;
    public Transform[] loadGroup;
    public Transform[] newSaveGroup;

    public GameObject parentOfAllMenus;
    public GameObject menuMain;
    public GameObject pauseMenuBack;
    public GameObject backToMainMenu;

    public GameObject menuBG;

    public bool isEnabled{ get; private set; }

    void Awake(){
        if(instanceOf == null){
            instanceOf = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(this);
            return;
        }
    }

    void Start(){
        float volumeValue = PlayerPrefs.GetFloat("Volume");

        if(volumeSlider != null)
            volumeSlider.value = volumeValue;
        
        if(volumeDisplay != null)
            volumeDisplay.SetValue(volumeValue + "");

        GlobalPlayerVariables.LoadAll();
        UpdateLoadButtons();
        UpdateNewSaveButtons();
        isEnabled = true;
        EnableMenu(menuMain);
        pauseMenuBack.gameObject.SetActive(false);
    }

    public void TogglePauseMenu(){
        isEnabled = !isEnabled;
        Time.timeScale = isEnabled ? 0.0f : 1.0f;
        MenuNavigation(isEnabled);
        gameObject.SetActive(isEnabled);
        EnableMenu(menuMain);
        pauseMenuBack.gameObject.SetActive(isEnabled);
    }

    public void EnableMenu(GameObject menuToEnable, bool enableBackToMain = false){
        int menuCount = parentOfAllMenus.transform.childCount;

        for(int i = 0; i < menuCount; i++){
            parentOfAllMenus.transform.GetChild(i).gameObject.SetActive(false);
        }

        backToMainMenu.gameObject.SetActive(enableBackToMain);
        menuToEnable.gameObject.SetActive(true);
    }

    public void BackToMainMenu(){
        EnableMenu(menuMain, false);
    }

    
    void MenuNavigation(bool isOn){
        this.gameObject.SetActive(isOn);
        Cursor.lockState = isOn ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void UpdateLoadButtons(){
        if(GlobalPlayerVariables.savesExist){
            loadButton.GetComponent<Button>().interactable = true;
            UIFontController loadButtonFont = loadButton.GetComponent<UIFontController>();
            loadButtonFont.ChangeFontMaterial(null);
            loadButtonFont.ForceRefreshText();

            for(int i = 0; i < loadGroup.Length; i++){
                if(GlobalPlayerVariables.saves[i] != null){
                    Button loadButton = loadGroup[i].GetComponent<Button>();
                    UIFontController controller = loadButton.transform.GetComponent<UIFontController>();
                    loadButton.interactable = true;
                    controller.ChangeFontMaterial(null);
                    controller.SetValue("Save 0" + (i + 1));
                }
            }
        }
    }

    void UpdateNewSaveButtons(){
        for(int i = 0; i < newSaveGroup.Length; i++){
            if(GlobalPlayerVariables.saves[i] != null){
                newSaveGroup[i].GetComponent<UIFontController>().SetValue("Save 0" + (i + 1));
            }
        }
    }

    void StartGame(){
        if(GlobalPlayerVariables.save == null){
            Debug.LogError("PlayerVariables weren't loaded for this save- cannot continue this operation.");
            return;
        }
        menuBG.SetActive(false);
        TogglePauseMenu();
        LoadingScreenController.instanceOf.Loading(true, SceneManager.LoadSceneAsync(GlobalPlayerVariables.save.level));
    }

    public void LoadGame(int saveIndex){
        GlobalPlayerVariables.Load(saveIndex);
        StartGame();
    }

    public void NewGame(int saveIndex){
        GlobalPlayerVariables.New(saveIndex);
        UpdateLoadButtons();
        UpdateNewSaveButtons();
        StartGame();
    }

    public void EndLevel(){
        MenuNavigation(true);
        GlobalPlayerVariables.Save();
        menuBG.SetActive(true);
        if(GlobalLevelVariables.instanceOf == null){
            EnableMenu(menuMain);
        }
        else{
            EnableMenu(endMenu.gameObject, true);
            StartCoroutine(endMenu.BeginAnimations());
        }
    }

    public void VolumeChange(){
        if(volumeSlider == null || volumeDisplay == null)
            return;
        
        float value = Mathf.Round(volumeSlider.value * 100);
        PlayerPrefs.SetFloat("Volume", value);
        volumeDisplay.SetValue("" + value);
    }

    public void ENDTHEMISERY(){
        Application.Quit();
    }
}
