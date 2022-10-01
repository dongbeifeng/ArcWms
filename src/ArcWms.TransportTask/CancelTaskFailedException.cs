namespace ArcWms;

/// <summary>
/// 表示导致取消任务失败的错误。
/// </summary>
[Serializable]
public class CancelTaskFailedException : Exception
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="error">错误信息，末尾不带标点符号。</param>
    public CancelTaskFailedException(string error) : base($"取消任务失败：{error}。") { }
}
