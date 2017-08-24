using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(RayTraceRenderer))]
public class TestRayTraceEditor : Editor
{
    RayTraceRenderer renderer;
    Vector2 scrollPos;
    void OnEnable()
    {
        renderer = target as RayTraceRenderer;
        
        EditorUtility.ClearProgressBar();
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(20);
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Render")) {
                renderer.updateProgress = (float progress) => {
                    bool bCancel = EditorUtility.DisplayCancelableProgressBar("Rendering...", "Render", progress);
                    return bCancel;
                };

                renderer.Init();
                renderer.Render();
                renderer.Release();
                EditorUtility.ClearProgressBar();

                EditorUtility.UnloadUnusedAssetsImmediate();
                System.GC.Collect();
            }
            if (GUILayout.Button("Save")) {
                byte[] bytes = renderer.texture.EncodeToPNG();
                string name = SceneManager.GetActiveScene().name;
                string pathToSave = EditorUtility.SaveFilePanel("save as png...", "", name + ".png", "png");
                if (!string.IsNullOrEmpty(pathToSave)) {
                    File.WriteAllBytes(pathToSave, bytes);
                }
            }
            if (GUILayout.Button("Clear")) {
                renderer.Clear();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        int boxSize = Screen.width - 20;
        GUILayout.Box(renderer.texture, GUILayout.Width(boxSize), GUILayout.Height(boxSize));
    }
}