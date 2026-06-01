using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public void OnQuitButtonClicked()
    {

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}