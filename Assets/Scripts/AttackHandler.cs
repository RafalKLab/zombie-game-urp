using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Playbale character attack
public class AttackHandler : MonoBehaviour
{
    [SerializeField] private LayerMask targetableMask;
    private float maxClickDistance = 1000f;

    private void Start()
    {
        GameInput.Instance.OnMouseLeftClick += GameInput_OnMouseLeftClick;
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
            GameInput.Instance.OnMouseLeftClick -= GameInput_OnMouseLeftClick;
    }

    private void GameInput_OnMouseLeftClick(object sender, System.EventArgs e)
    {
        if (ActiveCharacterManager.Instance == null) return;

        if (IsPointerOverUI_Now())
            return;

        PlayableCharacter activePlayableCharacter = ActiveCharacterManager.Instance.GetActivePlayableCharacter();

        if (activePlayableCharacter == null) return;


        if (Camera.main == null) return;

        if (UnityEngine.InputSystem.Mouse.current == null) return;

        Vector2 mousePos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit, maxClickDistance, targetableMask))
        {
            activePlayableCharacter.ClearAttackTarget();
            return;
        }

        AiTarget target = hit.collider.GetComponentInParent<AiTarget>();
        if (target == null)
        {
            activePlayableCharacter.ClearAttackTarget();
            return;
        }

        if (target.GetFaction() == Faction.Player)
        {
            activePlayableCharacter.ClearAttackTarget();
            return;
        }

        Health targetHealth = target.GetComponentInParent<Health>();
        if (targetHealth == null || targetHealth.IsDead)
        {
            activePlayableCharacter.ClearAttackTarget();
            return;
        }

        activePlayableCharacter.SetAttackTarget(target);
    }

    private bool IsPointerOverUI_Now()
    {
        if (EventSystem.current == null) return false;
        if (Mouse.current == null) return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}
