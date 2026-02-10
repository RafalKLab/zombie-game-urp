using UnityEngine;

public interface ICharacterController
{
    void MoveTo(Vector3 target);
    void SetAttackTarget(AiTarget target);
    void ClearAttackTarget();
}
