namespace Generator.Core.Inject
{
    public interface IInjector
    {
        string Inject(string originContent);
    }

    public interface IDALInjector : IInjector
    { }

    public interface IModelInjector : IInjector
    { }

    public interface IEnumInjector : IInjector
    { }
}
