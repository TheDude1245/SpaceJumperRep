using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{

    public float speed;

    // Update is called once per frame
    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * speed);
    }
}
