using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScript : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;

    [Header("Tips")]
    [SerializeField] private Text tip1;
    [SerializeField] private Text tip2;



    public void LoadLevelBtn(int sceneID)
    {
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);

        StartCoroutine(LoadLevelAsync(sceneID));
    }

    IEnumerator LoadLevelAsync(int sceneID)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);

        while (!operation.isDone) {
            float progressValue = Mathf.Clamp01(operation.progress / 0.9f);

            loadingSlider.value = progressValue;

            if(progressValue < 0.5f)
            {
                tip2.gameObject.SetActive(false);
            }
            else
            {
                tip1.gameObject.SetActive(false);
                tip2.gameObject.SetActive(true);
            }

            yield return null;
        }
    }
}
