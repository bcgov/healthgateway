/* 
 * Copyright (c) 2014, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.github.com/furore-fhir/spark/master/LICENSE
 */

namespace HealthGateway.Engine.Core
{
    public static class FhirParameter
    {
        public const string SNAPSHOT_ID = "id";
        public const string SNAPSHOT_INDEX = "start";
        public const string SUMMARY = "_summary";
        public const string COUNT = "_count";
        public const string SINCE = "_since";
        public const string SORT = "_sort";
    }

    public static class FhirHttpHeaders
    {
        public const string IfNoneExist = "If-None-Exist";
    }
}
