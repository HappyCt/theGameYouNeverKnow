using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class KnightAnimationGenerator2 : EditorWindow
{
    private string spriteSheetFolder = "Assets/2D HD Character Knight/Spritesheets/With shadows/need";
    private string outputFolder = "Assets/Animation";
    private int framesPerRow = 15;

    private float idleFrameRate    = 12f;
    private float walkFrameRate    = 12f;
    private float runFrameRate     = 18f;
    private float meleeFrameRate   = 24f;
    private float damageFrameRate  = 18f;
    private float dieFrameRate     = 12f;

    private bool overwriteExisting = true;

    private string[] directionNames = new string[]
    {
        "Down",
        "DownLeft",
        "Left",
        "UpLeft",
        "Up",
        "UpRight",
        "Right",
        "DownRight"
    };

    private static readonly HashSet<string> noLoopActions = new HashSet<string>
    {
        "Die", "TakeDamage", "Melee"
    };

    [MenuItem("Tools/Knight Animation Generator 2")]
    public static void ShowWindow()
    {
        GetWindow<KnightAnimationGenerator2>("Knight Anim Gen 2");
    }

    private void OnGUI()
    {
        GUILayout.Label("Knight Animation Generator 2", EditorStyles.boldLabel);
        GUILayout.Space(10);

        spriteSheetFolder  = EditorGUILayout.TextField("Sprite Sheet Folder", spriteSheetFolder);
        outputFolder       = EditorGUILayout.TextField("Output Folder", outputFolder);
        framesPerRow       = EditorGUILayout.IntField("Frames Per Row", framesPerRow);
        overwriteExisting  = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);

        GUILayout.Space(10);
        GUILayout.Label("Frame Rate per Action", EditorStyles.boldLabel);
        idleFrameRate   = EditorGUILayout.FloatField("Idle",       idleFrameRate);
        walkFrameRate   = EditorGUILayout.FloatField("Walk",       walkFrameRate);
        runFrameRate    = EditorGUILayout.FloatField("Run",        runFrameRate);
        meleeFrameRate  = EditorGUILayout.FloatField("Melee",      meleeFrameRate);
        damageFrameRate = EditorGUILayout.FloatField("TakeDamage", damageFrameRate);
        dieFrameRate    = EditorGUILayout.FloatField("Die",        dieFrameRate);

        GUILayout.Space(10);
        GUILayout.Label("Direction order (row 0 to row 7)", EditorStyles.boldLabel);
        for (int i = 0; i < directionNames.Length; i++)
        {
            directionNames[i] = EditorGUILayout.TextField($"Row {i}", directionNames[i]);
        }

        GUILayout.Space(15);

        if (GUILayout.Button("Generate All Clips", GUILayout.Height(40)))
        {
            GenerateClips();
        }
    }

    private float GetFrameRate(string actionName)
    {
        switch (actionName)
        {
            case "Idle":       return idleFrameRate;
            case "Walk":       return walkFrameRate;
            case "Run":        return runFrameRate;
            case "Melee":      return meleeFrameRate;
            case "TakeDamage": return damageFrameRate;
            case "Die":        return dieFrameRate;
            default:           return 12f;
        }
    }

    private void GenerateClips()
    {
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string parent     = Path.GetDirectoryName(outputFolder);
            string folderName = Path.GetFileName(outputFolder);
            AssetDatabase.CreateFolder(parent, folderName);
            Debug.Log($"[KnightAnimGen2] Created folder: {outputFolder}");
        }

        string[] actionNames = new string[] { "Idle", "Walk", "Run", "Melee", "TakeDamage", "Die" };
        int totalCreated  = 0;
        int totalSkipped  = 0;
        int totalFailed   = 0;

        foreach (string actionName in actionNames)
        {
            string sheetPath = $"{spriteSheetFolder}/{actionName}";
            Sprite[] sprites = LoadSprites(sheetPath);

            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"[KnightAnimGen2] Sprites not found: {sheetPath}(.png/.jpg)");
                totalFailed++;
                continue;
            }

            Debug.Log($"[KnightAnimGen2] Loaded {sprites.Length} sprites: {actionName}");

            int totalRows = sprites.Length / framesPerRow;
            float fps     = GetFrameRate(actionName);
            bool loop     = !noLoopActions.Contains(actionName);

            for (int row = 0; row < totalRows && row < directionNames.Length; row++)
            {
                string direction = directionNames[row];
                string clipName  = $"Knight_{actionName}_{direction}";
                string clipPath  = $"{outputFolder}/{clipName}.anim";

                if (!overwriteExisting && File.Exists(clipPath))
                {
                    Debug.Log($"[KnightAnimGen2] Skipped: {clipName}");
                    totalSkipped++;
                    continue;
                }

                AnimationClip clip = CreateClip(sprites, row, fps, loop);

                AnimationClip existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(clip, existing);
                    EditorUtility.SetDirty(existing);
                }
                else
                {
                    AssetDatabase.CreateAsset(clip, clipPath);
                }

                totalCreated++;
                Debug.Log($"[KnightAnimGen2] Done: {clipName} ({fps}fps, loop={loop})");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string summary = $"Created/Updated: {totalCreated}\nSkipped: {totalSkipped}\nFailed: {totalFailed}";
        Debug.Log($"[KnightAnimGen2] Done — {summary}");
        EditorUtility.DisplayDialog("Done", summary, "OK");
    }

    private AnimationClip CreateClip(Sprite[] sprites, int row, float fps, bool loop)
    {
        AnimationClip clip   = new AnimationClip();
        clip.frameRate       = fps;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        int startFrame = row * framesPerRow;
        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[framesPerRow];

        for (int i = 0; i < framesPerRow; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time  = i / fps,
                value = sprites[startFrame + i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        return clip;
    }

    private Sprite[] LoadSprites(string basePath)
    {
        string[] extensions = { ".png", ".jpg", ".jpeg" };

        foreach (string ext in extensions)
        {
            object[] assets = AssetDatabase.LoadAllAssetsAtPath(basePath + ext);
            if (assets == null || assets.Length == 0) continue;

            List<Sprite> sprites = new List<Sprite>();
            foreach (object asset in assets)
            {
                if (asset is Sprite s) sprites.Add(s);
            }

            if (sprites.Count == 0) continue;

            sprites.Sort((a, b) =>
            {
                int numA = ExtractTrailingNumber(a.name);
                int numB = ExtractTrailingNumber(b.name);
                return numA.CompareTo(numB);
            });

            return sprites.ToArray();
        }

        return null;
    }

    private int ExtractTrailingNumber(string name)
    {
        string numStr = "";
        for (int i = name.Length - 1; i >= 0; i--)
        {
            if (char.IsDigit(name[i]))
                numStr = name[i] + numStr;
            else
                break;
        }
        return numStr.Length > 0 ? int.Parse(numStr) : 0;
    }
}
