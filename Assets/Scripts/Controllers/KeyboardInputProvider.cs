using System;
using UnityEngine;
using UnityEngine.InputSystem; // Fondamentale per il Nuovo Input System

// 1. Deve implementare l'interfaccia di Input standard
// 2. Deve implementare l'interfaccia generata per le Actions (es. PlayerControls.IPlayerActions)
public class KeyboardInputProvider : IInputProvider, InputSystemActions.IPlayerActions, IDisposable
{
    // Variabile per memorizzare il valore Vector2 letto dall'azione "Move"
    private Vector2 moveVector = Vector2.zero;
    private bool isSprinting = false;

    // Riferimento al tuo Input Action Asset generato
    private InputSystemActions controls;

    public KeyboardInputProvider()
    {
        // 1. Crea e abilita le azioni
        controls = new InputSystemActions();

        // 2. Imposta questa classe come listener per i callback dell'Action Map "Player"
        controls.Player.SetCallbacks(this);

        // 3. Abilita l'Action Map per iniziare a ricevere input
        controls.Player.Enable();
    }

    // ---------------------------------------------------------------------
    // Implementazione dell'interfaccia IInputProvider (usata da PlayerController)
    // ---------------------------------------------------------------------

    public float GetHorizontalInput()
    {
        // Restituisce solo la componente X (orizzontale) del Vector2
        // Questo sostituisce Input.GetAxis("Horizontal")
        return moveVector.x;
    }

    // ---------------------------------------------------------------------
    // Implementazione dei Callback IPlayerActions (chiamati dal Nuovo Input System)
    // ---------------------------------------------------------------------

    // Questo metodo è chiamato ogni volta che l'azione "Move" cambia stato (premuto/rilasciato/continuo)
    public void OnMove(InputAction.CallbackContext context)
    {
        // Legge il Vector2 generato dal 2D Vector Composite (es. (-1, 0) o (1, 0))
        moveVector = context.ReadValue<Vector2>();
    }

    public bool IsSprintActive()
    {
        return isSprinting;
    }

    // Devi implementare anche tutti gli altri metodi dell'interfaccia IPlayerActions,
    // anche se li lasci vuoti.
    public void OnLook(InputAction.CallbackContext context) { }
    public void OnAttack(InputAction.CallbackContext context) { }
    public void OnInteract(InputAction.CallbackContext context) { }
    public void OnCrouch(InputAction.CallbackContext context) { }
    public void OnJump(InputAction.CallbackContext context) { }
    public void OnPrevious(InputAction.CallbackContext context) { }
    public void OnNext(InputAction.CallbackContext context) { }
    public void OnSprint(InputAction.CallbackContext context) 
    {
        // 'context.performed' è vero quando il tasto viene premuto
        // Se si usa il tipo Action Type: Button, questo è sufficiente
        isSprinting = context.performed;
    }

    public void Dispose()
    {
        controls.Player.Disable();
        controls.Dispose();
    }

}