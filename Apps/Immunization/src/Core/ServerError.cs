using System.Collections.Generic;
using System.Net;
using Hl7.Fhir.Model;

namespace HealthGateway.Engine.Core
{
    public static class Error
    {
        public static ServerException Create(HttpStatusCode code, string message, params object[] values)
        {
            return new ServerException(code, message, values);
        }

        public static ServerException BadRequest(string message, params object[] values)
        {
            return new ServerException(HttpStatusCode.BadRequest, message, values);
        }

        public static ServerException NotFound(string message, params object[] values)
        {
            return new ServerException(HttpStatusCode.NotFound, message, values);
        }

        public static ServerException NotFound(IKey key)
        {
            if (key.VersionId == null)
            {
                return NotFound("No {0} resource with id {1} was found.", key.TypeName, key.ResourceId);
            }
            else
            {
                return NotFound("There is no {0} resource with id {1}, or there is no version {2}", key.TypeName, key.ResourceId, key.VersionId);
            }
        }

        public static ServerException NotAllowed(string message)
        {
            return new ServerException(HttpStatusCode.Forbidden, message);
        }

        public static ServerException Internal(string message, params object[] values)
        {
            return new ServerException(HttpStatusCode.InternalServerError, message, values);
        }

        public static ServerException NotSupported(string message, params object[] values)
        {
            return new ServerException(HttpStatusCode.NotImplemented, message, values);
        }

        private static OperationOutcome.IssueComponent CreateValidationResult(string details, IEnumerable<string> location)
        {
            return new OperationOutcome.IssueComponent()
            {
                Severity = OperationOutcome.IssueSeverity.Error,
                Code = OperationOutcome.IssueType.Invalid,
                Details = new CodeableConcept("http://hl7.org/fhir/issue-type", "invalid"),
                Diagnostics = details,
                Location = location
            };
        }
    }
}
