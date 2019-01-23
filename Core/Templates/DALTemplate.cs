using Generator.Common;
using System.IO;

namespace Generator.Template
{
    public class DALTemplate
    {
        #region Exists
        public readonly static string EXISTS_TEMPLATE = GetTemplateFile("EXISTS_TEMPLATE");
        #endregion

        #region Insert
        #region Insert_SingleId
        public readonly static string INSERT_TEMPLATE1 = GetTemplateFile("INSERT_TEMPLATE1");
        #endregion

        #region Insert_SingleId_WithChildTable
        public readonly static string INSERT_TEMPLATE2 = GetTemplateFile("INSERT_TEMPLATE2");
        #endregion

        #region Insert_MultipleId
        public readonly static string INSERT_TEMPLATE3 = GetTemplateFile("INSERT_TEMPLATE3");
        #endregion

        #region Insert_MultipleId_WithChildTable
        public readonly static string INSERT_TEMPLATE4 = GetTemplateFile("INSERT_TEMPLATE4");
        #endregion
        #endregion

        #region Delete
        #region Delete_Single&MultipleId
        public readonly static string DELETE_TEMPLATE1 = GetTemplateFile("DELETE_TEMPLATE1");
        #endregion

        #region Delete_Single&MultipleId_WithChildTable
        public readonly static string DELETE_TEMPLATE2 = GetTemplateFile("DELETE_TEMPLATE2");
        #endregion

        #region Delete_Child_Single&MultipleId
        public readonly static string DELETE_CHILD_TEMPLATE1 = GetTemplateFile("DELETE_CHILD_TEMPLATE1");
        #endregion
        #endregion

        #region BatchDelete
        #region BatchDelete_SingleId
        public readonly static string BATCHDELETE_TEMPLATE1 = GetTemplateFile("BATCHDELETE_TEMPLATE1");
        #endregion

        #region BatchDelete_SingleId_WithChildTable
        public readonly static string BATCHDELETE_TEMPLATE2 = GetTemplateFile("BATCHDELETE_TEMPLATE2");
        #endregion

        #region BatchDelete_MultipleId
        public readonly static string BATCHDELETE_TEMPLATE3 = GetTemplateFile("BATCHDELETE_TEMPLATE3");
        #endregion

        #region BatchDelete_MultipleId_WithChildTable
        public readonly static string BATCHDELETE_TEMPLATE4 = GetTemplateFile("BATCHDELETE_TEMPLATE4");
        #endregion

        #region BatchDelete_Child_SingleId
        public readonly static string BATCHDELETE_CHILD_TEMPLATE1 = GetTemplateFile("BATCHDELETE_CHILD_TEMPLATE1");
        #endregion

        #region BatchDelete_Child_MultipleId
        public readonly static string BATCHDELETE_CHILD_TEMPLATE2 = GetTemplateFile("BATCHDELETE_CHILD_TEMPLATE2");
        #endregion
        #endregion

        #region Update
        #region Update_Single&MultipleId
        public readonly static string UPDATE_TEMPLATE1 = GetTemplateFile("UPDATE_TEMPLATE1");
        #endregion
        #region Update_Single&MultipleId fields
        public readonly static string UPDATE_TEMPLATE3 = GetTemplateFile("UPDATE_TEMPLATE3");
        #endregion
        #region Update_Single&MultipleId_WithChildTable
        public readonly static string UPDATE_TEMPLATE2 = GetTemplateFile("UPDATE_TEMPLATE2");
        #endregion
        #endregion

        #region GetModel
        #region GetModel_Single&Multiple
        public readonly static string GET_MODEL_TEMPLATE1 = GetTemplateFile("GET_MODEL_TEMPLATE1");
        #endregion

        #region GetModel_Single&Multiple_WithChildTable
        public readonly static string GET_MODEL_TEMPLATE2 = GetTemplateFile("GET_MODEL_TEMPLATE2");
        #endregion
        #endregion

        #region GetList
        #region GetList_Single&Multiple
        public readonly static string GET_LIST_TEMPLATE1 = GetTemplateFile("GET_LIST_TEMPLATE1");
        #endregion

        #region GetList_Single&Multiple_WithChildTable
        public readonly static string GET_LIST_TEMPLATE2 = GetTemplateFile("GET_LIST_TEMPLATE2");
        #endregion
        #endregion

        #region GetRecordCount
        public readonly static string GET_RECORD_COUNT_TEMPLATE = GetTemplateFile("GET_RECORD_COUNT_TEMPLATE");
        #endregion

        #region GetListByPage
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
