using System;
using Microsoft.AspNetCore.Mvc;

namespace GenericTroubleshooting.HttpFunctions.Classes
{
    public class UnauthorizedObjectResult : ObjectResult
    {
        public UnauthorizedObjectResult(object value) : base(value: value)
        {
            Value = value;
            StatusCode = 401;
        }

    }
}
