using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class MakeAnimation
{
    static readonly string[] Directions = { "Left", "DownLeft", "Down", "DownRight", "Right", "UpRight", "Up", "UpLeft" };
    const int FramesPerDirection = 15;
    const float FrameRate = 12f;

    [MenuItem("Tools/Generate Knight Animations")]
    public static void Generate()
    {
        string folder = "Assets/2D HD Character Knight/Spritesheets/With shadows";
        string[] pngPaths = Directory.GetFiles(folder, "*.png");

        foreach (string pngPath in pngPaths)
        {
            string unityPath = pngPath.Replace('\\', '/');
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(unityPath);
            Sprite[] sprites = System.Array.FindAll(assets, a => a is Sprite).Cast<Sprite>().ToArray();

            if (sprites.Length != FramesPerDirection * Directions.Length)
            {
                Debug.LogWarning($"Skipped {unityPath}: expected {FramesPerDirection * Directions.Length} sprites, found {sprites.Length}");
                continue;
            }

            // Sort by name suffix number: SpriteName_0, SpriteName_1, ...
            System.Array.Sort(sprites, (a, b) =>
            {
                int ia = ExtractIndex(a.name);
                int ib = ExtractIndex(b.name);
                return ia.CompareTo(ib);
            });

            string animName = Path.GetFileNameWithoutExtension(unityPath);

            for (int d = 0; d < Directions.Length; d++)
            {
                string clipName = $"Knight_{animName}_{Directions[d]}";
                string savePath = $"{folder}/{clipName}.anim";

                AnimationClip clip = new AnimationClip();
                clip.frameRate = FrameRate;

                EditorCurveBinding binding = new EditorCurveBinding
                {
                    type = typeof(SpriteRenderer),
                    path = "",
                    propertyName = "m_Sprite"
                };

                int startFrame = d * FramesPerDirection;
                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[FramesPerDirection];

                for (int f = 0; f < FramesPerDirection; f++)
                {
                    keyframes[f] = new ObjectReferenceKeyframe
                    {
                        time = f / FrameRate,
                        value = sprites[startFrame + f]
                    };
                }

                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                AssetDatabase.CreateAsset(clip, savePath);
                Debug.Log($"Created: {savePath}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("All animations generated.");
    }

    static int ExtractIndex(string spriteName)
    {
        int underscore = spriteName.LastIndexOf('_');
        if (underscore >= 0 && int.TryParse(spriteName.Substring(underscore + 1), out int index))
            return index;
        return 0;
    }
}
