namespace Migration.Common
{
    public interface ISourceRevision
    {
        string OriginId { get; }
        string Type { get; }

        object GetFieldValue(string fieldName);

    }
}