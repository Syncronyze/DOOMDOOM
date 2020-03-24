using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController instanceOf;

    private bool isLoading;
    private AsyncOperation loadOperation;

    [SerializeField]
    public Slider progressBar;

    [SerializeField]
    public UIFontController progressValue;

    float time;
    float minTime = 0.5f;

    void Awake(){
        if(instanceOf == null){
            instanceOf = this;
            DontDestroyOnLoad(gameObject);
            Loading(false, null);
        }
        else{
            Destroy(this);
            return;
        }
    }

    void Update(){
        if(!isLoading)
            return;
        
        time += Time.deltaTime;
        UpdateProgress(loadOperation.progress);
        if(loadOperation.isDone && time > minTime)
            Loading(false, null);
    }

    void UpdateProgress(float progress){
        progressBar.value = progress;
        progressValue.SetValue(Mathf.RoundToInt(progress * 100) + "%");
    }

    public void Loading(bool _isLoading, AsyncOperation toLoad){
        if(toLoad == null)
            _isLoading = false;

        isLoading = _isLoading;
        gameObject.SetActive(isLoading);
        loadOperation = toLoad;
        UpdateProgress(0f);
        time = 0;
    }
}
