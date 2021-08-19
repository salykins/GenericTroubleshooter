using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using GenericTroubleshooting.BusinessLogic;
using GenericTroubleshooting.Config;
using GenericTroubleshooting.HttpFunctions.Classes;
using System.Security.Claims;
using GenericTroubleshooting.Authorization;
using System.Collections.Generic;
using GenericTroubleshooting.Logging;
using System.Diagnostics;

namespace GenericTroubleshooting.HttpFunctions.v1
{
    public static class Troubleshooting
    {
        const string functionName = "Troubleshooting";
        [FunctionName(functionName)]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            #region set up logging
            //set config exectution context so we can use SolutionConfigs to get configs
            SolutionConfigs.Instance.ExecContext = context;

            //create logging dictionary
            var loggingAttributeDictionary = new Dictionary<string, object>();

            // create request reference Id
            string reqRefId = Guid.NewGuid().ToString();
            loggingAttributeDictionary.Add(key: "reqRefId", value: reqRefId);
            TroubleshootingRes resBody = new TroubleshootingRes();
            resBody.ReqRefId = reqRefId;
            #endregion

            #region verify request
            //verify an authorization header was given in the request
            var authorizationHeader = req.Headers["Authorization"];
            req.Headers.Remove("Authorization");
            loggingAttributeDictionary.Add(key: "request.headers", value: JsonConvert.SerializeObject(req.Headers));

            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return LogEndpointData(loggingAttributeDictionary: loggingAttributeDictionary,
                    res: new UnauthorizedObjectResult(value: new BaseHttpResponse() { ReqRefId = resBody.ReqRefId })
                    , stopwatch: stopwatch);
            }

            //verify access token is good
            ClaimsPrincipal principal;
            if ((principal = await AuthorizationValidator.ValidateTokenAsync(value: authorizationHeader, loggingAttributeDictionary: loggingAttributeDictionary)) == null)
            {
                return LogEndpointData(loggingAttributeDictionary: loggingAttributeDictionary,
                    res: new UnauthorizedObjectResult(value: new BaseHttpResponse() { ReqRefId = resBody.ReqRefId })
                    , stopwatch: stopwatch);

            }

            //parse request
            string requestBodyString = await new StreamReader(req.Body).ReadToEndAsync();
            loggingAttributeDictionary.Add(key: "request.body", value: requestBodyString);
            var requestBody = JsonConvert.DeserializeObject<TroubleshootingReq>(requestBodyString);

            resBody.tenantId = requestBody.tenantId;
            if (int.TryParse(requestBody.tenantId, out var tenantIdValid) == false)
            {
                return LogEndpointData(loggingAttributeDictionary: loggingAttributeDictionary,
                    res: new BadRequestObjectResult(error: new BaseHttpResponse() { ReqRefId = resBody.ReqRefId })
                    , stopwatch: stopwatch);

            }
            #endregion

            //perform business logic
            resBody.CustomObjectsTableForSfStatusesName = CustomObjectsBusinessLogic.GetQuestionOne(tenantId: tenantIdValid);

            //send log and return result
            return LogEndpointData(loggingAttributeDictionary: loggingAttributeDictionary,
                res: new OkObjectResult(value: resBody), stopwatch: stopwatch);
        }

        public static ObjectResult LogEndpointData(Dictionary<string, object> loggingAttributeDictionary, ObjectResult res,
            Stopwatch stopwatch)
        {
            stopwatch.Stop();
            TimeSpan ts = stopwatch.Elapsed;
            var elapsedTime = string.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
            loggingAttributeDictionary.Add(key: "duration_ms", value: ts.TotalMilliseconds);
            loggingAttributeDictionary.Add(key: "duration_formatted", value: elapsedTime);
            loggingAttributeDictionary.Add(key: "response.statusCode", value: res.StatusCode.ToString());
            if (res.Value != null)
            {
                loggingAttributeDictionary.Add(key: "response.body", value: JsonConvert.SerializeObject(res.Value));
            }
            Logger.Instance.HoneyComb.SendNow(loggingAttributeDictionary);
            return res;
        }
    }

    public class TroubleshootingReq : BaseHttpRequest
    {
        public string tenantId { get; set; }
    }

    public class TroubleshootingRes : BaseHttpResponse
    {
        private string _customObjectsTableForSfStatusesName;

        public string tenantId { get; set; }
        public bool CustomObjectsTableForSfStatusesExists { get; set; }
        public string CustomObjectsTableForSfStatusesName
        {
            get
            {
                return _customObjectsTableForSfStatusesName;
            }
            set
            {
                _customObjectsTableForSfStatusesName = value;
                if (string.IsNullOrWhiteSpace(value) == false)
                {
                    CustomObjectsTableForSfStatusesExists = true;
                }
            }
        }
    }

}
