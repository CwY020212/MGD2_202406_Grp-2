using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISafeAreaHandler : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea;
    private Vector2 screenSize;

    void Start()
    {
        panel = GetComponent<RectTransform>();
        screenSize = new Vector2(Screen.width, Screen.height);
        lastSafeArea = Screen.safeArea;
        UpdatePanelAnchors(lastSafeArea);
    }

    void Update()
    {
        Rect safeArea = Screen.safeArea;

        // Update only if the safe area has changed
        if (safeArea != lastSafeArea)
        {
            lastSafeArea = safeArea;
            UpdatePanelAnchors(safeArea);
        }

        // For testing purposes
        if (Application.isEditor && Input.GetButton("Jump"))
        {
            // Use the notch properties of the iPhone XS Max
            if (Screen.height > Screen.width)
            {
                // Portrait
                safeArea = new Rect(0f, 0.038f, 1f, 0.913f);
            }
            else
            {
                // Landscape
                safeArea = new Rect(0.049f, 0.051f, 0.902f, 0.949f);
            }
            UpdatePanelAnchors(safeArea);
        }
    }

    private void UpdatePanelAnchors(Rect safeArea)
    {
        Vector2 minAnchor = safeArea.position / screenSize;
        Vector2 maxAnchor = (safeArea.position + safeArea.size) / screenSize;
        panel.anchorMin = minAnchor;
        panel.anchorMax = maxAnchor;
    }

}
