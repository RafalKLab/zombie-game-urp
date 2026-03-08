using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CharacterLookAtTarget : MonoBehaviour
{
    [SerializeField] private MultiAimConstraint headAim;

    public void SetSourceTarget(Transform target)
    {
        var sources = headAim.data.sourceObjects;

        if (sources.Count == 0)
        {
            sources.Add(new WeightedTransform(target, 1f));
        }
        else
        {
            sources.SetTransform(0, target);
        }

        headAim.data.sourceObjects = sources;

        RigBuilder rigBuilder = GetComponent<RigBuilder>();
        if (rigBuilder != null)
        {
            rigBuilder.Build();
        }
    }
}
