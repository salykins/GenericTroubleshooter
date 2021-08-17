using System;
namespace GenericTroubleshooting.Config
{
    class SolutionConstants
    {
        public const string SolutionName = "GenericTroubleshooter";
        public const string EndpointRoutePath = "sa/";
        public enum DeploymentRegions
        {
            NorthAmer,
            SouthAmer,
            Europe,
            Africa,
            MiddleEast,
            EasternAsia,
            AustraliaNZ
        }
        public class HoneyCombConfig
        {
            public enum DataSets
            {
                debug,
                microservices,
                lambda,
            }
        }
        public class AmazonCognito
        {
            public const string UserPoolId = "us-east-2_srzaPkp0t";// old us-east-2_bR4yPsaLC, new us-east-2_srzaPkp0t
            public const string UserPoolAwsRegion = "us-east-2";
            public static string ApprovedIssuer {
                get
                {
                    return $"https://cognito-idp.{UserPoolAwsRegion}.amazonaws.com/{UserPoolId}";
                }
            }
            public static string ApprovedAudience
            {
                get
                {
                    return SolutionConfigs.Instance.GetConfig(configName: "COGNITO_SHARPEN_APP_CLIENT_ID");
                }
            }
        }
    }
}
