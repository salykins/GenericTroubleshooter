using System;
using Dapper;
using MySql.Data.MySqlClient;
using GenericTroubleshooting.Config;
using GenericTroubleshooting.DataClasses;

namespace GenericTroubleshooting.DataAccess
{
    public interface ICustomObjectsDataAccess
    {
        CustomObject GetAnswers(int tenantId);
    }

    public class CustomObjectsDataAccess : ICustomObjectsDataAccess
    {
        private static CustomObjectsDataAccess _instance;
        public static CustomObjectsDataAccess Instance
        {
            get
            {
                if(_instance != null)
                {
                    return _instance;
                }
                else
                {
                    return _instance = new CustomObjectsDataAccess();
                }
            }
        }
        private CustomObjectsDataAccess()
        {
        }

        public CustomObject GetAnswers(int tenantId)
        {
            CustomObject customObj = null;
            using (var connection = new MySqlConnection(SolutionConfigs.Instance.GetConfig(configName: "SQL_CONNECTION_STRING_FV_FvCustomObjects")))
            {
                customObj = connection.QueryFirst<CustomObject>(@"SELECT objId as 'ObjId'
                , `tenantID` as 'tenantId'
                , objType as 'ObjType'
                , objName as 'ObjName'
                , objNameAPI as 'ObjNameAPI'
                , objDesc as 'ObjDesc'
                , active as 'Active'
                FROM db.questionsTable
                WHERE `tenantID` = @tenantId AND objNameAPI = 'FathomSASFCampaignsStatus__api'
                LIMIT 300;", param: new { tenantId = tenantId });
            }
            return customObj;
        }
    }

    public class DataAccessFactory
    {
        public static ICustomObjectsDataAccess GetCustomObjectsDataAccessObj()
        {
            return CustomObjectsDataAccess.Instance;
        }
    }
}
