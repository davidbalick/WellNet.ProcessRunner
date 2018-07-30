namespace WellNet.ProcessRunner
{
    public interface IFunction
    {
        string Context { get; set; }
        void Execute();
    }
}
