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
  vus: 5,
  iterations: 5,
}

export default function () {

  let user = common.users[common.getRandomInteger(0, common.users.length - 1)];

  if (((__ITER == 0) & user.hdid == null) || (user.hdid == null)) {
    let loginRes = common.authenticateUser(user);
    check(loginRes, {
      'Authenticated successfully': loginRes == 200
    }) || errorRate.add(1);
  }

  common.refreshTokenIfNeeded(user);

  var params = {
    headers: {
      "Content-Type": "application/json",
      Authorization: "Bearer " + user.token,
    },
  };

  let requests = {
    'beta': {
      method: 'GET',
      url: common.BetaRequestUrl + "/" + user.hdid,
      params: params
    },
    'comment': {
      method: 'GET',
      url: common.CommentUrl + "/" + user.hdid,
      params: params
    },
    'communication': {
      method: 'GET',
      url: common.CommunicationUrl + "/" + user.hdid,
      params: params
    },
    'conf': {
      method: 'GET',
      url: common.ConfigurationUrl + "/" + user.hdid,
      params: params
    },
    'note': {
      method: 'GET',
      url: common.NoteUrl + "/" + user.hdid,
      params: params
    },
    'profile': {
      method: 'GET',
      url: common.UserProfileUrl + "/" + user.hdid,
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

  let responses = http.batch(requests);

  check(responses['beta'], {
    "Beta Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  check(responses['comment'], {
    "Comment Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  check(responses['communication'], {
    "Communication Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  check(responses['conf'], {
    "Configuration Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  check(responses['note'], {
    "Note Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  check(responses['profile'], {
    "UserProfile Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  check(responses['patient'], {
    "PatientService Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  check(responses['meds'], {
    "MedicationService Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  check(responses['labs'], {
    "LaboratoryService Response Code is 200": (r) => r.status == 200,
  }) || errorRate.add(1);

  sleep(1);
}