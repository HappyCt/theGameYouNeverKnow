using UnityEngine;
using UnityEditor;

public class AddAttackEvents
{
    [MenuItem("Tools/Add Attack Animation Events")]
    public static void AddEvents()
    {
        string folder = "Assets/Animation of player/Attack";
        var guidList = new System.Collections.Generic.List<string>();
        guidList.AddRange(AssetDatabase.FindAssets("Knight_Melee_ t:AnimationClip", new[] { folder }));
        guidList.AddRange(AssetDatabase.FindAssets("Knight_MeleeRun_ t:AnimationClip", new[] { folder }));
        string[] guids = guidList.ToArray();

        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null) continue;

            AnimationEvent evt = new AnimationEvent
            {
                functionName = "OnAttackEnd",
                time = clip.length
            };

            AnimationUtility.SetAnimationEvents(clip, new[] { evt });
            EditorUtility.SetDirty(clip);
            Debug.Log($"Added OnAttackEnd to {clip.name} at {clip.length:F3}s");
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Done: {count} clips updated.");
    }
}
