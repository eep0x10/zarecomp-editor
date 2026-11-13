namespace ZaTool.Engine;

/// <summary>A write failed after its verified backup existed. Never automatically replay an ambiguous replacement.</summary>
public sealed class SaveCommitException(string backupDirectory, string phase, bool mayHaveCommitted, Exception inner)
    : IOException("Save commit verification failed. Inspect the verified backup and reload before retrying.", inner)
{
    public string BackupDirectory { get; } = backupDirectory;
    public string Phase { get; } = phase;
    public bool MayHaveCommitted { get; } = mayHaveCommitted;
}
