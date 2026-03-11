using System.Collections.Generic;
using UnityEngine;

public class CharacterPartSwapUtility : MonoBehaviour
{
    [System.Serializable]
    public class RendererSwapPair
    {
        [Header("Optional label only")]
        public string partName;

        [Header("SOURCE (mesh you want to transfer)")]
        public SkinnedMeshRenderer sourceRenderer;

        [Header("TARGET (renderer on destination character)")]
        public SkinnedMeshRenderer targetRenderer;
    }

    [Header("All parts to swap, e.g. Body / Legs / Feet / Head")]
    [SerializeField] private RendererSwapPair[] parts;

    [Header("Root of the target character rig (e.g. Armature / mixamorig:Hips)")]
    [SerializeField] private Transform targetRigRoot;

    [Header("Options")]
    [SerializeField] private bool copyMesh = true;
    [SerializeField] private bool copyMaterials = true;
    [SerializeField] private bool copyBlendShapeWeights = true;
    [SerializeField] private bool setUpdateWhenOffscreenForTesting = true;

    [ContextMenu("Swap All Character Parts")]
    public void SwapAllParts()
    {
        if (targetRigRoot == null)
        {
            Debug.LogError("Missing targetRigRoot.", this);
            return;
        }

        if (parts == null || parts.Length == 0)
        {
            Debug.LogError("No parts configured.", this);
            return;
        }

        // Build bone map once
        Dictionary<string, Transform> targetBonesByName = new Dictionary<string, Transform>(512);

        foreach (Transform bone in targetRigRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!targetBonesByName.ContainsKey(bone.name))
                targetBonesByName.Add(bone.name, bone);
        }

        bool allSucceeded = true;

        for (int i = 0; i < parts.Length; i++)
        {
            RendererSwapPair pair = parts[i];

            if (pair == null)
                continue;

            bool success = SwapSingleRenderer(pair, targetBonesByName);
            if (!success)
                allSucceeded = false;
        }

        if (allSucceeded)
            Debug.Log("Character part swap completed for all configured parts.", this);
        else
            Debug.LogWarning("Character part swap finished, but some parts failed. Check console.", this);
    }

    private bool SwapSingleRenderer(RendererSwapPair pair, Dictionary<string, Transform> targetBonesByName)
    {
        if (pair.sourceRenderer == null || pair.targetRenderer == null)
        {
            Debug.LogError($"Missing source or target renderer in part '{pair.partName}'.", this);
            return false;
        }

        SkinnedMeshRenderer source = pair.sourceRenderer;
        SkinnedMeshRenderer target = pair.targetRenderer;

        if (source.sharedMesh == null)
        {
            Debug.LogError($"Source renderer in part '{pair.partName}' has no sharedMesh.", this);
            return false;
        }

        // 1) Copy mesh/materials
        if (copyMesh)
            target.sharedMesh = source.sharedMesh;

        if (copyMaterials)
            target.sharedMaterials = source.sharedMaterials;

        // 2) Remap bones
        Transform[] sourceBones = source.bones;

        if (sourceBones == null || sourceBones.Length == 0)
        {
            Debug.LogError($"Source renderer in part '{pair.partName}' has no bones. It may not be properly skinned.", this);
            return false;
        }

        Transform[] remappedBones = new Transform[sourceBones.Length];

        for (int i = 0; i < sourceBones.Length; i++)
        {
            Transform sourceBone = sourceBones[i];

            if (sourceBone == null)
            {
                Debug.LogError($"Part '{pair.partName}': source bone[{i}] is null.", this);
                return false;
            }

            if (!targetBonesByName.TryGetValue(sourceBone.name, out Transform mappedBone))
            {
                Debug.LogError($"Part '{pair.partName}': target rig does not contain bone '{sourceBone.name}'.", this);
                return false;
            }

            remappedBones[i] = mappedBone;
        }

        target.bones = remappedBones;

        // 3) Remap rootBone
        if (source.rootBone != null && targetBonesByName.TryGetValue(source.rootBone.name, out Transform mappedRoot))
        {
            target.rootBone = mappedRoot;
        }
        else
        {
            // fallback
            if (targetBonesByName.TryGetValue("mixamorig:Hips", out Transform hips))
                target.rootBone = hips;
            else if (targetBonesByName.TryGetValue("Hips", out Transform hipsAlt))
                target.rootBone = hipsAlt;
        }

        // 4) BlendShapes
        if (copyBlendShapeWeights && source.sharedMesh != null && target.sharedMesh != null)
        {
            int sourceBlendShapeCount = source.sharedMesh.blendShapeCount;
            int targetBlendShapeCount = target.sharedMesh.blendShapeCount;
            int count = Mathf.Min(sourceBlendShapeCount, targetBlendShapeCount);

            for (int i = 0; i < count; i++)
            {
                target.SetBlendShapeWeight(i, source.GetBlendShapeWeight(i));
            }
        }

        // 5) Optional offscreen update
        if (setUpdateWhenOffscreenForTesting)
            target.updateWhenOffscreen = true;

        Debug.Log($"Swapped part '{pair.partName}'.", this);
        return true;
    }
}