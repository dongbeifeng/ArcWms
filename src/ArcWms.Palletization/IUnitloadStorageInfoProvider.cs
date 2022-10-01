namespace ArcWms;

// TODO 重构
// TODO 文档 扩展点
/// <summary>
/// 
/// </summary>
public interface IUnitloadStorageInfoProvider
{
    string GetPalletSpecification(Unitload unitload);

    string GetOutFlag(Unitload unitload);

    string GetStorageGroup(Unitload unitload);

}
