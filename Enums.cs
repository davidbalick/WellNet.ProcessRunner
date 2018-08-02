namespace WellNet.ProcessRunner
{
    public enum JobPropertyBagKey
    {
        Filename,
        ArchivedFilename
    };
    public enum EventMessageSeverity
    {
        Fatal = 0,
        Documentation = 1,
        UserInterfaceProgress = 2
    }
    public enum EventJobStatus
    {
        Working = 1,
        Completed = 2,
        Failed = 3
    }
    public enum SetupJobDirection
    {
        Unknown = 0,
        NotApplicable = 1,
        Inbound = 2,
        Outbound = 3
    }


}
