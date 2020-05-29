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
import { b64decode } from 'k6/encoding';
import { Rate } from 'k6/metrics';

export let errorRate = new Rate('errors');

let environment = (__ENV.HG_ENV != undefined) ? __ENV.HG_ENV : 'dev';

let TokenEndpointUrl = "https://sso-" + environment + ".pathfinder.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token";
let MedicationServiceUrl = "https://" + environment + ".healthgateway.gov.bc.ca/api/medicationservice/v1/api/MedicationStatement";
let LaboratoryServiceUrl = "https://" + environment + ".healthgateway.gov.bc.ca/api/laboratoryservice/v1/api/Laboratory";

let passwd = __ENV.HG_PASSWORD;

let users = [
  { username: "loadtest_01", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_02", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_03", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_04", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_05", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_06", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_07", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_08", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_09", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_10", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_11", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_12", password: passwd, hdid: null, token: null, refresh: null, expires: null },
//  { username: "loadtest_13", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_14", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_15", password: passwd, hdid: null, token: null, refresh: null, expires: null },
  { username: "loadtest_20", password: passwd, hdid: null, token: null, refresh: null, expires: null },

];

function getRandom(min, max) {
  return Math.random() * (max - min) + min;
}

function parseJwt(jwt) {
  var accessToken = jwt.split('.')[1];

  var decoded = b64decode(accessToken, "rawurl");
  var token_json = JSON.parse(decoded);
  return token_json;
};

function getExpiresTime(seconds) {
  return (Date.now() + seconds*1000);
}

function parseHdid(accessToken) {
  var json = parseJwt(accessToken);
  var hdid = json["hdid"];
  return hdid;
}

function authenticateUser(user) {

  let auth_form_data = {
    grant_type: "password",
    client_id: "healthgateway",
    audience: "healthgateway",
    scope: "openid",
    username: user.username,
    password: user.password,
  };

  console.log(TokenEndpointUrl);
  console.log("Authenticating username: " + auth_form_data.username);
  var res = http.post(TokenEndpointUrl, auth_form_data);
  var res_json = JSON.parse(res.body);
  user.token = res_json["access_token"];
  user.refresh = res_json["refresh_token"];
  var seconds = res_json["expires_in"];
  user.expires = getExpiresTime(seconds);
  user.hdid = parseHdid(user.token);
}

function refreshUser(user) {
  let refresh_form_data = {
    grant_type: "refresh_token",
    client_id: "healthgateway",
    refresh_token: user.refresh,
  };

  console.log(TokenEndpointUrl);
  console.log("Getting Refresh Token for username: " + user.username);
  var res = http.post(TokenEndpointUrl, refresh_form_data);
  var res_json = JSON.parse(res.body);
  user.token = res_json["access_token"];
  user.refresh = res_json["refresh_token"];
  var seconds = res_json["expires_in"];
  user.expires = getExpiresTime(seconds);
}

export default function () {

  let access_token = null;
  let hdid = null;

  let user = users[__VU % users.length];

  if (__ITER == 0) {
    if (user.hdid == null) {
      authenticateUser(user);
    }
  }
  if (user.expires < (Date.now() - 3000)) // milliseconds
  {
    refreshUser(user);
  }
  var params = {
    headers: {
      "Content-Type": "application/json",
      Authorization: "Bearer " + user.token,
    },
  };


  group('Timeline flow', function() {
    group('Medication', function() {
      console.log("Medications for username: " + user.username);

      let r = http.get(MedicationServiceUrl + "/" + user.hdid, params);

      if (r.status != 200) console.log("Http Eror Code: " + r.status);


      check(r, {
        "Response Code is 200": (r) => r.status == 200,
        "Response Code is not 504": (r) => r.status != 504,
        "Response Code is not 500": (r) => r.status != 500,
        "Response Code is not 403": (r) => r.status != 403,
      }) || errorRate.add(1);

    });
    group('Laboratory', function() {
      console.log("Laboratory for username: " + user.username);

      let res = http.get(LaboratoryServiceUrl, params);

      if (res.status != 200) console.log("Http Eror Code: " + res.status);

      check(res, {
        "Response Code is 200": (res) => res.status == 200,
        "Response Code is not 504": (res) => res.status != 504,
        "Response Code is not 500": (res) => res.status != 500,
        "Response Code is not 403": (res) => res.status != 403,
      }) || errorRate.add(1);

    });
    sleep(getRandom(5, 15));
  });
}

