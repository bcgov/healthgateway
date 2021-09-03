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
import PapaParse from "./papaparse.js";

export let testType = __ENV.TYPE;

export let loadOptions = {
    stages: [
        { duration: "20s", target: 10 }, // below normal load
        { duration: "1m", target: 50 },
        { duration: "1m", target: 400 }, // spike to maximum expected users
        { duration: "3m", target: 400 }, // stay there
        { duration: "1m", target: 250 }, // scale down
        { duration: "3m", target: 10 },
        { duration: "10s", target: 0 }, //
    ],
    thresholds: {
        http_req_duration: ['p(99)<8000'], // 99% of requests must complete below 8s
      },
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
        { duration: "1m", target: 600 }, // spike to super high users
        { duration: "3m", target: 600 }, // stay there
        { duration: "1m", target: 200 }, // scale down
        { duration: "3m", target: 10 },
        { duration: "10s", target: 0 }, //
    ],
};

export let stressOptions = {
    stages: [
        { duration: "2m", target: 50 }, // below normal load
        { duration: "5m", target: 100 },
        { duration: "2m", target: 200 }, // normal load
        { duration: "5m", target: 200 },
        { duration: "2m", target: 400 }, // around the breaking point
        { duration: "4m", target: 400 },
        { duration: "2m", target: 500 }, // beyond the breaking point
        { duration: "5m", target: 550 },
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
    case 'spike':
        options = spikeOptions;
        break;
    case 'soak':
        options = soakOptions;
        break;
    case 'stress':
        options = stressOptions;
        break;
    case 'smoke':
    default:
        testType = 'smoke';
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

    //let cardUrl = "https://hg-test.api.gov.bc.ca/v1/api/VaccineStatus";
    let cardUrl = "https://test.healthgateway.gov.bc.ca/api/immunizationservice/v1/api/VaccineStatus";
    let entryPageUrl  = "https://test.healthgateway.gov.bc.ca/vaccinecard"

    // Loop through all username/password pairs
    //for (var userData of csvData) {
        //console.log(JSON.stringify(userData));

    //}

    // Pick a random username/password pair
    let randomUser = csvData[__VU % csvData.length];
    //let randomUser = csvData[Math.floor(Math.random() * csvData.length)];

    console.log('Random user: ', JSON.stringify(randomUser));

    let res1 = http.get(entryPageUrl);
    check(res1, {"SPA Status 200": (r) => r.status === 200})

    sleep(5);  // the min time we think it would take someone to enter their information

    let params = {
        headers:  { 'User-Agent': 'k6', 
        'phn': randomUser.phn, 
        'dateOfBirth': randomUser.dateOfBirth, 
        'dateOfVaccine': randomUser.dateOfVaccine }
    }
    let res2 = http.get(cardUrl, params);
    check(res2, {"API Status 200": (r) => r.status === 200})

    sleep(1);
}
