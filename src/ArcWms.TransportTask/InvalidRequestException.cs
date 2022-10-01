namespace ArcWms;

/// <summary>
/// 在收到无效的请求时引发。
/// </summary>
[Serializable]
public class InvalidRequestException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error">错误信息，末尾不带标点符号。</param>
    public InvalidRequestException(string error) : base($"无效的请求：{error}")
    {
    }
}
