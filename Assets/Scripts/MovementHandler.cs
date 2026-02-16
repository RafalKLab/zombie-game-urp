using UnityEngine;

// Playbale character movement, TODO: Refactor with clear architecture one handles input proceeds to controller
public class MovementHandler : MonoBehaviour
{
    private void Start()
    {
        GameInput.Instance.OnMouseRightClick += OnMouseRightClick;
        GameInput.Instance.OnInteractClick += GameInput_OnInteractClick;
    }
 
    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnMouseRightClick -= OnMouseRightClick;
            GameInput.Instance.OnInteractClick -= GameInput_OnInteractClick;
        }
    }

    private void OnMouseRightClick(object sender, System.EventArgs e)
    {

        if (ActiveCharacterManager.Instance == null)
        {
            return;
        }

        PlayableCharacter playableCharacter = ActiveCharacterManager.Instance.GetActivePlayableCharacter();

        if (playableCharacter == null)
        {
            return;
        }

        if (Camera.main == null)
        {
            return;
        }

        if (UnityEngine.InputSystem.Mouse.current == null)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(
            UnityEngine.InputSystem.Mouse.current.position.ReadValue()
        );

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            playableCharacter.MoveTo(hit.point);
        }
    }

    private void GameInput_OnInteractClick(object sender, System.EventArgs e)
    {
        if (ActiveCharacterManager.Instance == null)
        {
            return;
        }

        PlayableCharacter playableCharacter = ActiveCharacterManager.Instance.GetActivePlayableCharacter();

        if (playableCharacter == null)
        {
            return;
        }

        playableCharacter.TryInteract();
    }

}
