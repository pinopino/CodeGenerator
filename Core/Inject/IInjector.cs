﻿namespace Generator.Core.Inject
{
    public interface IInjector
    {
        string Inject(string originContent, string tableName = "", string columnName = "");
    }

    public interface IDALInjector : IInjector
    { }

    public interface IModelInjector : IInjector
    { }

    public interface IEnumInjector : IInjector
    { }
}
