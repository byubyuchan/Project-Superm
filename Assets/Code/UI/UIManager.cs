using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private class PanelData
    {
        public GameObject panel;
        public System.Action closeAction;
    }

    private List<PanelData> panelStack = new List<PanelData>();

    public System.Action onEmptyEsc;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OpenEscapeUI()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            if (EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                return;
            }
        }

        // Remove panel data that is null or inactive by the time Player clicks close button
        panelStack.RemoveAll(p => p.panel == null || !p.panel.activeSelf);

        // close the top panel if there is any panel in the stack
        if (panelStack.Count > 0)
        {
            PanelData topPanel = panelStack[panelStack.Count - 1];
            topPanel.closeAction?.Invoke();
        }
        else
        {
            // If there is no panel in the stack, call onEmptyEsc action if it exists
            onEmptyEsc?.Invoke();
        }
    }

    public void ShowPanel(GameObject panelObj, System.Action closeFunc)
    {
        panelObj.SetActive(true);

        panelStack.RemoveAll(p => p.panel == panelObj);

        panelStack.Add(new PanelData { panel = panelObj, closeAction = closeFunc });
    }
}