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
import { check, sleep, group } from 'k6';
import { Rate } from 'k6/metrics';
import * as common from './inc/common.js';

export let errorRate = new Rate('errors');

export let options = {
  stages: [
    { duration: '20s', target: 10 }, // below normal load
    { duration: '1m', target: 10 },
    { duration: '1m', target: 400 }, // spike to super high users
    { duration: '5m', target: 400 }, // stay there 
    { duration: '1m', target: 200 }, // scale down
    { duration: '3m', target: 10 },
    { duration: '10s', target: 0 }, //
  ],
};

export default function () {

  let user = common.users[__VU % common.users.length];

  if ((__ITER == 0) && (user.hdid == null)) {
    let loginRes = common.authenticateUser(user);
    check(loginRes, {
      'Authenticated successfully': loginRes == 200
    }) || errorRate.add(1);
  }

  common.refreshTokenIfNeeded(user); // only refreshes if needed.

  let params = {
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

  sleep(1);
}

