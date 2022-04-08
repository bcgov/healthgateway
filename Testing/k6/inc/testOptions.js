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


export let loadOptions = {
    vu: maxVus,
    stages: [
        { duration: "2m", target: rampVus }, // simulate ramp-up of traffic from 1 users over a few minutes.
        { duration: "3m", target: rampVus }, // stay at number of users for several minutes
        { duration: "3m", target: maxVus }, // ramp-up to users peak for some minutes (peak hour starts)
        { duration: "3m", target: maxVus }, // stay at users for short amount of time (peak hour)
        { duration: "2m", target: rampVus }, // ramp-down to lower users over 3 minutes (peak hour ends)
        { duration: "3m", target: rampVus }, // continue for additional time
        { duration: "2m", target: 0 }, // ramp-down to 0 users
    ],
    thresholds: {
        errors: ["rate < 0.05"], // threshold on a custom metric
        http_req_duration: ["p(90)< 9000"], // 90% of requests must complete this threshold
        http_req_duration: ["avg < 5000"], // average of requests must complete within this time
    },
};
export let smokeOptions = {
    vus: 2,
    iterations: 5,
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
        { duration: "5m", target: 400 }, // stay there
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
        { duration: "5m", target: 0 }, // scale down. Recovery stage.
    ],
};
