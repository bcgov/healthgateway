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
import { check, sleep } from 'k6';
import { SharedArray } from 'k6/data';
import * as common from "../../inc/common.js";
import PapaParse from "../../inc/papaparse.js";


export let entryPageUrl  = common.BaseSiteUrl + "/vaccinecard";

console.log(common.BaseSiteUrl);

// not using SharedArray here will mean that the code in the function call (that is what loads and
// parses the csv) will be executed per each VU which also means that there will be a complete copy
// per each VU
const csvData = new SharedArray("another data name", function() {
    // Load CSV file and parse it using Papa Parse
    return PapaParse.parse(open('./data.csv'), { header: true }).data;
});

export default function () {

    let randomUser = csvData[__VU % csvData.length];

    let spaBatchResponses = http.batch(
        common.spaAssetRequests(),
    );
    common.checkResponses(spaBatchResponses);

    let response = http.get(entryPageUrl, { headers: common.HttpHeaders });

    success = check(response[0], {
        'VaccineCard Page Title Correct': (r) => (r.status === 200)
            && r.html
            && r.html('title').text().includes('Health Gateway'),
         'Reached VaccineCard Page; Not Queue-IT': (r) => r.body.search('queue-it.net') === -1,

    });

    if (success)
    {
        sleep(5);  // the min time we think it would take someone to enter their information

        let params = {
            headers:  { 'User-Agent': 'k6', 
            'X-API-KEY' : specialHeaderKey,
            'phn': randomUser.phn, 
            'dateOfBirth': randomUser.dateOfBirth, 
            'dateOfVaccine': randomUser.dateOfVaccine }
        }
        let res2 = http.get(cardUrl, params);

        check(res2, {"API Status 200": (r) => r.status === 200})
        check(res2, {
            'Reached API Endpoint; Not Queue-IT': (r) => r.body.search('queue-it.net') === -1,
            'API Response Content-Type is JSON': (r) => r.headers['Content-Type'].search('application/json') >= 0, 
            });
    }
    else {
        console.log("Skipping API Call; in Waiting Room!")
    }
    sleep(1);
}
