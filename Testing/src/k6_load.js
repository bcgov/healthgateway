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

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import * as common from './inc/common.js';

let groupDuration = Trend('batch');

export let options = {
  vu: 300,
  stages: [
    { duration: '3m', target: 70 }, // simulate ramp-up of traffic from 1 users over a few minutes.
    { duration: '5m', target: 70 }, // stay at number of users for several minutes
    { duration: '3m', target: 300 }, // ramp-up to users peak for some minutes (peak hour starts)
    { duration: '2m', target: 300 }, // stay at users for short amount of time (peak hour)
    { duration: '3m', target: 70 }, // ramp-down to lower users over 3 minutes (peak hour ends)
    { duration: '5m', target: 70 }, // continue for additional time
    { duration: '3m', target: 0 }, // ramp-down to 0 users
  ],
  thresholds: {
    'errors': ['rate < 0.05'], // threshold on a custom metric
    'http_req_duration': ['p(90)< 9000'], // 90% of requests must complete this threshold 
    'http_req_duration': ['avg < 5000'], // average of requests must complete within this time
  },
}

function groupWithDurationMetric(name, group_function) {
  let start = new Date();
  group(name, group_function);
  let end = new Date();
  groupDuration.add(end - start, { groupName: name });
}

export default function () {

  let user = common.users[__VU % common.users.length];

  common.authorizeUser(user);

  groupWithDurationMetric('batch', function () {

    let webClientBatchResponses = http.batch(common.webClientRequests(user));
    let timelineBatchResponses = http.batch(common.timelineRequests(user));

    common.checkResponses(webClientBatchResponses);
    common.checkResponses(timelineBatchResponses);

  });

  sleep(1);
}

