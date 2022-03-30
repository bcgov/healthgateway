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

export let options = {
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

export default function () {
    let user = common.users[__VU % common.users.length];

    common.authorizeUser(user);

    common.groupWithDurationMetric("batch", function () {
        let webClientBatchResponses = http.batch(
            common.webClientRequests(user)
        );
        let timelineBatchResponses = http.batch(common.timelineRequests(user));

        common.checkResponses(webClientBatchResponses);
        common.checkResponses(timelineBatchResponses);
    });

    sleep(1);
}
