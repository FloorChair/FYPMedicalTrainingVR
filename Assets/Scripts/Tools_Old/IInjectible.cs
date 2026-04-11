public interface IInjectable
{
    void Inject(float deltaTime);
    void ResetProgress();
    bool IsCompleted { get; }
}
