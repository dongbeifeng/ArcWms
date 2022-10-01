namespace ArcWms;

public class DefaultBatchService : IBatchService
{
    public string GetValueForNoBatch()
    {
        return "*****";
    }

    public string Normalize(string? batch)
    {
        if (string.IsNullOrWhiteSpace(batch))
        {
            return GetValueForNoBatch();
        }

        return batch.Trim();
    }
}
