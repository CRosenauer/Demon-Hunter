using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMenuInput : MonoBehaviour
{
    [SerializeField] MenuEvent m_select;
    [SerializeField] MenuEvent m_cancel;
    [SerializeField] MenuNavigationEvent m_navigation;

	public void OnSelect(InputAction.CallbackContext context)
	{
		if(!context.performed)
        {
			return;
        }

		m_select.Raise();
	}

	public void OnCancel(InputAction.CallbackContext context)
	{
		if (!context.performed)
		{
			return;
		}

		m_cancel.Raise();
	}

	public void OnNavigate(InputAction.CallbackContext context)
	{
		if (!context.performed)
		{
			return;
		}

		Vector2 input = context.ReadValue<Vector2>();

		input.x = Mathf.Approximately(input.x, 0f) ? 0f : Mathf.Sign(input.x);
		input.y = Mathf.Approximately(input.y, 0f) ? 0f : Mathf.Sign(input.y);

		m_navigation.Raise(input);
	}
}
