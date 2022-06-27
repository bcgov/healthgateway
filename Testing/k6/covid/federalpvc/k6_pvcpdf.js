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
import * as common from "../../inc/common.js";
import PapaParse from "../../inc/papaparse.js";
import { check, sleep, group } from 'k6';
import { SharedArray } from 'k6/data';


export let cardUrl = common.ServiceEndpoints.Immunization + "PublicVaccineStatus";
export let pdfUrl = cardUrl + "/pdf";
export let entryPageUrl = common.BaseSiteUrl + "/vaccinecard";

console.log("Test: " + testType);

// not using SharedArray here will mean that the code in the function call (that is what loads and
// parses the csv) will be executed per each VU which also means that there will be a complete copy
// per each VU
const csvData = new SharedArray("another data name", function () {
    // Load CSV file and parse it using Papa Parse
    return PapaParse.parse(open('./data.csv'), { header: true }).data;
});

export default function () {

    let randomUser = csvData[__VU % csvData.length];

    let success = false;
    http.setResponseCallback(http.expectedStatuses(200));

    group('Get SPA Web Page Assets Async', function () {

        let spaBatchResponses = http.batch(
            common.spaAssetRequests(),
        );
        common.checkResponses(spaBatchResponses);

        let response = http.get(entryPageUrl, { headers: common.HttpHeaders });

        success = check(response[0], {
            'VaccineCard Page Title Correct': (r) => (r.status === 200)
                && r.html
                && r.html('title').text().includes('Health Gateway')
        });

        sleep(1);
    });

    group('Get VaccineCard with QR', function () {

        if (success === true) {
            sleep(3);  // min time we think it would take someone to enter their information

            let params = {
                headers: {
                    'User-Agent': 'k6',
                    'X-API-KEY': specialHeaderKey,
                    'phn': randomUser.phn,
                    'dateOfBirth': randomUser.dateOfBirth,
                    'dateOfVaccine': randomUser.dateOfVaccine
                }
            }

            let res2 = http.get(cardUrl, params);

            common.checkResponse(res2);
            check(res2, {
                'Reached API Endpoint; Not Queue-IT': (r) => (r.status === 200)
                    && r.body
                    && !r.body.includes('queue-it.net'),
                'API Response Content-Type is JSON': (r) => (r.status === 200)
                    && (r.headers['Content-Type'].search('application/json') >= 0),
            });
        }
        else {
            console.log("Skipping API Call; in Waiting Room!")
        }
        sleep(1);
    });

    group('Get Federal Proof of Vaccination PDF', function () {
        let requestParams = {
            headers: {
                'User-Agent': 'k6',
                'X-API-KEY': specialHeaderKey,
                'phn': randomUser.phn,
                'dateOfBirth': randomUser.dateOfBirth,
                'dateOfVaccine': randomUser.dateOfVaccine,
                'proofTemplate': 'Federal'
            }
        }
        sleep(1); // the think-time before user chooses to download the pdfUrl.
        let res3 = http.get(pdfUrl, requestParams);

        //console.log(res3.body.toString());

        common.checkResponse(res3);
        check(res3, {
            'Reached VaccineStatus/pdf API Endpoint; Not Queue-IT': (r) => (r.status === 200)
                && r.body
                && !r.body.includes('queue-it.net'),
            'Response Content-Type is application/json': (r) => (r.status === 200)
                && r.headers
                && (r.headers['Content-Type'].search('application/json') >= 0),
            'Response contains a Federal PDF': (r) => (r.status === 200)
                && r.body
                && r.body.includes("\"resourcePayload\":")
                && r.body.includes("\"mediaType\": \"application/pdf\"")
                && r.body.includes("\"encoding\": \"base64\"")
                && (r.json()['resultStatus'] === 1),
            'Response does not contain actionCode=REFRESH': (r) => (r.status === 200)
                && r.body
                && !(r.body.includes("actionCode")
                    && r.body.includes("REFRESH"))

        });

    });
}
