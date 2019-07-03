using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Hl7.Fhir.Rest;
using HealthGateway.Engine.Core;
using Microsoft.AspNetCore.Mvc;
using HealthGateway.Service;
using HealthGateway.Engine.Extensions;

namespace HealthGateway.v1.Controllers
{
    [Route("v1/api/[controller]")]    
    [ApiController]
    public class FhirController : ControllerBase
    {
        //private const string RESOURCE_NAME = "Immunization";

        readonly IFhirService fhirService;

        private readonly string FHIR_VERSION = "SOME VERSION";

        public FhirController(IFhirService fhirService)
        {
            // This will be a (injected) constructor parameter in ASP.vNext.
            this.fhirService = fhirService;
        }

        [HttpGet, Route("{type}/{id}")]
        public ServerFhirResponse Read(string type, string id)
        {
            ConditionalHeaderParameters parameters = new ConditionalHeaderParameters(Request);
            Key key = Key.Create(type, id);
            ServerFhirResponse response = fhirService.Read(key, parameters);

            return response;
        }

        [HttpGet, Route("{type}/{id}/_history/{vid}")]
        public ServerFhirResponse VRead(string type, string id, string vid)
        {
            Key key = Key.Create(type, id, vid);
            return fhirService.VersionRead(key);
        }

        [HttpPut, Route("{type}/{id?}")]
        public ServerFhirResponse Update(string type, Resource resource, string id = null)
        {
            string versionid = FhirHttpUtil.IfMatchVersionId(HttpContext.Request);
            Key key = Key.Create(type, id, versionid);
            if (key.HasResourceId())
            {
                return fhirService.Update(key, resource);
            }
            else
            {
                SearchParams searchparams = SearchParams.FromUriParamList(HttpHeaderUtil.TupledParameters(HttpContext.Request));
                return fhirService.ConditionalUpdate(key, resource, searchparams);
            }
        }

        [HttpPost, Route("{type}")]
        public ServerFhirResponse Create(string type, Resource resource)
        {
            Key key = Key.Create(type, resource?.Id);

            if (HttpHeaderUtil.Exists(Request.Headers, FhirHttpHeaders.IfNoneExist))
            {
                NameValueCollection searchQueryString =
                    HttpUtility.ParseQueryString(
                        Request.Headers.First(h => h.Key == FhirHttpHeaders.IfNoneExist).Value.Single());

                IEnumerable<Tuple<string, string>> searchValues =
                    searchQueryString.Keys.Cast<string>()
                        .Select(k => new Tuple<string, string>(k, searchQueryString[k]));


                return fhirService.ConditionalCreate(key, resource, SearchParams.FromUriParamList(searchValues));
            }

            return fhirService.Create(key, resource);
        }

        [HttpDelete, Route("{type}/{id}")]
        public ServerFhirResponse Delete(string type, string id)

        {
            Key key = Key.Create(type, id);
            ServerFhirResponse response = fhirService.Delete(key);
            return response;
        }

        [HttpDelete, Route("{type}")]
        public ServerFhirResponse ConditionalDelete(string type)
        {
            Key key = Key.Create(type);
            return fhirService.ConditionalDelete(key, HttpHeaderUtil.TupledParameters(Request));
        }

        [HttpGet, Route("{type}/{id}/_history")]
        public ServerFhirResponse History(string type, string id)
        {
            Key key = Key.Create(type, id);
            var parameters = new HistoryParameters(HttpContext.Request);
            return fhirService.History(key, parameters);
        }

        // ============= Validate
        [HttpPost, Route("{type}/{id}/$validate")]
        public ServerFhirResponse Validate(string type, string id, Resource resource)
        {
            //entry.Tags = Request.GetFhirTags();
            Key key = Key.Create(type, id);
            return fhirService.ValidateOperation(key, resource);
        }

        [HttpPost, Route("{type}/$validate")]
        public ServerFhirResponse Validate(string type, Resource resource)
        {
            // DSTU2: tags
            //entry.Tags = Request.GetFhirTags();
            Key key = Key.Create(type);
            return fhirService.ValidateOperation(key, resource);
        }

        // ============= Type Level Interactions

        [HttpGet, Route("{type}")]
        public ServerFhirResponse Search(string type)
        {
            int start = FhirHttpUtil.GetIntParameter(HttpContext.Request, FhirParameter.SNAPSHOT_INDEX) ?? 0;
            var searchparams = HttpHeaderUtil.GetSearchParams(HttpContext.Request);
            return fhirService.Search(type, searchparams, start);
        }

        [HttpPost, HttpGet, Route("{type}/_search")]
        public ServerFhirResponse SearchWithOperator(string type)
        {
            // todo: get tupled parameters from post.
            return Search(type);
        }

        [HttpGet, Route("{type}/_history")]
        public ServerFhirResponse History(string type)
        {
            var parameters = new HistoryParameters(Request);
            return fhirService.History(type, parameters);
        }

        // ============= Whole System Interactions

        [HttpGet, Route("metadata")]
        public ServerFhirResponse Metadata()
        {
            return fhirService.CapabilityStatement(FHIR_VERSION);
        }

        [HttpOptions, Route("")]
        public ServerFhirResponse Options()
        {
            return fhirService.CapabilityStatement(FHIR_VERSION);
        }

        [HttpPost, Route("")]
        public ServerFhirResponse Transaction(Bundle bundle)
        {
            return fhirService.Transaction(bundle);
        }

        [HttpGet, Route("_history")]
        public ServerFhirResponse History()
        {
            var parameters = new HistoryParameters(Request);
            return fhirService.History(parameters);
        }

        [HttpGet, Route("_snapshot")]
        public ServerFhirResponse Snapshot()
        {
            string snapshot = FhirHttpUtil.GetStringParameter(Request, FhirParameter.SNAPSHOT_ID);
            int start = FhirHttpUtil.GetIntParameter(Request, FhirParameter.SNAPSHOT_INDEX) ?? 0;
            return fhirService.GetPage(snapshot, start);
        }

        // Operations

        [HttpPost, Route("${operation}")]
        public ServerFhirResponse ServerOperation(string operation)
        {
            switch (operation.ToLower())
            {
                case "error":
                    throw new Exception("This error is for testing purposes");
                default:
                    /* OperationOutcome outcome = new OperationOutcome();
                    outcome.AddError(string.Format("Unknown operation", args));
                    return new FhirResponse(HttpStatusCode.NotFound, );*/
                    throw new Exception("A different error");
            }
        }

        [HttpPost, Route("{type}/{id}/${operation}")]
        public ServerFhirResponse InstanceOperation(string type, string id, string operation, Parameters parameters)
        {
            Key key = Key.Create(type, id);
            switch (operation.ToLower())
            {
                case "meta":
                    return fhirService.ReadMeta(key);
                case "meta-add":
                    return fhirService.AddMeta(key, parameters);
                case "meta-delete":
                default:
                    //return Respond.WithError(HttpStatusCode.NotFound, "Unknown operation");
                    throw new Exception("An error");
            }
        }

        [HttpPost, HttpGet, Route("{type}/{id}/$everything")]
        public ServerFhirResponse Everything(string type, string id)
        {
            Key key = Key.Create(type, id);
            return fhirService.Everything(key);
        }

        [HttpPost, HttpGet, Route("{type}/$everything")]
        public ServerFhirResponse Everything(string type)
        {
            Key key = Key.Create(type);
            return fhirService.Everything(key);
        }

        [HttpPost, HttpGet, Route("Composition/{id}/$document")]
        public ServerFhirResponse Document(string id)
        {
            Key key = Key.Create("Composition", id);
            return fhirService.Document(key);
        }
    }
}
