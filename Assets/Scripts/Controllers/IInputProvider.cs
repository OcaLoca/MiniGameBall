public interface IInputProvider
{
    float GetHorizontalInput();

    bool IsSprintActive();

    bool OnAttack();
}
