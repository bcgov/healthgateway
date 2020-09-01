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
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';
import * as common from './inc/common.js';

export let errorRate = new Rate('errors');

export let options = {
  stages: [
    { duration: '5m', target: 60 }, // simulate ramp-up of traffic from 1 to 60 users over 5 minutes.
    { duration: '10m', target: 60 }, // stay at 60 users for 10 minutes
    { duration: '3m', target: 300 }, // ramp-up to users peak for 3 minutes (peak hour starts)
    { duration: '2m', target: 300 }, // stay at users for short amount of time (peak hour)
    { duration: '3m', target: 50 }, // ramp-down to 60 users over 3 minutes (peak hour ends)
    { duration: '5m', target: 50 }, // continue at 60 for additional 10 minutes
    { duration: '3m', target: 0 }, // ramp-down to 0 users
  ],
  thresholds: {
    http_req_duration: ['p(95)<4500'], // 95% of requests must complete below 4.5s
  },
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

  let responses = http.batch(requests);

  check(responses['patient'], {
    "PatientService Response Code is 200": (r) => r.status == 200,
    "PatientService Response Code is not 504": (r) => r.status != 504,
    "PatientService Response Code is not 500": (r) => r.status != 500,
    "PatientService Response Code is not 403": (r) => r.status != 403,
  }) || errorRate.add(1);

  check(responses['meds'], {
    "MedicationService Response Code is 200": (r) => r.status == 200,
    "MedicationService Response Code is not 504": (r) => r.status != 504,
    "MedicationService Response Code is not 500": (r) => r.status != 500,
    "MedicationService Response Code is not 403": (r) => r.status != 403,
  }) || errorRate.add(1);

  check(responses['labs'], {
    "LaboratoryService Response Code is 200": (r) => r.status == 200,
    "LaboratoryService Response Code is not 504": (r) => r.status != 504,
    "LaboratoryService Response Code is not 500": (r) => r.status != 500,
    "LaboratoryService Response Code is not 403": (r) => r.status != 403,
  }) || errorRate.add(1);

  sleep(common.getRandom(1, 5));
}

