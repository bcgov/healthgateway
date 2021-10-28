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
import { check, sleep, group } from 'k6';
import { SharedArray } from 'k6/data';
import PapaParse from "./papaparse.js";

export let testType = __ENV.HG_TYPE  ? __ENV.HG_TYPE : "smoke";
export let environment = __ENV.HG_ENV  ? __ENV.HG_ENV : "test"; // default to test environment
export let specialHeaderKey = __ENV.HG_KEY ? __ENV.HG_KEY : "nokey"; // special key 

export let baseSiteUrl = "https://" + environment + ".healthgateway.gov.bc.ca";
export let cardUrl = baseSiteUrl + "/api/immunizationservice/v1/api/VaccineStatus";
export let pdfUrl = cardUrl + "/pdf";
export let entryPageUrl  = baseSiteUrl + "/vaccinecard";
export let vendorChunkJsUrl = baseSiteUrl + "/js/chunk-vendors.c61f122d.js";
export let siteChunkJsUrl = baseSiteUrl + "/js/app.8136e1c8.js";
export let cssUrl = baseSiteUrl + "/css/app.c90e9393.css";
export let cssVendorsUrl = baseSiteUrl + "/css/chunk-vendors.21f4bba7.css";

console.log(baseSiteUrl);

export let rpsRampingOptions = {
    discardResponseBodies: false,
    scenarios: {
        ramping: {
            executor: 'ramping-arrival-rate',
            startRate: 60,
            timeUnit:'1m',
            preAllocatedVUs: 100,
            maxVUs: 200,
            stages: [
                { target: 120, duration: '30s'},
                { target: 240, duration: '1m'},
                { target: 480, duration: '2m'},
                { target: 960, duration: '30s'},
                { target: 1100, duration: '3m'},
                { target: 960, duration: '1m'},
                { target: 240, duration: '1m'},
                { target: 0, duration: '30s'}
            ],
        },
    },
};

export let onceOptions = {
    discardResponseBodies: false,
    scenarios: {
        constant: {
            executor: 'constant-arrival-rate',
            rate: 1,
            timeUnit:'1s',
            duration: '10s',
            preAllocatedVUs: 10,
            maxVUs: 100
        },
    },
};

export let loadOptions = {
    stages: [
        { duration: "30s", target: 50 },
        { duration: "30s", target: 75 },
        { duration: "1m", target: 150 },
        { duration: "2m", target: 200 },
        { duration: "2m", target: 300 },
        { duration: "3m", target: 400 }, 
        { duration: "30s", target: 100 },
        { duration: "20s", target: 50 },
        { duration: "10s", target: 1 },
    ],
    thresholds: {
        http_req_duration: ['p(99)<8000'], // 99% of requests must complete below 8s
      },
};

export let load2Options = {
    stages: [
        { duration: "30m", target: 10 }, // well below normal load
    ],
};

export let soakOptions = {
    stages: [
        { duration: "1m", target: 10 }, // below normal load
        { duration: "2m", target: 250 },
        { duration: "3h56m", target: 250 }, // stay at high users for hours 'soaking' the system
        { duration: "2m", target: 0 }, // drop back down
    ],
};

export let spikeOptions = {
    stages: [
        { duration: "20s", target: 10 }, // below normal load
        { duration: "1m", target: 10 },
        { duration: "1m", target: 400 }, // spike to super high users
        { duration: "3m", target: 400 }, // stay there
        { duration: "1m", target: 100 }, // scale down
        { duration: "3m", target: 10 },
        { duration: "10s", target: 0 }, //
    ],
};

export let stressOptions = {
    stages: [
        { duration: "2m", target: 50 }, // below normal load
        { duration: "5m", target: 100 },
        { duration: "2m", target: 150 }, // normal load
        { duration: "3m", target: 300 },
        { duration: "5m", target: 400 }, // around the breaking point
        { duration: "2m", target: 500 }, // beyond the breaking point
        { duration: "1m", target: 560 },
        { duration: "3m", target: 600 }, // limit
        { duration: "5m", target: 0 }, // scale down. Recovery stage.
    ],
};

export let options = {
    vus: 3,
    iterations: 5
}

switch (testType)
{
    case 'load':
        options = loadOptions;
        break;
    case 'load2':
        options = load2Options;
        break;
    case 'spike':
        options = spikeOptions;
        break;
    case 'soak':
        options = soakOptions;
        break;
    case 'stress':
        options = stressOptions;
        break;
    case 'rps':
        options = rpsRampingOptions;
        break;
    case 'once':
        options = onceOptions;
        break;
    case 'smoke':
    default:
        break;
}

console.log("Test: " + testType);

export let currentUser = 0;

// not using SharedArray here will mean that the code in the function call (that is what loads and
// parses the csv) will be executed per each VU which also means that there will be a complete copy
// per each VU
const csvData = new SharedArray("another data name", function() {
    // Load CSV file and parse it using Papa Parse
    return PapaParse.parse(open('./data.csv'), { header: true }).data;
});

export default function () {

    let randomUser = csvData[__VU % csvData.length];

    //console.log('Random user: ', JSON.stringify(randomUser));

    let headers = {
        'User-Agent': 'k6', 
        'X-API-KEY' : specialHeaderKey,
    }
    let success = false;

    group('get SPA assets', function() {

    let responses = http.batch([
        ['GET', entryPageUrl, null, { headers: headers, tags: { ctype: 'html' } }],
        ['GET', cssUrl, null, { headers: headers, tags: { ctype: 'css' } }],
        ['GET', cssVendorsUrl, null, { headers: headers, tags: { ctype: 'js' } }],
        ['GET', vendorChunkJsUrl, null, { headers: headers, tags: { ctype: 'js' } }],
        ['GET', siteChunkJsUrl, null, { headers: headers, tags: { ctype: 'js' } }],
      ] );
      check(responses[0], {
        'Webpage status 200': (res) => res.status === 200,
      });

    success = check(responses[0], {
        'Reached VaccineCard Page; Not Queue-IT': (r) => r.body.search('queue-it.net') === -1,
        'VaccineCard Page Title Correct': (r) => r.html('title').text() == 'Health Gateway',
      });
    });

    group('get vaccinecard with QR', function() {

    if (success)
    {
        sleep(15);  // the min time we think it would take someone to enter their information

        let params = {
            headers:  { 'User-Agent': 'k6', 
            'X-API-KEY' : specialHeaderKey,
            'phn': randomUser.phn, 
            'dateOfBirth': randomUser.dateOfBirth, 
            'dateOfVaccine': randomUser.dateOfVaccine }
        }
        let res2 = http.get(cardUrl, params);

        check(res2, {"VaccineStatus API Status 200": (r) => r.status === 200});
        check(res2, {
            'Reached API Endpoint; Not Queue-IT': (r) => r.body.search('queue-it.net') === -1,
            'API Response Content-Type is JSON': (r) => r.headers['Content-Type'].search('application/json') >= 0, 
            });
    }
    else {
        console.log("Skipping API Call; in Waiting Room!")
    }
    sleep(1);
    });

    group('Get Fed/Prov Combined PDF', function() {
        let requestParams = {
            headers:  { 'User-Agent': 'k6', 
            'X-API-KEY' : specialHeaderKey,
            'phn': randomUser.phn, 
            'dateOfBirth': randomUser.dateOfBirth, 
            'dateOfVaccine': randomUser.dateOfVaccine,
            'proofTemplate': 'Combined' } 
        }
        sleep(2); // the think-time before user chooses to download the pdfUrl.
        let res3 = http.get(pdfUrl, requestParams);

        check(res3, {"VaccineStatus/pdf API Status 200": (r) => r.status === 200});
        check(res3, {
            'Reached VaccineStatus/pdf API Endpoint; Not Queue-IT': (r) => r.body.search('queue-it.net') === -1,
            'Response Content-Type is application/json': (r) => r.headers['Content-Type'].search('application/json') >= 0,
            'Response contains the Base64 PDF': (r) => r.body.includes("resourcePayload") && r.body.includes("VaccineProof.pdf") && r.body.includes("data")
        });

    });

}
