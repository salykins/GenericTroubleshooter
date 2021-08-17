using System;
using GenericTroubleshooting.DataAccess;

namespace GenericTroubleshooting.BusinessLogic
{
    public class CustomObjectsBusinessLogic
    {
        private static ICustomObjectsDataAccess _customObjectDataAccess
        {
            get
            {
                return DataAccessFactory.GetCustomObjectsDataAccessObj();
            }
        }

        public static string GetQuestionOne(int tenantId)
        {
            return _customObjectDataAccess.GetAnswers(tenantId: tenantId)?.ObjName;
        }

        public static string GetQuestionTwo(int tenantId)
        {
            return _customObjectDataAccess.GetAnswers(tenantId: tenantId)?.ObjName;
        }
    }
}
