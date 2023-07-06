using UnityEngine;
using System.Collections;

public class IntensityControl : MonoBehaviour
{
    float exposure = 1;

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Exposure", exposure);
    }

    void OnGUI()
    {
        GUILayout.BeginVertical(GUILayout.Width(150));

        GUILayout.Label("Intensity");
        exposure = GUILayout.HorizontalSlider(exposure, 0.25f, 4);

        GUILayout.EndHorizontal();
    }
}
