namespace Generator.Core.Inject
{
    public interface IInjector
    {
        string Name { get; }
        bool Check(string tableName, string columnName = "");
        string Inject(string originContent, string tableName = "", string columnName = "");
    }

    public interface IDALInjector : IInjector
    { }

    public interface IModelInjector : IInjector
    { }

    public interface IEnumInjector : IInjector
    { }
}
