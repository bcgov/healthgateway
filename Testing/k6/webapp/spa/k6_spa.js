//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------

import http from "k6/http";
import { sleep } from "k6";
import * as common from "../../inc/common.js";

export let vendorChunkJsUrl = common.baseSiteUrl + "/js/chunk-vendors.c61f122d.js";
export let siteChunkJsUrl = common.baseSiteUrl  + "/js/app.8136e1c8.js";
export let cssUrl = common.baseSiteUrl  + "/css/app.c90e9393.css";
export let cssVendorsUrl = common.baseSiteUrl  + "/css/chunk-vendors.21f4bba7.css";

export let options = common.OptionConfig();

export default function () {

    let user = common.users[__VU % common.users.length];

    common.getConfigurations();
    common.getOpenIdConfigurations();
    common.authorizeUser(user);
    
    common.groupWithDurationMetric("spaBatch", function () {
        let spaBatchResponses = http.batch(
            common.spaAssetRequests(),
        );
        common.checkResponse(spaBatchResponses);
    });

    sleep(1);
}
