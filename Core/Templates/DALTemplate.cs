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
        // Insert_SingleId
        public readonly static string INSERT_TEMPLATE1 = GetTemplateFile("INSERT_TEMPLATE1");

        // Insert_MultipleId
        public readonly static string INSERT_TEMPLATE2 = GetTemplateFile("INSERT_TEMPLATE2");
        #endregion

        #region Delete
        // Delete_Single&MultipleId
        public readonly static string DELETE_TEMPLATE1 = GetTemplateFile("DELETE_TEMPLATE1");

        // BatchDelete_SingleId
        public readonly static string BATCHDELETE_TEMPLATE1 = GetTemplateFile("BATCHDELETE_TEMPLATE1");

        // BatchDelete_MultipleId
        public readonly static string BATCHDELETE_TEMPLATE2 = GetTemplateFile("BATCHDELETE_TEMPLATE2");
        #endregion

        #region Update
        // Update_Single&MultipleId
        public readonly static string UPDATE_TEMPLATE1 = GetTemplateFile("UPDATE_TEMPLATE1");
        #endregion

        #region GetModel
        // GetModel_Single&Multiple
        public readonly static string GET_MODEL_TEMPLATE1 = GetTemplateFile("GET_MODEL_TEMPLATE1");
        #endregion

        #region GetList
        // GetList_Single&Multiple
        public readonly static string GET_LIST_TEMPLATE1 = GetTemplateFile("GET_LIST_TEMPLATE1");
        #endregion

        #region GetCount
        public readonly static string GET_COUNT_TEMPLATE = GetTemplateFile("GET_COUNT_TEMPLATE");
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
