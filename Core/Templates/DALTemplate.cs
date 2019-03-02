using Generator.Common;
using System.IO;

namespace Generator.Template
{
    public class DALTemplate
    {
        #region Exists
        public readonly static string EXISTS_TEMPLATE1 = GetTemplateFile("EXISTS_TEMPLATE1");
        // Exists_With_Predicate
        public readonly static string EXISTS_TEMPLATE2 = GetTemplateFile("EXISTS_TEMPLATE2");
        #endregion

        #region Insert
        // Insert_SingleId
        public readonly static string INSERT_TEMPLATE1 = GetTemplateFile("INSERT_TEMPLATE1");
        #endregion

        #region Delete
        // Delete_SingleId
        public readonly static string DELETE_TEMPLATE1 = GetTemplateFile("DELETE_TEMPLATE1");

        // Delete_With_Predicate
        public readonly static string DELETE_TEMPLATE2 = GetTemplateFile("DELETE_TEMPLATE2");

        // BatchDelete_SingleId
        public readonly static string BATCHDELETE_TEMPLATE1 = GetTemplateFile("BATCHDELETE_TEMPLATE1");
        #endregion

        #region Update
        // Update_Single&MultipleId
        public readonly static string UPDATE_TEMPLATE1 = GetTemplateFile("UPDATE_TEMPLATE1");

        // Update_Single&MultipleId_With_Predicate
        public readonly static string UPDATE_TEMPLATE2 = GetTemplateFile("UPDATE_TEMPLATE2");
        #endregion

        #region GetModel
        // GetModel_Single&MultipleId
        public readonly static string GET_MODEL_TEMPLATE1 = GetTemplateFile("GET_MODEL_TEMPLATE1");

        // GetModel_Single&MultipleId_With_Predicate
        public readonly static string GET_MODEL_TEMPLATE2 = GetTemplateFile("GET_MODEL_TEMPLATE2");
        #endregion

        #region GetList
        // GetList_With_Predicate
        public readonly static string GET_LIST_TEMPLATE1 = GetTemplateFile("GET_LIST_TEMPLATE1");
        #endregion

        #region GetCount
        // GetCount_With_Predicate
        public readonly static string GET_COUNT_TEMPLATE = GetTemplateFile("GET_COUNT_TEMPLATE");
        #endregion

        #region GetListByPage
        public readonly static string GET_LIST_BY_PAGE_TEMPLATE = GetTemplateFile("GET_LIST_BY_PAGE_TEMPLATE");
        #endregion

        #region Joined
        public readonly static string INNER_JOIN_TEMPLATE = GetTemplateFile("INNER_JOIN_TEMPLATE");
        public readonly static string LEFT_JOIN_TEMPLATE = GetTemplateFile("LEFT_JOIN_TEMPLATE");
        public readonly static string RIGHT_JOIN_TEMPLATE = GetTemplateFile("RIGHT_JOIN_TEMPLATE");
        #endregion

        #region 辅助方法
        private static string GetTemplateFile(string name)
        {
            return DirHelper.FindFile(Path.Combine("Templates", "DAL"), name + ".txt", true);
        }
        #endregion
    }
}
