using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class HierarchyColorEditor
{
    static HierarchyColorEditor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
    }

    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj != null)
        {
            Color storedColor = GetStoredColor(obj);

            if (storedColor != Color.clear)
            {
                EditorGUI.DrawRect(selectionRect, storedColor);
                // Calculate luminance to determine if the text should be white or black
                float luminance = 0.2126f * storedColor.r + 0.7152f * storedColor.g + 0.0722f * storedColor.b;

                // Choose text color based on luminance (dark background = white text, light background = black text)
                Color textColor = luminance < 0.5f ? Color.white : Color.black;

                // Set label style with dynamic text color
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = textColor }  // Set the text color dynamically
                };

                // Now, draw the label on top of the background
                Rect labelRect = new Rect(selectionRect.x + 20, selectionRect.y, selectionRect.width, selectionRect.height);
                EditorGUI.LabelField(labelRect, obj.name, labelStyle);
            }

        }
    }

    // Get the color stored in a custom editor data (could be saved in EditorPrefs or a custom class)
    public static Color GetStoredColor(GameObject obj)
    {
        string key = "ObjectColor_" + obj.GetInstanceID();
        if (EditorPrefs.HasKey(key))
        {
            // Return stored color if it exists (as RGBA encoded string)
            string colorString = EditorPrefs.GetString(key);
            return StringToColor(colorString);
        }

        return Color.clear; // Default to no color
    }

    // Convert a string (RGBA) back to a Color
    private static Color StringToColor(string colorString)
    {
        string[] values = colorString.Split(',');
        if (values.Length == 4)
        {
            return new Color(
                float.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(values[3], System.Globalization.CultureInfo.InvariantCulture)
            );
        }

        return Color.clear;
    }

    // Convert Color to string (RGBA)
    private static string ColorToString(Color color)
    {
        return $"{color.r.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}," +
           $"{color.g.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}," +
           $"{color.b.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}," +
           $"{color.a.ToString("F6", System.Globalization.CultureInfo.InvariantCulture)}";
    }

    // Add custom context menu for changing and resetting the color
    [MenuItem("GameObject/Hierarchy Color Change/Change Color", false, 10)]
    private static void ChangeColor()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj != null)
        {
            // Open the color picker window to select a color
            ColorPickerWindow.Open(selectedObj);
        }
        else
        {
            Debug.LogWarning("No GameObject selected!");
        }
    }

    [MenuItem("GameObject/Hierarchy Color Change/Reset Color", false, 11)]
    private static void ResetColorFromMenu()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj != null)
        {
            ResetColor(selectedObj);
        }
        else
        {
            Debug.LogWarning("No GameObject selected!");
        }
    }

    // Reset the color of the selected GameObject to clear (transparent)
    public static void ResetColor(GameObject obj)
    {
        string key = "ObjectColor_" + obj.GetInstanceID();
        EditorPrefs.DeleteKey(key); // Remove the saved color from EditorPrefs

        // Trigger the hierarchy window to refresh (force a repaint)
        EditorApplication.RepaintHierarchyWindow();
    }

    // Store the selected color using EditorPrefs (or you could use a custom class for persistent storage)
    public static void StoreColor(GameObject obj, Color color)
    {
        string key = "ObjectColor_" + obj.GetInstanceID();

        // Check if alpha is 0, and if so, set it to 1 (fully opaque) for visibility
        if (color.a == 0)
        {
            color.a = 1f; // Ensure that the color is visible in the hierarchy
        }

        string colorString = ColorToString(color);

        EditorPrefs.SetString(key, colorString);

        // Ensure the object is marked dirty
        EditorUtility.SetDirty(obj);

        // Force repaint
        EditorApplication.RepaintHierarchyWindow();
    }
}

// Create a custom window to display the color picker
public class ColorPickerWindow : EditorWindow
{
    private GameObject selectedObject;
    private Color selectedColor = Color.white;

    public static void Open(GameObject obj)
    {
        ColorPickerWindow window = GetWindow<ColorPickerWindow>("Pick Color");
        window.selectedObject = obj;
        window.selectedColor = HierarchyColorEditor.GetStoredColor(obj);
        window.Show();
    }

    private void OnGUI()
    {
        if (selectedObject == null)
        {
            EditorGUILayout.LabelField("No object selected!");
            return;
        }

        selectedColor = EditorGUILayout.ColorField("Pick Color", selectedColor);

        if (GUILayout.Button("Apply Color"))
        {
            // Store the selected color
            HierarchyColorEditor.StoreColor(selectedObject, selectedColor);
            // Close the color picker window
            Close();
        }

        if (GUILayout.Button("Reset Color"))
        {
            // Reset color to clear
            HierarchyColorEditor.ResetColor(selectedObject);
            // Close the color picker window
            Close();
        }
    }
}