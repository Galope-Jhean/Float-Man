using UnityEngine;
using System.Collections;

public class GUIControls : MonoBehaviour
{
    #region Variables
    public Transform camera;
    public StarFlickering starFlickerController;
    public LoopingSprite bigStarMovingController;

    Vector3 originalPosition;
    Vector3 editedPosition;

    float moveSpeed = 1, flickerSpeed = 1;
    public float cameraSpeed = 0.1f;
    Vector3 lastPosition;

    bool enableFlicker;

    Vector2 baseFlickerSpeed, baseStarMoveSpeed;

    public float maxSpeed = 1.5f, maxZoom = 1;

    bool hideAll = true, starDropDown, bigStarDropDown;
    int currentScene;
    const int TotalScenes = 20;
    #endregion

    #region Methods
    void Start()
    {
        DontDestroyOnLoad(gameObject);//preserve this between scenes

        originalPosition = camera.position;
    }

    void OnGUI()
    {
        #region Initialising
        if(camera == null)//this will be the case when the scene changes
        {
            camera = GameObject.FindObjectOfType<Camera>().transform;
            starFlickerController = GameObject.FindObjectOfType<StarFlickering>();
            bigStarMovingController = GameObject.FindObjectOfType<LoopingSprite>();
        }
        #endregion

        if(GUILayout.Button(hideAll ? "Hide Controls" : "Show Controls"))
            hideAll = !hideAll;

        if(hideAll)
        {
            GUILayout.Label("Camera Zoom");
            editedPosition.z = GUILayout.HorizontalSlider(editedPosition.z, 0, maxZoom);

            if(starFlickerController != null)
                if(editedPosition.z < (maxZoom * 0.75) && starFlickerController.scrollSpeed != Vector2.zero)
                    GUILayout.Label("Small stars flickering too much?\nTry turning their speed down\nor zooming the camera in further");

            GUILayout.Label("Time Control (Speed Up All)");
            moveSpeed = GUILayout.HorizontalSlider(moveSpeed, 1, maxSpeed);

            if(starFlickerController != null)
            {
                if(GUILayout.Button("Edit Small Stars"))
                    starDropDown = !starDropDown;

                if(starDropDown)
                {
                    GUILayout.Label("Small Stars Movement Speed X");
                    starFlickerController.scrollSpeed.x = GUILayout.HorizontalSlider(starFlickerController.scrollSpeed.x, -0.002f, 0.002f);

                    GUILayout.Label("Small Stars Movement Speed Y");
                    starFlickerController.scrollSpeed.y = GUILayout.HorizontalSlider(starFlickerController.scrollSpeed.y, -0.002f, 0.002f);

                    enableFlicker = GUILayout.Toggle(enableFlicker, enableFlicker ? "Flickering Enabled" : "Flickering Disabled");
                    starFlickerController.enableFlickering = enableFlicker;

                    if(starFlickerController.enableFlickering)
                    {
                        GUILayout.Label("Small Stars Flicker Speed X");
                        starFlickerController.flickerSpeed.x = GUILayout.HorizontalSlider(starFlickerController.flickerSpeed.x, -0.02f, 0.02f);

                        GUILayout.Label("Small Stars Flicker Speed Y");
                        starFlickerController.flickerSpeed.y = GUILayout.HorizontalSlider(starFlickerController.flickerSpeed.y, -0.02f, 0.02f);

                        GUILayout.Label("Flickering will look unnaturally\nfast if the stars are moving!\nSmall stars should be stationary\nor very slow if you enable flickering");
                    }
                }
            }

            if(bigStarMovingController != null)
            {
                if(GUILayout.Button("Edit Large Stars"))
                    bigStarDropDown = !bigStarDropDown;

                if(bigStarDropDown)
                {
                    GUILayout.Space(10);

                    GUILayout.Label("Large Stars Movement Speed X");
                    bigStarMovingController.scrollSpeed.x = GUILayout.HorizontalSlider(bigStarMovingController.scrollSpeed.x, -0.005f, 0.005f);

                    GUILayout.Label("Large Stars Movement Speed Y");
                    bigStarMovingController.scrollSpeed.y = GUILayout.HorizontalSlider(bigStarMovingController.scrollSpeed.y, -0.005f, 0.005f);
                }
            }
        }

        GUILayout.BeginHorizontal();

        if(GUILayout.Button("Back"))
        {
            currentScene--;

            if(currentScene < 0)
                currentScene = TotalScenes - 1;

            Application.LoadLevel(currentScene);
        }

        if(GUILayout.Button("Next"))
        {
            currentScene = (currentScene + 1) % TotalScenes;

            Application.LoadLevel(currentScene);
        }

        GUILayout.EndHorizontal();
    }

    void Update()
    {
        if(camera != null)
            camera.position = originalPosition + editedPosition;

        Time.timeScale = moveSpeed;
    }
    #endregion
}
