using Generator.Template;
using System.Text;

namespace Generator.Core
{
    public class ModelGenerator
    {
        private SQLMetaData _config;

        public ModelGenerator(SQLMetaData config)
        {
            this._config = config;
        }

        public string Get_Class(string tableName)
        {
            var table_config = _config[tableName];

            var sb1 = new StringBuilder();
            table_config.Columns.ForEach(p => {
                if (_config.MongoSupport_Model && p.IsPrimaryKey)
                {
                    sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', "ObjectId", p.Name.ToLower()));
                }
                else
                {
                    sb1.AppendLine(string.Format("{0}{0}private {1} _{2};", '\t', p.DbType, p.Name.ToLower()));
                }
            });

            var sb2 = new StringBuilder();
            for (int i = 0; i < table_config.Columns.Count; i++)
            {
                var p = table_config.Columns[i];
                if (_config.MongoSupport_Model && p.IsPrimaryKey)
                {
                    sb2.AppendLine(string.Format("{0}{0}/// <summary>", '\t'));
                    sb2.AppendLine(string.Format("{0}{0}/// {1}", '\t', p.Comment));
                    sb2.AppendLine(string.Format("{0}{0}/// </summary>", '\t'));
                    sb2.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', "ObjectId", p.Name));
                    sb2.AppendLine(string.Format("{0}{0}{{", '\t'));
                    sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; }}", '\t', p.Name.ToLower()));
                    sb2.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                    sb2.AppendLine(string.Format("{0}{0}}}", '\t'));
                    sb2.AppendLine();
                }
                else
                {
                    if (i == table_config.Columns.Count - 1)
                    {
                        sb2.AppendLine(string.Format("{0}{0}/// <summary>", '\t'));
                        sb2.AppendLine(string.Format("{0}{0}/// {1}", '\t', p.Comment));
                        sb2.AppendLine(string.Format("{0}{0}/// </summary>", '\t'));
                        if (p.Nullable)
                        {
                            sb2.AppendLine(string.Format("{0}{0}public {1}? {2}", '\t', p.DbType, p.Name));
                        }
                        else
                        {
                            sb2.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', p.DbType, p.Name));
                        }
                        sb2.AppendLine(string.Format("{0}{0}{{", '\t'));
                        sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; }}", '\t', p.Name.ToLower()));
                        sb2.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                        sb2.Append(string.Format("{0}{0}}}", '\t'));
                    }
                    else
                    {
                        sb2.AppendLine(string.Format("{0}{0}/// <summary>", '\t'));
                        sb2.AppendLine(string.Format("{0}{0}/// {1}", '\t', p.Comment));
                        sb2.AppendLine(string.Format("{0}{0}/// </summary>", '\t'));
                        if (p.Nullable)
                        {
                            sb2.AppendLine(string.Format("{0}{0}public {1}? {2}", '\t', p.DbType, p.Name));
                        }
                        else
                        {
                            sb2.AppendLine(string.Format("{0}{0}public {1} {2}", '\t', p.DbType, p.Name));
                        }
                        sb2.AppendLine(string.Format("{0}{0}{{", '\t'));
                        sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1} = value; }}", '\t', p.Name.ToLower()));
                        sb2.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}; }}", '\t', p.Name.ToLower()));
                        sb2.AppendLine(string.Format("{0}{0}}}", '\t'));
                        sb2.AppendLine();
                    }
                }
            }

            foreach (var ref_table in table_config.ReferenceTable)
            {
                sb1.AppendLine(string.Format("{0}{0}private List<{1}> _{2}list;", '\t', ref_table.Name, ref_table.Name.ToLower()));
                sb2.AppendLine(string.Format("{0}{0}/// <summary>", '\t'));
                sb2.AppendLine(string.Format("{0}{0}/// {1} 集合", '\t', ref_table.Name));
                sb2.AppendLine(string.Format("{0}{0}/// </summary>", '\t'));
                sb2.AppendLine(string.Format("{0}{0}public List<{1}> {1}List", '\t', ref_table.Name));
                sb2.AppendLine(string.Format("{0}{0}{{", '\t'));
                sb2.AppendLine(string.Format("{0}{0}{0}set {{ _{1}list = value; }}", '\t', ref_table.Name.ToLower()));
                sb2.AppendLine(string.Format("{0}{0}{0}get {{ return _{1}list; }}", '\t', ref_table.Name.ToLower()));
                sb2.AppendLine(string.Format("{0}{0}}}", '\t'));
            }

            var str = string.Format(ModelTemplate.CLASS_TEMPLATE,
                                    table_config.Comment,
                                    _config.Model_ClassNamePrefix,
                                    tableName,
                                    _config.Model_ClassNameSuffix,
                                    string.IsNullOrWhiteSpace(_config.Model_BaseClass) ? string.Empty : (" : " + _config.Model_BaseClass),
                                    tableName,
                                    sb1.ToString(),
                                    sb2.ToString());
            return str;
        }
    }
}
