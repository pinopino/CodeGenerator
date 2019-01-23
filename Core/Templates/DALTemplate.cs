using Generator.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.Template
{
    public class DALTemplate
    {
        #region Exists
        private readonly static string _EXISTS_TEMPLATE = @"
        /// <summary>
        /// 是否存在指定的{0}
        /// </summary>
        {1}
        /// <returns>是否存在，true为存在</returns>
        public static bool Exists({2})
        {{
            var sql = new StringBuilder();
            sql.Append(""SELECT COUNT(1) FROM {3}"");
            sql.Append("" WHERE {4}"");
            var ret = false;
            using (var conn = GetOpenConnection())
            {{
                ret = conn.ExecuteScalar<int>(sql.ToString(), new {{ {5} }}) > 0;
            }}

            return ret;
        }}";

        public readonly static string EXISTS_TEMPLATE = GetTemplateFile("EXISTS_TEMPLATE");
        #endregion

        #region Insert
        #region Insert_SingleId
        private readonly static string _INSERT_TEMPLATE1 = @"
        /// <summary>
        /// 添加{0}
        /// </summary>
        /// <param name=""model"">{1}实体</param>
        /// <returns>新插入数据的id</returns>
        public static {2} Insert({3} model, SqlConnection conn = null, SqlTransaction transaction = null)
        {{
            var sql = new StringBuilder();
            sql.Append(""INSERT INTO {4}({5})"");
            sql.Append("" OUTPUT INSERTED.[{6}]"");
            sql.Append(""VALUES({7})"");
            var ret = {8};
            if (conn != null)
            {{
                if (transaction == null)
                {{
                    throw new ArgumentNullException(""transaction"");
                }}
                ret = conn.ExecuteScalar<{9}>(sql.ToString(), model, transaction);
            }}
            else
            {{
                using (var conn1 = GetOpenConnection())
                {{
                    ret = conn1.ExecuteScalar<{10}>(sql.ToString(), model);
                }}
            }}

            return ret;
        }}";

        public readonly static string INSERT_TEMPLATE1 = GetTemplateFile("INSERT_TEMPLATE1");
        #endregion

        #region Insert_SingleId_WithChildTable
        private readonly static string _INSERT_TEMPLATE2 = @"
        /// <summary>
        /// 添加{0}
        /// </summary>
        /// <param name=""model"">{1}实体</param>
        /// <returns>新插入数据的id</returns>
        public static {2} Insert({3} model)
        {{
            var sql = new StringBuilder();
            sql.Append(""INSERT INTO {4}({5}) "");
            sql.Append("" OUTPUT INSERTED.[{6}] "");
            sql.Append("" VALUES({7}) "");
            var ret = {8};
            SqlConnection conn = null;
            SqlTransaction tran = null;
            try
            {{
                using (conn = GetOpenConnection())
                {{
                    using (tran = conn.BeginTransaction())
                    {{
                        // 插入{9}基本信息
                        ret = conn.ExecuteScalar<{10}>(sql.ToString(), model, tran);
                        
                        {11}
                        // 提交事务
                        tran.Commit();
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                // todo: 异常记录
                ret = {12};
            }}
            finally
            {{
                if ({13})
                {{
                    // 回滚事务
                    tran.Rollback();
                }}
            }}

            return ret;
        }}";

        public readonly static string INSERT_TEMPLATE2 = GetTemplateFile("INSERT_TEMPLATE2");
        #endregion

        #region Insert_MultipleId
        private readonly static string _INSERT_TEMPLATE3 = @"
        /// <summary>
        /// 添加{0}
        /// </summary>
        /// <param name=""model"">{1}实体</param>
        /// <returns>新插入数据的标识</returns>
        public static {2} Insert({3} model, SqlConnection conn = null, SqlTransaction transaction = null)
        {{
            var sql = new StringBuilder();
            sql.Append(""INSERT INTO {4}({5})"");
            sql.Append("" OUTPUT {6} "");
            sql.Append("" VALUES({7})"");
            object ret = null;
            if (conn != null)
            {{
                if (transaction == null)
                {{
                    throw new ArgumentNullException(""transaction"");
                }}
                ret = conn.QuerySingle<{8}>(sql.ToString(), model, transaction);
            }}
            else
            {{
                using (var conn1 = GetOpenConnection())
                {{
                    ret = conn1.QuerySingle<{9}>(sql.ToString(), model);
                }}
            }}

            return ({10})ret;
        }}";

        public readonly static string INSERT_TEMPLATE3 = GetTemplateFile("INSERT_TEMPLATE3");
        #endregion

        #region Insert_MultipleId_WithChildTable
        private readonly static string _INSERT_TEMPLATE4 = @"
        /// <summary>
        /// 添加{0}
        /// </summary>
        /// <param name=""model"">{1}实体</param>
        /// <returns>新插入数据的标识</returns>
        public static {2} Insert({3} model)
        {{
            var sql = new StringBuilder();
            sql.Append(""INSERT INTO {4}({5}) "");
            sql.Append("" OUTPUT {6} "");
            sql.Append("" VALUES({7}) "");
            object ret = null;
            SqlConnection conn = null;
            SqlTransaction tran = null;
            try
            {{
                using (conn = GetOpenConnection())
                {{
                    using (tran = conn.BeginTransaction())
                    {{
                        // 插入{8}基本信息
                        ret = conn.QuerySingle<{9}>(sql.ToString(), model, tran);
                        
                        {10}
                        // 提交事务
                        tran.Commit();
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                // todo: 异常记录
                ret = null;
            }}
            finally
            {{
                if (ret == null)
                {{
                    // 回滚事务
                    tran.Rollback();
                }}
            }}

            return ({11})ret;
        }}";

        public readonly static string INSERT_TEMPLATE4 = GetTemplateFile("INSERT_TEMPLATE4");
        #endregion
        #endregion

        #region Delete
        #region Delete_Single&MultipleId
        private readonly static string _DELETE_TEMPLATE1 = @"
        /// <summary>
        /// 删除指定的{0}
        /// </summary>
        {1}
        /// <returns>是否成功，true为成功</returns>
        public static bool Delete({2})
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {3} "");
            sql.Append("" WHERE {4}"");
            var ret = false;
            using (var conn = GetOpenConnection())
            {{
                ret = conn.Execute(sql.ToString(), new {{ {5} }}) > 0;
            }}

            return ret;
        }}";

        public readonly static string DELETE_TEMPLATE1 = GetTemplateFile("DELETE_TEMPLATE1");
        #endregion

        #region Delete_Single&MultipleId_WithChildTable
        private readonly static string _DELETE_TEMPLATE2 = @"
        /// <summary>
        /// 删除指定的{0}
        /// </summary>
        {1}
        /// <returns>是否成功，true为成功</returns>
        public static bool Delete({2})
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {3} "");
            sql.Append("" WHERE {4}"");
            var ret = false;
            SqlConnection conn = null;
            SqlTransaction tran = null;
            try
            {{
                using (conn = GetOpenConnection())
                {{
                    using (tran = conn.BeginTransaction())
                    {{
                        // 删除{5}基本信息
                        ret = conn.Execute(sql.ToString(), new {{ {6} }}) > 0;

                        {7}
                        // 提交事务
                        tran.Commit();
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                // todo: 异常记录
                ret = false;
            }}
            finally
            {{
                if (ret == false)
                {{
                    // 回滚事务
                    tran.Rollback();
                }}
            }}

            return ret;
        }}";

        public readonly static string DELETE_TEMPLATE2 = GetTemplateFile("DELETE_TEMPLATE2");
        #endregion

        #region Delete_Child_Single&MultipleId
        private readonly static string _DELETE_CHILD_TEMPLATE1 = @"
        /// <summary>
        /// 删除指定的所有{0}
        /// </summary>
        {1}
        /// <returns>是否成功，true为成功</returns>
        public static bool DeleteFor{2}({3}, SqlConnection conn = null, SqlTransaction transaction = null)
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {4} "");
            sql.Append("" WHERE {5}"");
            var ret = false;
            if (conn != null)
            {{
                if (transaction == null)
                {{
                    throw new ArgumentNullException(""transaction"");
                }}
                ret = conn.Execute(sql.ToString(), new {{ {6} }}) > 0;
            }}
            else
            {{
                using (var conn1 = GetOpenConnection())
                {{
                    ret = conn1.Execute(sql.ToString(), new {{ {7} }}) > 0;
                }}
            }}

            return ret;
        }}";

        public readonly static string DELETE_CHILD_TEMPLATE1 = GetTemplateFile("DELETE_CHILD_TEMPLATE1");
        #endregion
        #endregion

        #region BatchDelete
        #region BatchDelete_SingleId
        private readonly static string _BATCHDELETE_TEMPLATE1 = @"
        /// <summary>
        /// 批量删除指定的{0}
        /// </summary>
        /// <param name=""ids"">{1} id列表</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool Delete(List<{2}> ids)
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {3} "");
            sql.Append("" WHERE {4} IN @ids"");
            var ret = false;
            using (var conn = GetOpenConnection())
            {{
                ret = conn.Execute(sql.ToString(), new {{ @ids = ids.ToArray() }}) > 0;
            }}

            return ret;
        }}";

        public readonly static string BATCHDELETE_TEMPLATE1 = GetTemplateFile("BATCHDELETE_TEMPLATE1");
        #endregion

        #region BatchDelete_SingleId_WithChildTable
        private readonly static string _BATCHDELETE_TEMPLATE2 = @"
        /// <summary>
        /// 批量删除指定的{0}
        /// </summary>
        /// <param name=""ids"">{1} id列表</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool Delete(List<{2}> ids)
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {3} "");
            sql.Append("" WHERE {4} IN @ids"");
            var ret = false;
            SqlConnection conn = null;
            SqlTransaction tran = null;
            try
            {{
                using (conn = GetOpenConnection())
                {{
                    using (tran = conn.BeginTransaction())
                    {{
                        // 删除{5}基本信息
                        ret = conn.Execute(sql.ToString(), new {{ @ids = ids.ToArray() }}) > 0;

                        {6}
                        // 提交事务
                        tran.Commit();
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                // todo: 异常记录
                ret = false;
            }}
            finally
            {{
                if (ret == false)
                {{
                    // 回滚事务
                    tran.Rollback();
                }}
            }}

            return ret;
        }}";

        public readonly static string BATCHDELETE_TEMPLATE2 = GetTemplateFile("BATCHDELETE_TEMPLATE2");
        #endregion

        #region BatchDelete_MultipleId
        private readonly static string _BATCHDELETE_TEMPLATE3 = @"
        /// <summary>
        /// 批量删除指定的{0}
        /// </summary>
        /// <param name=""ids"">{1} 标识列表</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool Delete(List<{2}> ids)
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {3} "");
            sql.Append("" WHERE {4} IN @ids"");
            var ret = false;
            using (var conn = GetOpenConnection())
            {{
                ret = conn.Execute(sql.ToString(), new {{ @ids = ids.ToArray() }}) > 0;
            }}

            return ret;
        }}";

        public readonly static string BATCHDELETE_TEMPLATE3 = GetTemplateFile("BATCHDELETE_TEMPLATE3");
        #endregion

        #region BatchDelete_MultipleId_WithChildTable
        private readonly static string _BATCHDELETE_TEMPLATE4 = @"
        /// <summary>
        /// 批量删除指定的{0}
        /// </summary>
        /// <param name=""ids"">{1} 标识列表</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool Delete(List<{2}> ids)
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {3} "");
            sql.Append("" WHERE {4} IN @ids"");
            var ret = false;
            SqlConnection conn = null;
            SqlTransaction tran = null;
            try
            {{
                using (conn = GetOpenConnection())
                {{
                    using (tran = conn.BeginTransaction())
                    {{
                        // 删除{5}基本信息
                        ret = conn.Execute(sql.ToString(), new {{ @ids = ids.ToArray() }}) > 0;

                        {6}
                        // 提交事务
                        tran.Commit();
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                // todo: 异常记录
                ret = false;
            }}
            finally
            {{
                if (ret == false)
                {{
                    // 回滚事务
                    tran.Rollback();
                }}
            }}

            return ret;
        }}";

        public readonly static string BATCHDELETE_TEMPLATE4 = GetTemplateFile("BATCHDELETE_TEMPLATE4");
        #endregion

        #region BatchDelete_Child_SingleId
        private readonly static string _BATCHDELETE_CHILD_TEMPLATE1 = @"
        /// <summary>
        /// 删除指定的所有{0}
        /// </summary>
        /// <param name=""ids"">{1} id列表</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool DeleteFor{2}(List<{3}> ids, SqlConnection conn = null, SqlTransaction transaction = null)
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {4} "");
            sql.Append("" WHERE {5} IN @ids"");
            var ret = false;
            if (conn != null)
            {{
                if (transaction == null)
                {{
                    throw new ArgumentNullException(""transaction"");
                }}
                ret = conn.Execute(sql.ToString(), new {{ @ids = ids.ToArray() }}) > 0;
            }}
            else
            {{
                using (var conn1 = GetOpenConnection())
                {{
                    ret = conn1.Execute(sql.ToString(), new {{ @ids = ids.ToArray() }}) > 0;
                }}
            }}

            return ret;
        }}";

        public readonly static string BATCHDELETE_CHILD_TEMPLATE1 = GetTemplateFile("BATCHDELETE_CHILD_TEMPLATE1");
        #endregion

        #region BatchDelete_Child_MultipleId
        private readonly static string _BATCHDELETE_CHILD_TEMPLATE2 = @"
        /// <summary>
        /// 删除指定的所有{0}
        /// </summary>
        /// <param name=""ids"">{1} 标识列表</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool DeleteFor{2}(List<{3}> ids, SqlConnection conn = null, SqlTransaction transaction = null)
        {{
            var sql = new StringBuilder();
            sql.Append(""DELETE FROM {4} "");
            sql.Append("" WHERE {5} IN @ids"");
            var ret = false;
            if (conn != null)
            {{
                if (transaction == null)
                {{
                    throw new ArgumentNullException(""transaction"");
                }}
                ret = conn.Execute(sql.ToString(), new {{ @ids = ids.ToArray() }}) > 0;
            }}
            else
            {{
                using (var conn1 = GetOpenConnection())
                {{
                    ret = conn1.Execute(sql.ToString(), new {{ @ids = ids.ToArray() }}) > 0;
                }}
            }}

            return ret;
        }}";

        public readonly static string BATCHDELETE_CHILD_TEMPLATE2 = GetTemplateFile("BATCHDELETE_CHILD_TEMPLATE2");
        #endregion
        #endregion

        #region Update
        #region Update_Single&MultipleId
        private readonly static string _UPDATE_TEMPLATE1 = @"
        /// <summary>
        /// 更新{0}
        /// </summary>
        /// <param name=""model"">{1}实体</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool Update({2} model, SqlConnection conn = null, SqlTransaction transaction = null)
        {{
            var sql = new StringBuilder();
            sql.Append(""UPDATE {3}"");
            sql.Append("" SET {4}"");
            sql.Append("" WHERE {5}"");
            var ret = false;
            if (conn != null)
            {{
                if (transaction == null)
                {{
                    throw new ArgumentNullException(""transaction"");
                }}
                ret = conn.Execute(sql.ToString(), model, transaction) > 0;
            }}
            else
            {{
                using (var conn1 = GetOpenConnection())
                {{
                    ret = conn1.Execute(sql.ToString(), model) > 0;
                }}
            }}

            return ret;
        }}";

        public readonly static string UPDATE_TEMPLATE1 = GetTemplateFile("UPDATE_TEMPLATE1");
        #endregion
        #region Update_Single&MultipleId fields
        private readonly static string _UPDATE_TEMPLATE3 = @"
        /// <summary>
        /// 更新{0}
        /// </summary>
        /// <param name=""model"">{1}实体</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool UpdateFields({2} model, SqlConnection conn = null, SqlTransaction transaction = null,params string[] fields)
        {{
            var sql = new StringBuilder();
            sql.Append(""UPDATE {3}"");
            if (fields == null || fields.Count() == 0)
            {{
                 sql.Append("" SET {4}"");
            }}
            else
            {{
                sql.Append("" SET "");
                for (int i = 0; i < fields.Count(); i++)
                {{
                    sql.Append($""["" + fields[i] + ""]=@"" + fields[i] + """");
                    if (i != fields.Count() - 1)
                    {{
                        sql.Append("","");
                    }}
                }}
            }}

            sql.Append("" WHERE {5}"");
            var ret = false;
            if (conn != null)
            {{
                if (transaction == null)
                {{
                    throw new ArgumentNullException(""transaction"");
                }}
                ret = conn.Execute(sql.ToString(), model, transaction) > 0;
            }}
            else
            {{
                using (var conn1 = GetOpenConnection())
                {{
                    ret = conn1.Execute(sql.ToString(), model) > 0;
                }}
            }}

            return ret;
        }}";

        public readonly static string UPDATE_TEMPLATE3 = GetTemplateFile("UPDATE_TEMPLATE3");
        #endregion
        #region Update_Single&MultipleId_WithChildTable
        private readonly static string _UPDATE_TEMPLATE2 = @"
        /// <summary>
        /// 更新{0}
        /// </summary>
        /// <param name=""model"">{1}实体</param>
        /// <returns>是否成功，true为成功</returns>
        public static bool Update({2} model)
        {{
            var sql = new StringBuilder();
            sql.Append(""UPDATE {3}"");
            sql.Append("" SET {4}"");
            sql.Append("" WHERE {5}"");
            var ret = false;
            SqlConnection conn = null;
            SqlTransaction tran = null;
            try
            {{
                using (conn = GetOpenConnection())
                {{
                    using (tran = conn.BeginTransaction())
                    {{
                        // 更新{6}基本信息
                        ret = conn.Execute(sql.ToString(), model) > 0;

                        {7}
                        // 提交事务
                        tran.Commit();
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                // todo: 异常记录
                ret = false;
            }}
            finally
            {{
                if (ret == false)
                {{
                    // 回滚事务
                    tran.Rollback();
                }}
            }}

            return ret;
        }}";

        public readonly static string UPDATE_TEMPLATE2 = GetTemplateFile("UPDATE_TEMPLATE2");
        #endregion
        #endregion

        #region GetModel
        #region GetModel_Single&Multiple
        private readonly static string _GET_MODEL_TEMPLATE1 = @"
        /// <summary>
        /// 获取指定的{0}
        /// </summary>
        {1}
        /// <returns>{2}实体</returns>
        public static {3} GetModel({4})
        {{
            var sql = new StringBuilder();
            sql.Append(""SELECT TOP 1 {5} FROM {6} "");
            sql.Append("" WHERE {7}"");
            {8} ret = null;
            using (var conn = GetOpenConnection())
            {{
                ret = conn.QueryFirst<{9}>(sql.ToString(), new {{ {10} }});
            }}

            return ret;
        }}";

        public readonly static string GET_MODEL_TEMPLATE1 = GetTemplateFile("GET_MODEL_TEMPLATE1");
        #endregion

        #region GetModel_Single&Multiple_WithChildTable
        private readonly static string _GET_MODEL_TEMPLATE2 = @"
        /// <summary>
        /// 获取指定的{0}
        /// </summary>
        {1}
        /// <returns>{2}实体</returns>
        public static {3} GetModel({4})
        {{
            var sql = new StringBuilder();
            sql.Append("" SELECT {5}"");
            sql.Append("" FROM {6}"");
            sql.Append("" {7} "");
            sql.Append("" WHERE {8}"");
            {9} ret = null;
            using (var conn = GetOpenConnection())
            {{
                var lookup = new Dictionary<{10}>();
                ret = conn.Query<{11}>(
                        sql.ToString(),
                        ({12}) =>
                        {{
                            // see: https://stackoverflow.com/questions/7508322/how-do-i-map-lists-of-nested-objects-with-dapper
                            {13} tmp_obj;
                            if (!lookup.TryGetValue({14}, out tmp_obj))
                            {{
                                lookup.Add({15}, tmp_obj = {16});
                            }}
                            {17}
                            return tmp_obj;
                        }},
                        new {{ {18} }})
                        .Single();
            }}

            return ret;
        }}";

        public readonly static string GET_MODEL_TEMPLATE2 = GetTemplateFile("GET_MODEL_TEMPLATE2");
        #endregion
        #endregion

        #region GetList
        #region GetList_Single&Multiple
        private readonly static string _GET_LIST_TEMPLATE1 = @"
        /// <summary>
        /// 批量获取{0}
        /// </summary>
        /// <param name=""where"">查询条件</param>
        /// <param name=""top"">取出前top数的数据</param>
        /// <returns>{1}列表</returns>
        public static List<{2}> GetList(string where = """", int top = 100)
        {{
            var sql = new StringBuilder();
            sql.Append(""SELECT "");
            sql.Append("" TOP "" + top.ToString());
            sql.Append("" {3} "");
            sql.Append("" FROM {4} "");
            if (!string.IsNullOrWhiteSpace(where))
            {{
                sql.Append("" WHERE "" + where);
            }}
            object ret = null;
            using (var conn = GetOpenConnection())
            {{
                ret = conn.Query<{5}>(sql.ToString()).ToList();
            }}

            return (List<{6}>)ret;
        }}";

        public readonly static string GET_LIST_TEMPLATE1 = GetTemplateFile("GET_LIST_TEMPLATE1");
        #endregion

        #region GetList_Single&Multiple_WithChildTable
        private readonly static string _GET_LIST_TEMPLATE2 = @"
        /// <summary>
        /// 批量获取{0}
        /// </summary>
        /// <param name=""where"">查询条件</param>
        /// <returns>{1}列表</returns>
        public static List<{2}> GetList(string where = """")
        {{
            var sql = new StringBuilder();
            sql.Append("" SELECT {3}"");
            sql.Append("" FROM {4}"");
            sql.Append("" {5} "");
            if (!string.IsNullOrWhiteSpace(where))
            {{
                sql.Append("" WHERE "" + where); // todo: 条件中可能需要自动补上APPTask.字样
            }}
            {6} ret = null;
            using (var conn = GetOpenConnection())
            {{
                var lookup = new Dictionary<{7}>();
                ret = conn.Query<{8}>(
                        sql.ToString(),
                        ({9}) =>
                        {{
                            {10} tmp_obj;
                            if (!lookup.TryGetValue({11}, out tmp_obj))
                            {{
                                lookup.Add({12}, tmp_obj = {13});
                            }}
                            {14}
                            return tmp_obj;
                        }},
                        new {{ {15} }})
                        .Single();
            }}

            return ret;
        }}";

        public readonly static string GET_LIST_TEMPLATE2 = GetTemplateFile("GET_LIST_TEMPLATE2");
        #endregion
        #endregion

        #region GetRecordCount
        private readonly static string _GET_RECORD_COUNT_TEMPLATE = @"
        /// <summary>
		/// 获取记录总数
		/// </summary>
        protected static int GetRecordCount(string where = """")
        {{
            var sql = new StringBuilder();
            sql.Append(""SELECT COUNT(1) FROM {0} "");
            if (!string.IsNullOrWhiteSpace(where))
            {{
                if (!where.Trim().StartsWith(""where"", StringComparison.CurrentCultureIgnoreCase))
                {{
                    sql.Append("" WHERE "" + where);
                }}
                else
                {{
                    sql.Append("" "");
                    sql.Append(where);
                    sql.Append("" "");
                }}
            }}
            var ret = -1;
            using (var conn = GetOpenConnection())
            {{
                ret = conn.ExecuteScalar<int>(sql.ToString());
            }}

            return ret;
        }}";

        public readonly static string GET_RECORD_COUNT_TEMPLATE = GetTemplateFile("GET_RECORD_COUNT_TEMPLATE");
        #endregion

        #region GetListByPage
        private readonly static string _GET_LIST_BY_PAGE_TEMPLATE = @"
        /// <summary>
		/// 分页获取数据列表
		/// </summary>
		public static PageDataView<{0}> GetListByPage(string where = """", string orderBy = """", string columns = "" * "", int pageSize = 20, int currentPage = 1)
        {{
            return Paged<{1}>(""{2}"", where, orderBy, columns, pageSize, currentPage);
        }}";

        public readonly static string GET_LIST_BY_PAGE_TEMPLATE = GetTemplateFile("GET_LIST_BY_PAGE_TEMPLATE");
        #endregion

        #region 辅助方法
        private static string GetTemplateFile(string name)
        {
            return DirHelper.FindFile(Path.Combine("Templates", "DAL"), name + ".txt", true);
        }
        #endregion
    }
}
