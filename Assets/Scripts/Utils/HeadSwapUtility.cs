using System.Collections.Generic;
using UnityEngine;

public class HeadSwapUtility : MonoBehaviour
{
    [Header("SOURCE (head you want to transfer)")]
    [SerializeField] private SkinnedMeshRenderer sourceHeadRenderer;

    [Header("TARGET (head renderer on the destination character)")]
    [SerializeField] private SkinnedMeshRenderer targetHeadRenderer;

    [Header("Root of the target character rig (e.g. Armature / mixamorig:Hips)")]
    [SerializeField] private Transform targetRigRoot;

    [Header("Options")]
    [SerializeField] private bool copyMeshAndMaterials = true;
    [SerializeField] private bool setUpdateWhenOffscreenForTesting = true;

    [ContextMenu("Swap Head (Remap Bones)")]
    public void SwapHead()
    {
        if (sourceHeadRenderer == null || targetHeadRenderer == null || targetRigRoot == null)
        {
            Debug.LogError("Missing references: sourceHeadRenderer / targetHeadRenderer / targetRigRoot.", this);
            return;
        }

        // 1) Copy mesh and materials (optional)
        if (copyMeshAndMaterials)
        {
            targetHeadRenderer.sharedMesh = sourceHeadRenderer.sharedMesh;
            targetHeadRenderer.sharedMaterials = sourceHeadRenderer.sharedMaterials;
        }

        // 2) Build dictionary of target rig bones by name
        Dictionary<string, Transform> targetBonesByName = new Dictionary<string, Transform>(512);

        foreach (Transform bone in targetRigRoot.GetComponentsInChildren<Transform>(true))
        {
            if (!targetBonesByName.ContainsKey(bone.name))
                targetBonesByName.Add(bone.name, bone);
        }

        // 3) Remap bones[] in the exact same order as sourceHeadRenderer.bones
        Transform[] sourceBones = sourceHeadRenderer.bones;

        if (sourceBones == null || sourceBones.Length == 0)
        {
            Debug.LogError("Source head renderer has no bones. This usually means it is not properly skinned.", this);
            return;
        }

        Transform[] remappedBones = new Transform[sourceBones.Length];

        for (int i = 0; i < sourceBones.Length; i++)
        {
            Transform sourceBone = sourceBones[i];

            if (sourceBone == null)
            {
                Debug.LogError($"Source bone[{i}] is null. Cannot remap.", this);
                return;
            }

            if (!targetBonesByName.TryGetValue(sourceBone.name, out Transform mappedBone))
            {
                Debug.LogError($"Target rig does not contain bone: '{sourceBone.name}'.", this);
                return;
            }

            remappedBones[i] = mappedBone;
        }

        targetHeadRenderer.bones = remappedBones;

        // 4) Remap rootBone by name
        if (sourceHeadRenderer.rootBone != null &&
            targetBonesByName.TryGetValue(sourceHeadRenderer.rootBone.name, out Transform mappedRoot))
        {
            targetHeadRenderer.rootBone = mappedRoot;
        }
        else
        {
            // Fallback attempt
            if (targetBonesByName.TryGetValue("mixamorig:Head", out Transform head))
                targetHeadRenderer.rootBone = head;
            else if (targetBonesByName.TryGetValue("Head", out Transform headAlt))
                targetHeadRenderer.rootBone = headAlt;
        }

        if (setUpdateWhenOffscreenForTesting)
            targetHeadRenderer.updateWhenOffscreen = true;

        Debug.Log("Head swap completed: mesh/materials copied and bones remapped.", this);
    }
}