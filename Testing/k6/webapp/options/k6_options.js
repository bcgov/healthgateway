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

export let options = common.OptionConfig();

export default function () {

    common.getConfigurations();

    const url = common.BaseSiteUrl;
    const params = {
        headers: {
            'User-Agent': 'Grafana/k6',
            'X-API-KEY': common.SpecialHeaderKey,
            'Access-Control-Request-Headers': 'Content-Type',
            'Access-Control-Request-Method' : 'POST',
            'Connection' : 'Keep-Alive',
            'Accept': 'text/html, application/json',
            'Origin': 'http://localhost',
            'Accept-Encoding': 'gzip, deflate',
            'Accept-Language': 'en-US, en;q=0.5'
        }
    }
    let res = http.options(url, null, params);
    common.checkResponse(res, 204);
    sleep(1);
}

