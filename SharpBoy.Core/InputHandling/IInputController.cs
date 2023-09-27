namespace SharpBoy.Core.InputHandling
{
    public interface IInputController
    {
        void CheckForInputs();
        byte ReadRegister();
        void WriteRegister(byte value);
    }
}