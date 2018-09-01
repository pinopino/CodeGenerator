using Generator.Common;
using Generator.Template;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Generator.Core
{
    public class DALGenerator
    {
        private SQLMetaData _config;

        public DALGenerator(SQLMetaData config)
        {
            this._config = config;
        }

        #region Exists
        public string Get_Exists(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.ExistPredicate.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table_config.ExistPredicate.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table_config.ExistPredicate.ForEach(p => sb3.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb4 = new StringBuilder();
            table_config.ExistPredicate.ForEach(p => sb4.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.EXISTS_TEMPLATE,
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));
            return str;
        }
        #endregion

        #region Insert
        public string Get_Insert(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey;
            var refTable = table_config.ReferenceTable;

            if (primaryKey.Count == 1)
            {
                if (refTable.Count == 0)
                {
                    return Get_Insert_SingleId(tableName);
                }
                else
                {
                    return Get_Insert_SingleId_WithChildTable(tableName);
                }
            }
            else
            {
                if (refTable.Count == 0)
                {
                    return Get_Insert_MultipleId(tableName);
                }
                else
                {
                    return Get_Insert_MultipleId_WithChildTable(tableName);
                }
            }
        }

        private string Get_Insert_SingleId(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey[0];

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    if (p.HasDefaultValue)
                    {
                        if(!_config.SkipDefault)
                        {
                            sb1.Append(string.Format("[{0}], ", p.Name));
                        }
                    }
                    else
                    {
                        sb1.Append(string.Format("[{0}], ", p.Name));
                    }
                }
            });

            var sb2 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    if (p.HasDefaultValue)
                    {
                        if (!_config.SkipDefault)
                        {
                            sb2.Append(string.Format("@{0}, ", p.Name));
                        }
                    }
                    else
                    {
                        sb2.Append(string.Format("@{0}, ", p.Name));
                    }
                }
            });

            var str = string.Format(DALTemplate.INSERT_TEMPLATE1,
                                    table_config.Comment,
                                    table_config.Comment,
                                    primaryKey.DbType,
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    primaryKey.Name,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    GetReturnStr(primaryKey.DbType),
                                    primaryKey.DbType,
                                    primaryKey.DbType);

            return str;
        }

        private string Get_Insert_SingleId_WithChildTable(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey[0];

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    if (p.HasDefaultValue)
                    {
                        if (!_config.SkipDefault)
                        {
                            sb1.Append(string.Format("[{0}], ", p.Name));
                        }
                    }
                    else
                    {
                        sb1.Append(string.Format("[{0}], ", p.Name));
                    }
                }
            });

            var sb2 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    if (p.HasDefaultValue)
                    {
                        if (!_config.SkipDefault)
                        {
                            sb2.Append(string.Format("@{0}, ", p.Name));
                        }
                    }
                    else
                    {
                        sb2.Append(string.Format("@{0}, ", p.Name));
                    }
                }
            });

            var sb3 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p => 
            {
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}// 插入相关的{1}信息", '\t', p.Name));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}foreach (var {1} in model.{2}List) ", '\t', p.Name, p.Name));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{{", '\t'));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}{1}Helper.Insert({2}, conn, tran);", '\t', p.Name, p.Name));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}}}", '\t'));
                sb3.AppendLine();
            });

            var str = string.Format(DALTemplate.INSERT_TEMPLATE2,
                                    table_config.Comment,
                                    table_config.Comment,
                                    primaryKey.DbType,
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    primaryKey.Name,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    GetReturnStr(primaryKey.DbType),
                                    table_config.Comment,
                                    primaryKey.DbType,
                                    sb3.ToString(),
                                    GetReturnStr(primaryKey.DbType),
                                    GetIfStr(primaryKey.DbType));

            return str;
        }

        private string Get_Insert_MultipleId(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            sb1.Append("Tuple<");
            table_config.PrimaryKey.ForEach(p => sb1.Append(string.Format("{0}, ", p.DbType)));
            sb1.Append(">");

            var sb2 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    if (p.HasDefaultValue)
                    {
                        if (!_config.SkipDefault)
                        {
                            sb2.Append(string.Format("[{0}], ", p.Name));
                        }
                    }
                    else
                    {
                        sb2.Append(string.Format("[{0}], ", p.Name));
                    }
                }
            });

            var sb3 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb3.Append(string.Format("INSERTED.[{0}], ", p.Name)));
            
            var sb4 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    if (p.HasDefaultValue)
                    {
                        if (!_config.SkipDefault)
                        {
                            sb4.Append(string.Format("@{0}, ", p.Name));
                        }
                    }
                    else
                    {
                        sb4.Append(string.Format("@{0}, ", p.Name));
                    }
                }
            });

            var sb5 = sb1.ToString().Replace(", >", "") + ">";

            var sb6 = sb1.ToString().Replace(", >", "") + ">";

            var sb7 = sb1.ToString().Replace(", >", "") + ">";

            var str = string.Format(DALTemplate.INSERT_TEMPLATE3,
                                    table_config.Comment,
                                    table_config.Comment,
                                    sb1.ToString().Replace(", >", "") + ">",
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    sb3.ToString().TrimEnd(", ".ToCharArray()),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()),
                                    sb5,
                                    sb6,
                                    sb7);

            return str;
        }

        private string Get_Insert_MultipleId_WithChildTable(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            sb1.Append("Tuple<");
            table_config.PrimaryKey.ForEach(p => sb1.Append(string.Format("{0}, ", p.DbType)));
            sb1.Append(">");

            var sb2 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    if (p.HasDefaultValue)
                    {
                        if (!_config.SkipDefault)
                        {
                            sb2.Append(string.Format("[{0}], ", p.Name));
                        }
                    }
                    else
                    {
                        sb2.Append(string.Format("[{0}], ", p.Name));
                    }
                }
            });

            var sb3 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb3.Append(string.Format("INSERTED.[{0}], ", p.Name)));

            var sb4 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity)
                {
                    if (p.HasDefaultValue)
                    {
                        if (!_config.SkipDefault)
                        {
                            sb4.Append(string.Format("@{0}, ", p.Name));
                        }
                    }
                    else
                    {
                        sb4.Append(string.Format("@{0}, ", p.Name));
                    }
                }
            });

            var sb5 = sb1.ToString().Replace(", >", "") + ">";

            var sb6 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p =>
            {
                sb6.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}// 插入相关的{1}信息", '\t', p.Name));
                sb6.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}foreach (var {1} in model.{2}List) ", '\t', p.Name, p.Name));
                sb6.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{{", '\t'));
                sb6.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}{1}Helper.Insert({2}, conn, tran);", '\t', p.Name, p.Name));
                sb6.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}}}", '\t'));
                sb6.AppendLine();
            });

            var sb7 = sb1.ToString().Replace(", >", "") + ">";

            var str = string.Format(DALTemplate.INSERT_TEMPLATE4,
                                    table_config.Comment,
                                    table_config.Comment,
                                    sb1.ToString().Replace(", >", "") + ">",
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    sb3.ToString().TrimEnd(", ".ToCharArray()),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()),
                                    table_config.Comment,
                                    sb5,
                                    sb6.ToString(),
                                    sb7);

            return str;
        }

        private string GetReturnStr(string keyType)
        {
            switch (keyType.ToLower())
            {
                case "int":
                    {
                        return "0";

                    } break;
                case "long":
                    {
                        return "0l";

                    } break;
                case "string":
                    {
                        return "string.Empty";

                    } break;
            }

            return string.Empty;
        }

        private string GetIfStr(string keyType)
        {
            switch (keyType.ToLower())
            {
                case "int":
                case "long":
                    {
                        return "ret == 0";

                    }
                    break;
                case "string":
                    {
                        return "ret == string.Empty";

                    }
                    break;
            }

            return string.Empty;
        }
        #endregion

        #region Delete
        public string Get_Delete(string tableName)
        {
            var table_config = _config[tableName];
            if (table_config.ReferenceTable.Count == 0)
            {
                return Get_Delete1(tableName);
            }
            else
            {
                return Get_Delete_WithChildTable(tableName);
            }
        }

        private string Get_Delete1(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb3.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb4 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb4.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.DELETE_TEMPLATE1,
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }

        private string Get_Delete_WithChildTable(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb3.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb4 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb4.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var sb5 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p =>
            {
                sb5.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}// 删除相关的{1}信息", '\t', p.Name));
                sb5.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{1}Helper.DeleteFor{2}({3}, conn, tran);", '\t', p.Name, tableName, string.Join(", ", p.ForeignKey.Select(k => k.FromName))));
            });

            var str = string.Format(DALTemplate.DELETE_TEMPLATE2,
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb3.ToString().TrimEnd("and "),
                                    table_config.Comment,
                                    sb4.ToString().TrimEnd(", ".ToCharArray()),
                                    sb5.ToString());

            return str;
        }

        public List<string> Get_Delete_Child(string tableName)
        {
            var ret = new List<string>();
            var table_config = _config[tableName];
            var find = _config.Tables.Where(p => p.ReferenceTable.Exists(k => k.Name.ToLower() == tableName.ToLower()));
            if (find != null)
            {
                foreach (var item in find)
                {
                    var f_key = item.ReferenceTable.Find(p => p.Name.ToLower() == tableName.ToLower());

                    var sb1 = new StringBuilder();
                    f_key.ForeignKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.ToName, p.ToName)));

                    var sb2 = new StringBuilder();
                    f_key.ForeignKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.ToName)));

                    var sb3 = new StringBuilder();
                    f_key.ForeignKey.ForEach(p => sb3.Append(string.Format("[{0}]=@{1} and ", p.ToName, p.ToName)));

                    var sb4 = new StringBuilder();
                    f_key.ForeignKey.ForEach(p => sb4.Append(string.Format("@{0}={1}, ", p.ToName, p.ToName)));

                    var str = string.Format(DALTemplate.DELETE_CHILD_TEMPLATE1,
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    item.Name,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb3.ToString().TrimEnd("and "),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()),
                                    sb4.ToString().TrimEnd(", ".ToCharArray()));

                    ret.Add(str);
                }
            }

            return ret;
        }
        #endregion

        #region BatchDelete
        public string Get_BatchDelete(string tableName)
        {
            var table_config = _config[tableName];
            if (table_config.PrimaryKey.Count == 1)
            {
                return Get_BatchDelete_SingleId(tableName);
            }
            else
            {
                return Get_BatchDelete_MultipleId(tableName);
            }
        }

        private string Get_BatchDelete_SingleId(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey[0];

            var str = string.Format(DALTemplate.BATCHDELETE_TEMPLATE1,
                                    table_config.Comment,
                                    table_config.Comment,
                                    primaryKey.DbType,
                                    string.Format("[{0}]", tableName),
                                    string.Format("[{0}]", primaryKey.Name));

            return str;
        }

        private string Get_BatchDelete_SingleId_WithChildTable(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey[0];

            var sb1 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p =>
            {
                sb1.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}// 删除相关的{1}信息", '\t', p.Name));
                sb1.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{1}Helper.DeleteFor{2}({3}, conn, tran);", '\t', p.Name, tableName, "ids"));
            });

            var str = string.Format(DALTemplate.BATCHDELETE_TEMPLATE2,
                                    table_config.Comment,
                                    table_config.Comment,
                                    primaryKey.DbType,
                                    string.Format("[{0}]", tableName),
                                    string.Format("[{0}]", primaryKey.Name),
                                    table_config.Comment,
                                    sb1.ToString());

            return str;
        }

        private string Get_BatchDelete_MultipleId(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            sb1.Append("Tuple<");
            table_config.PrimaryKey.ForEach(p => sb1.Append(string.Format("{0}, ", p.DbType)));
            sb1.Append(">");

            var sb2 = new StringBuilder();
            sb2.Append("(");
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("[{0}], ", p.Name)));
            sb2.Append(")");

            var str = string.Format(DALTemplate.BATCHDELETE_TEMPLATE3,
                                    table_config.Comment,
                                    table_config.Comment,
                                    sb1.ToString().Replace(", >", "") + ">",
                                    string.Format("[{0}]", tableName),
                                    sb2.ToString().Replace(", )", "") + ")");

            return str;
        }

        private string Get_BatchDelete_MultipleId_WithChildTable(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            sb1.Append("Tuple<");
            table_config.PrimaryKey.ForEach(p => sb1.Append(string.Format("{0}, ", p.DbType)));
            sb1.Append(">");

            var sb2 = new StringBuilder();
            sb2.Append("(");
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("[{0}], ", p.Name)));
            sb2.Append(")");

            var sb3 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p =>
            {
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}// 删除相关的{1}信息", '\t', p.Name));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{1}Helper.DeleteFor{2}({3}, conn, tran);",'\t', p.Name, tableName, "ids"));
            });

            var str = string.Format(DALTemplate.BATCHDELETE_TEMPLATE4,
                                    table_config.Comment,
                                    table_config.Comment,
                                    sb1.ToString().Replace(", >", "") + ">",
                                    string.Format("[{0}]", tableName),
                                    sb2.ToString().Replace(", )", "") + ")",
                                    table_config.Comment,
                                    sb3.ToString());

            return str;
        }

        public List<string> Get_BatchDeleteChild_SingleId(string tableName)
        {
            var ret = new List<string>();
            var table_config = _config[tableName];
            var find = _config.Tables.Where(p => p.ReferenceTable.Exists(k => k.Name.ToLower() == tableName.ToLower()));
            if (find != null)
            {
                foreach (var item in find)
                {
                    var f_key = item.ReferenceTable.Find(p => p.Name.ToLower() == tableName.ToLower()).ForeignKey[0];
                    var str = string.Format(DALTemplate.BATCHDELETE_CHILD_TEMPLATE1,
                                    table_config.Comment,
                                    item.Name,
                                    item.Name,
                                    f_key.DbType,
                                    tableName,
                                    f_key.ToName);

                    ret.Add(str);
                }
            }

            return ret;
        }

        public List<string> Get_BatchDeleteChild_MultipleId(string tableName)
        {
            var ret = new List<string>();
            var table_config = _config[tableName];
            var find = _config.Tables.Where(p => p.ReferenceTable.Exists(k => k.Name.ToLower() == tableName.ToLower()));
            if (find != null)
            {
                foreach (var item in find)
                {
                    var f_key = item.ReferenceTable.Find(p => p.Name.ToLower() == tableName.ToLower());

                    var sb1 = new StringBuilder();
                    sb1.Append("Tuple<");
                    f_key.ForeignKey.ForEach(p => sb1.Append(string.Format("{0}, ", p.DbType)));
                    sb1.Append(">");

                    var sb2 = new StringBuilder();
                    sb2.Append("(");
                    f_key.ForeignKey.ForEach(p => sb2.Append(string.Format("[{0}], ", p.ToName)));
                    sb2.Append(")");

                    var str = string.Format(DALTemplate.BATCHDELETE_CHILD_TEMPLATE2,
                                    table_config.Comment,
                                    item.Name,
                                    item.Name,
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()));

                    ret.Add(str);
                }
            }

            return ret;
        }
        #endregion

        #region Update
        public string Get_Update(string tableName)
        {
            var table_config = _config[tableName];
            if (table_config.ReferenceTable.Count == 0)
            {
                return Get_Update1(tableName);
            }
            else
            {
                return Get_Update_WithChildTable(tableName);
            }
        }

        private string Get_Update1(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey.Find(p => p.Name.ToLower() == "id");

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity && p.Name.ToLower() != "createdtime" && p.Name.ToLower() != "creatorid")
                    sb1.Append(string.Format("[{0}]=@{1}, ", p.Name, p.Name));
            });

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("[{0}]=@{1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.UPDATE_TEMPLATE1,
                                    table_config.Comment,
                                    table_config.Comment,
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }

        private string Get_Update_WithChildTable(string tableName)
        {
            var table_config = _config[tableName];
            var primaryKey = table_config.PrimaryKey.Find(p => p.Name.ToLower() == "id");

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p =>
            {
                if (!p.IsIdentity && p.Name.ToLower() != "createdtime" && p.Name.ToLower() != "creatorid")
                    sb1.Append(string.Format("[{0}]=@{1}, ", p.Name, p.Name));
            });

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("[{0}]=@{1}, ", p.Name, p.Name)));

            var sb3 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p =>
            {
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}// 更新相关的{1}信息", '\t', p.Name));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}foreach (var {1} in model.{2}List) ", '\t', p.Name, p.Name));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{{", '\t'));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}{1}Helper.Update({2}, conn, tran);", '\t', p.Name, p.Name));
                sb3.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}}}", '\t'));
            });

            var str = string.Format(DALTemplate.UPDATE_TEMPLATE2,
                                    table_config.Comment,
                                    table_config.Comment,
                                    tableName,
                                    string.Format("[{0}]", tableName),
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    tableName,
                                    sb3.ToString());

            return str;
        }
        #endregion

        #region Select
        public string Get_GetModel(string tableName)
        {
            var table_config = _config[tableName];
            if (table_config.ReferenceTable.Count == 0)
            {
                return Get_GetModel1(tableName);
            }
            else
            {
                return Get_GetModel_WithChildTable(tableName);
            }
        }

        private string Get_GetModel1(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table_config.Columns.ForEach(p => sb3.Append(string.Format("[{0}], ", p.Name)));

            var sb4 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb4.Append(string.Format("[{0}]=@{1} and ", p.Name, p.Name)));

            var sb5 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb5.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.GET_MODEL_TEMPLATE1,
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    tableName,
                                    tableName,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    sb3.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb4.ToString().TrimEnd("and "),
                                    tableName,
                                    tableName,
                                    sb5.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }

        private string Get_GetModel_WithChildTable(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb1.AppendLine(string.Format("{0}{0}/// <param name=\"{1}\">{2}</param>", '\t', p.Name, p.Comment)));

            var sb2 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb2.Append(string.Format("{0} {1}, ", p.DbType, p.Name)));

            var sb3 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p => sb3.Append(string.Format("[{0}].*, ", p.Name)));

            var join = new Dictionary<string, string>();
            table_config.ReferenceTable.ForEach(p => join.Add(p.Name, string.Join(" and ", p.ForeignKey.Select(k => string.Format("[{0}].[{1}]=[{2}].[{3}]", p.Name, k.ToName, tableName, k.FromName)))));

            var sb4 = new StringBuilder();
            foreach (var item in join)
            {
                sb4.Append(string.Format(" INNER JOIN [{0}] ON {1} ", item.Key, item.Value));
            }

            var sb5 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb5.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var sb6 = new StringBuilder();
            if (table_config.PrimaryKey.Count == 1)
            {
                sb6.Append(string.Format("{0}, {1}", table_config.PrimaryKey[0].DbType, tableName));
            }
            else
            {
                sb6.Append("Tuple<");
                table_config.PrimaryKey.ForEach(p => sb6.Append(string.Format("{0}, ", p.DbType)));
                sb6.Append(">");
                sb6.Append(", " + tableName);
            }

            var sb7 = new StringBuilder();
            sb7.Append(tableName + ", ");
            table_config.ReferenceTable.ForEach(p => sb7.Append(p.Name + ", "));
            sb7.Append(tableName);

            var sb8 = new StringBuilder();
            sb8.Append(tableName.ToLower() + ", ");
            table_config.ReferenceTable.ForEach(p => sb8.Append(p.Name.ToLower() + ", "));

            var sb9 = new StringBuilder();
            if (table_config.PrimaryKey.Count == 1)
            {
                sb9.Append(string.Format("{0}.{1}", tableName.ToLower(), table_config.PrimaryKey[0].Name));
            }
            else
            {
                sb9.Append("new Tuple<");
                table_config.PrimaryKey.ForEach(p => sb9.Append(string.Format("{0}, ", p.DbType)));
                sb9.Append(">(");
                table_config.PrimaryKey.ForEach(p => sb9.Append(string.Format("{0}.{1}, ", tableName.ToLower(), p.Name)));
                sb9.Append(")");
            }

            var sb10 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p =>
            {
                sb10.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}if (tmp_obj.{1}List == null)", '\t', p.Name));
                sb10.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}{{", '\t'));
                sb10.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}{0}tmp_obj.{1}List = new List<{1}>();", '\t', p.Name));
                sb10.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}}}", '\t'));
                sb10.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}tmp_obj.{1}List.Add({2});", '\t', p.Name, p.Name.ToLower()));
            });

            var sb11 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb11.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.GET_MODEL_TEMPLATE2,
                                    table_config.Comment,
                                    sb1.ToString().TrimEnd(Environment.NewLine),
                                    tableName,
                                    tableName,
                                    sb2.ToString().TrimEnd(", ".ToCharArray()),
                                    sb3.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb4.ToString(),
                                    sb5.ToString().TrimEnd(", ".ToCharArray()),
                                    tableName,
                                    sb6.ToString(),
                                    sb7.ToString(),
                                    sb8.ToString().TrimEnd(", ".ToCharArray()),
                                    tableName,
                                    sb9.ToString(),
                                    sb9.ToString(),
                                    tableName.ToLower(),
                                    sb10.ToString(),
                                    sb11.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }

        public string Get_GetList(string tableName)
        {
            var table_config = _config[tableName];
            if (table_config.ReferenceTable.Count == 0)
            {
                return Get_GetList1(tableName);
            }
            else
            {
                return Get_GetList_WithChildTable(tableName);
            }
        }

        private string Get_GetList1(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p => sb1.Append(string.Format("[{0}], ", p.Name)));

            var str = string.Format(DALTemplate.GET_LIST_TEMPLATE1,
                                    table_config.Comment,
                                    tableName,
                                    tableName,
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    tableName,
                                    tableName);

            return str;
        }

        private string Get_GetList_WithChildTable(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p => sb1.Append(string.Format("[{0}].*, ", p.Name)));

            var join = new Dictionary<string, string>();
            table_config.ReferenceTable.ForEach(p => join.Add(p.Name, string.Join(" and ", p.ForeignKey.Select(k => string.Format("[{0}].[{1}]=[{2}].[{3}]", p.Name, k.ToName, tableName, k.FromName)))));

            var sb2 = new StringBuilder();
            foreach (var item in join)
            {
                sb2.Append(string.Format(" INNER JOIN [{0}] ON {1} ", item.Key, item.Value));
            }

            var sb3 = new StringBuilder();
            if (table_config.PrimaryKey.Count == 1)
            {
                sb3.Append(string.Format("{0}, {1}", table_config.PrimaryKey[0].DbType, tableName));
            }
            else
            {
                sb3.Append("Tuple<");
                table_config.PrimaryKey.ForEach(p => sb3.Append(string.Format("{0}, ", p.DbType)));
                sb3.Append(">");
                sb3.Append(", " + tableName);
            }

            var sb4 = new StringBuilder();
            sb4.Append(tableName + ", ");
            table_config.ReferenceTable.ForEach(p => sb4.Append(p.Name + ", "));
            sb4.Append(tableName);

            var sb5 = new StringBuilder();
            sb5.Append(tableName.ToLower() + ", ");
            table_config.ReferenceTable.ForEach(p => sb5.Append(p.Name.ToLower() + ", "));

            var sb6 = new StringBuilder();
            if (table_config.PrimaryKey.Count == 1)
            {
                sb6.Append(string.Format("{0}.{1}", tableName.ToLower(), table_config.PrimaryKey[0].Name));
            }
            else
            {
                sb6.Append("new Tuple<");
                table_config.PrimaryKey.ForEach(p => sb6.Append(string.Format("{0}, ", p.DbType)));
                sb6.Append(">(");
                table_config.PrimaryKey.ForEach(p => sb6.Append(string.Format("{0}.{1}, ", tableName.ToLower(), p.Name)));
                sb6.Append(")");
            }

            var sb7 = new StringBuilder();
            table_config.ReferenceTable.ForEach(p =>
            {
                sb7.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}if (tmp_obj.{1}List == null)", '\t', p.Name));
                sb7.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}{{", '\t'));
                sb7.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}{0}tmp_obj.{1}List = new List<{1}>();", '\t', p.Name));
                sb7.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}}}", '\t'));
                sb7.AppendLine(string.Format("{0}{0}{0}{0}{0}{0}{0}tmp_obj.{1}List.Add({2});", '\t', p.Name, p.Name.ToLower()));
            });

            var sb8 = new StringBuilder();
            table_config.PrimaryKey.ForEach(p => sb8.Append(string.Format("@{0}={1}, ", p.Name, p.Name)));

            var str = string.Format(DALTemplate.GET_LIST_TEMPLATE2,
                                    table_config.Comment,
                                    tableName,
                                    tableName,
                                    sb1.ToString().TrimEnd(", ".ToCharArray()),
                                    string.Format("[{0}]", tableName),
                                    sb2.ToString(),
                                    tableName,
                                    sb3.ToString(),
                                    sb4.ToString(),
                                    sb5.ToString().TrimEnd(", ".ToCharArray()),
                                    tableName,
                                    sb6.ToString(),
                                    sb6.ToString(),
                                    tableName.ToLower(),
                                    sb7.ToString(),
                                    sb8.ToString().TrimEnd(", ".ToCharArray()));

            return str;
        }
        #endregion

        #region Page
        public string Get_GetRecordCount(string tableName)
        {
            var table_config = _config[tableName];

            var str = string.Format(DALTemplate.GET_RECORD_COUNT_TEMPLATE,
                                    string.Format("[{0}]", tableName));

            return str;
        }

        public string Get_GetListByPage(string tableName)
        {
            var table_config = _config[tableName];

            var str = string.Format(DALTemplate.GET_LIST_BY_PAGE_TEMPLATE,
                                    tableName,
                                    tableName,
                                    tableName);

            return str;
        }
        #endregion
    }
}
