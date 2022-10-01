namespace ArcWms;

/// <summary>
/// 表示导致完成任务失败的错误。
/// </summary>
[Serializable]
public class CompleteTaskFailedException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error">错误信息，末尾不带标点符号。</param>
    public CompleteTaskFailedException(string error) : base($"完成任务失败：{error}。") { }
}
