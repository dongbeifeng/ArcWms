namespace ArcWms;

/// <summary>
/// 表示导致创建任务失败的错误。
/// </summary>
[Serializable]
public class CreateTaskFailedException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error">错误信息，末尾不带标点符号。</param>
    public CreateTaskFailedException(string error) : base($"创建任务失败：{error}。") { }
}
