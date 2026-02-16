using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PortraitCapture : MonoBehaviour
{
    [Header("Capture Source")]
    [SerializeField] private Camera portraitCamera;
    [SerializeField] private RenderTexture renderTexture;

    [Header("Output")]
    [SerializeField] private string fileName = "portrait";
    [SerializeField, Min(1)] private int superSample = 1;

    [Header("Green Screen (Chroma Key)")]
    [Tooltip("Solid background color to key out. Use a color that never appears on the model (magenta is a classic).")]
    [SerializeField] private Color32 keyColor = new Color32(255, 0, 255, 255); // Magenta
    [Tooltip("How close a pixel must be to the key color to be removed (0-60 typical).")]
    [SerializeField, Range(0, 80)] private int tolerance = 12;
    [Tooltip("If true, forces the camera background to keyColor for the capture.")]
    [SerializeField] private bool forceCameraBackgroundToKeyColor = true;

    [Header("Save Location (Editor)")]
    [Tooltip("If true, saves directly into Assets/Portraits so Unity auto-imports the PNG as an asset.")]
    [SerializeField] private bool saveDirectlyToAssets = true;
    [Tooltip("Subfolder under Assets/ when saveDirectlyToAssets is enabled.")]
    [SerializeField] private string assetsSubfolder = "Portraits";

    [ContextMenu("Capture PNG")]
    public void CapturePng()
    {
        if (portraitCamera == null)
        {
            Debug.LogError("PortraitCapture: Brak  kamery.");
            return;
        }

        if (renderTexture == null)
        {
            Debug.LogError("PortraitCapture: Brak  RenderTexture.");
            return;
        }

        int w = renderTexture.width * Mathf.Max(1, superSample);
        int h = renderTexture.height * Mathf.Max(1, superSample);

        RenderTexture prevCamTarget = portraitCamera.targetTexture;
        RenderTexture prevActive = RenderTexture.active;

        RenderTexture tempRT = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32);
        tempRT.antiAliasing = 1;

        // Save & override camera background (optional but recommended)
        var prevClearFlags = portraitCamera.clearFlags;
        var prevBg = portraitCamera.backgroundColor;

        portraitCamera.targetTexture = tempRT;

        if (forceCameraBackgroundToKeyColor)
        {
            portraitCamera.clearFlags = CameraClearFlags.SolidColor;
            portraitCamera.backgroundColor = new Color32(keyColor.r, keyColor.g, keyColor.b, 255);
        }

        // Render now
        portraitCamera.Render();

        // Restore camera settings
        portraitCamera.clearFlags = prevClearFlags;
        portraitCamera.backgroundColor = prevBg;

        // Read pixels from RT into CPU texture
        RenderTexture.active = tempRT;

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        // Chroma key: remove keyColor -> alpha 0
        ApplyChromaKey(tex, keyColor, tolerance);

        byte[] pngBytes = tex.EncodeToPNG();

        // Cleanup render targets
        portraitCamera.targetTexture = prevCamTarget;
        RenderTexture.active = prevActive;
        RenderTexture.ReleaseTemporary(tempRT);

        if (Application.isPlaying) Destroy(tex);
        else DestroyImmediate(tex);

        // Save file
#if UNITY_EDITOR
        SaveInEditor(pngBytes);
#else
        SaveInBuild(pngBytes);
#endif
    }

    private static void ApplyChromaKey(Texture2D tex, Color32 key, int tol)
    {
        tol = Mathf.Max(0, tol);

        var pixels = tex.GetPixels32();
        for (int i = 0; i < pixels.Length; i++)
        {
            var p = pixels[i];

            bool isKey =
                Mathf.Abs(p.r - key.r) <= tol &&
                Mathf.Abs(p.g - key.g) <= tol &&
                Mathf.Abs(p.b - key.b) <= tol;

            if (isKey)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
    }

#if UNITY_EDITOR
    private void SaveInEditor(byte[] pngBytes)
    {
        if (saveDirectlyToAssets)
        {
            string dir = Path.Combine(Application.dataPath, assetsSubfolder);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, fileName + ".png");
            File.WriteAllBytes(path, pngBytes);

            Debug.Log($"Zapisano portret: {path}");
            AssetDatabase.Refresh();
            return;
        }

        string defaultDir = Path.Combine(Application.dataPath, assetsSubfolder);
        if (!Directory.Exists(defaultDir)) Directory.CreateDirectory(defaultDir);

        string pathPicked = EditorUtility.SaveFilePanel(
            "Zapisz PNG",
            defaultDir,
            fileName,
            "png"
        );

        if (string.IsNullOrEmpty(pathPicked)) return;

        File.WriteAllBytes(pathPicked, pngBytes);
        Debug.Log($"Zapisano portret: {pathPicked}");
        AssetDatabase.Refresh();
    }
#endif

    private void SaveInBuild(byte[] pngBytes)
    {
        string dir = Path.Combine(Application.persistentDataPath, "Portraits");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, fileName + ".png");
        File.WriteAllBytes(path, pngBytes);
        Debug.Log($"Saved portrait to: {path}");
    }
}
