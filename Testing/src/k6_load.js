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

export let failed = new Rate('failed_requests');
let groupDuration = Trend('groupDuration');


export let options = {
  stages: [
    { duration: '5m', target: 60 }, // simulate ramp-up of traffic from 1 to 60 users over 5 minutes.
    { duration: '10m', target: 60 }, // stay at 60 users for 10 minutes
    { duration: '3m', target: 300 }, // ramp-up to users peak for 3 minutes (peak hour starts)
    { duration: '2m', target: 300 }, // stay at users for short amount of time (peak hour)
    { duration: '3m', target: 60 }, // ramp-down to 60 users over 3 minutes (peak hour ends)
    { duration: '5m', target: 60 }, // continue at 60 for additional time
    { duration: '3m', target: 0 }, // ramp-down to 0 users
  ],
  thresholds: {
    'failed_requests': ['rate < 0.05'], // threshold on a custom metric
    'http_req_duration': ['p(90)< 9000'], // 90% of requests must complete below 9 seconds
    'http_req_duration': ['avg < 4001'], // average of requests must complete below 4 seconds

    'groupDuration{groupName:userRequestBatch}': ['avg < 15000'], // average batch of all calls to backend APIs

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

  if (user.hdid == null) {
    let loginRes = common.authenticateUser(user);
    check(loginRes, {
      'Authenticated successfully': loginRes == 200
    }) || errorRate.add(1);
  }
  if (user.expires < (Date.now() - 3000)) // milliseconds
  {
    common.refreshUser(user);
  }

  var params = {
    headers: {
      "Content-Type": "application/json",
      Authorization: "Bearer " + user.token,
    },
  };

  let requests = {
    'comment': {
      method: 'GET',
      url: common.CommentUrl + "/" + user.hdid,
      params: params
    },
    'note': {
      method: 'GET',
      url: common.NoteUrl + "/" + user.hdid,
      params: params
    },
    'patient': {
      method: 'GET',
      url: common.PatientServiceUrl + "/" + user.hdid,
      params: params
    },
    'meds': {
      method: 'GET',
      url: common.MedicationServiceUrl + "/" + user.hdid,
      params: params
    },

    'labs': {
      method: 'GET',
      url: common.LaboratoryServiceUrl + "?hdid=" + user.hdid,
      params: params
    }
  };

  groupWithDurationMetric('userRequestBatch', function () {
    let responses = http.batch(requests);

    check(responses['comment'], {
      "Comment Response Code is 200": (r) => r.status == 200,
      "Comment Response Code is not 400": (r) => r.status != 400,
      "Comment Response Code is not 401": (r) => r.status != 401,
      "Comment Response Code is not 403": (r) => r.status != 403,
      "Comment Response Code is not 408": (r) => r.status != 408,
      "Comment Response Code is not 429": (r) => r.status != 429,
      "Comment Response Code is not 500": (r) => r.status != 500,
      "Comment Response Code is not 504": (r) => r.status != 504,
    }) || failed.add(true);

    check(responses['note'], {
      "Note Response Code is 200": (r) => r.status == 200,
      "Note Response Code is not 400": (r) => r.status != 400,
      "Note Response Code is not 401": (r) => r.status != 401,
      "Note Response Code is not 403": (r) => r.status != 403,
      "Note Response Code is not 408": (r) => r.status != 408,
      "Note Response Code is not 429": (r) => r.status != 429,
      "Note Response Code is not 500": (r) => r.status != 500,
      "Note Response Code is not 504": (r) => r.status != 504,
    }) || failed.add(true);

    check(responses['patient'], {
      "PatientService Response Code is 200": (r) => r.status == 200,
      "PatientService Response Code is not 400": (r) => r.status != 400,
      "PatientService Response Code is not 401": (r) => r.status != 401,
      "PatientService Response Code is not 403": (r) => r.status != 403,
      "PatientService Response Code is not 408": (r) => r.status != 408,
      "PatientService Response Code is not 429": (r) => r.status != 429,
      "PatientService Response Code is not 500": (r) => r.status != 500,
      "PatientService Response Code is not 504": (r) => r.status != 504,
    }) || failed.add(true);

    check(responses['meds'], {
      "MedicationService Response Code is 200": (r) => r.status == 200,
      "MedicationService Response Code is not 400": (r) => r.status != 400,
      "MedicationService Response Code is not 401": (r) => r.status != 401,
      "MedicationService Response Code is not 403": (r) => r.status != 403,
      "MedicationService Response Code is not 408": (r) => r.status != 408,
      "MedicationService Response Code is not 429": (r) => r.status != 429,
      "MedicationService Response Code is not 500": (r) => r.status != 500,
      "MedicationService Response Code is not 504": (r) => r.status != 504,
    }) || failed.add(true);

    check(responses['labs'], {
      "LaboratoryService Response Code is 200": (r) => r.status == 200,
      "LaboratoryService Response Code is not 400": (r) => r.status != 400,
      "LaboratoryService Response Code is not 401": (r) => r.status != 401,
      "LaboratoryService Response Code is not 403": (r) => r.status != 403,
      "LaboratoryService Response Code is not 408": (r) => r.status != 408,
      "LaboratoryService Response Code is not 429": (r) => r.status != 429,
      "LaboratoryService Response Code is not 500": (r) => r.status != 500,
      "LaboratoryService Response Code is not 504": (r) => r.status != 504,
    }) || failed.add(true);
  });

  sleep(1);
}

