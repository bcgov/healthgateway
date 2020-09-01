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
import { b64decode } from 'k6/encoding';

let passwd = __ENV.HG_PASSWORD;
let environment = (__ENV.HG_ENV != undefined) ? __ENV.HG_ENV : 'test';

let baseUrl = "https://" + environment + ".healthgateway.gov.bc.ca";
let TokenEndpointUrl = "https://sso-" + environment + ".pathfinder.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token";
export let MedicationServiceUrl = baseUrl + "/api/medicationservice/v1/api/MedicationStatement";
export let LaboratoryServiceUrl = baseUrl + "/api/laboratoryservice/v1/api/Laboratory";
export let PatientServiceUrl = baseUrl + "/api/PatientService/v1/api/Patient";

// Healthgateway WebClient app APIs:
export let BetaRequestUrl = baseUrl + "/v1/api/BetaRequest";
export let CommentUrl = baseUrl + "/v1/api/Comment";
export let CommunicationUrl = baseUrl + "/v1/api/Communication";
export let ConfigurationUrl = baseUrl + "/v1/api/Configuration";
export let NoteUrl = baseUrl + "/v1/api/Note";
export let UserProfileUrl = baseUrl + "/v1/api/UserProfile"; 

export let users = [
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
    { username: "loadtest_13", password: passwd, hdid: null, token: null, refresh: null, expires: null },
    { username: "loadtest_14", password: passwd, hdid: null, token: null, refresh: null, expires: null },
    { username: "loadtest_15", password: passwd, hdid: null, token: null, refresh: null, expires: null },
    { username: "loadtest_20", password: passwd, hdid: null, token: null, refresh: null, expires: null },
];

function parseJwt(jwt) {
    var accessToken = jwt.split('.')[1];

    var decoded = b64decode(accessToken, "rawurl");
    var token_json = JSON.parse(decoded);
    return token_json;
};

function parseHdid(accessToken) {
    var json = parseJwt(accessToken);
    var hdid = json["hdid"];
    return hdid;
}

export function getExpiresTime(seconds) {
    return (Date.now() + seconds * 1000);
}

export function authenticateUser(user) {

    let auth_form_data = {
        grant_type: "password",
        client_id: "healthgateway",
        audience: "healthgateway",
        scope: "openid patient/Laboratory.read",
        username: user.username,
        password: user.password,
    };
    console.log("Authenticating username: " + auth_form_data.username);
    var res = http.post(TokenEndpointUrl, auth_form_data);
    var res_json = JSON.parse(res.body);
    if (res.status == 200) {
        user.token = res_json["access_token"];
        user.refresh = res_json["refresh_token"];
        var seconds = res_json["expires_in"];
        user.expires = getExpiresTime(seconds);
        user.hdid = parseHdid(user.token);
        console.log("hdid=" + user.hdid);
    }
    else {
        console.log("Authentication Error = " + res_json["error"]);
    }
    return res.status;
}

export function refreshUser(user) {
    let refresh_form_data = {
        grant_type: "refresh_token",
        client_id: "healthgateway",
        refresh_token: user.refresh,
    };
    console.log("Getting Refresh Token for username: " + user.username);
    var res = http.post(TokenEndpointUrl, refresh_form_data);
    var res_json = JSON.parse(res.body);
    if (res.status == 200) {
        user.token = res_json["access_token"];
        user.refresh = res_json["refresh_token"];
        var seconds = res_json["expires_in"];
        user.expires = getExpiresTime(seconds);
    }
    else {
        console.log("Error = " + res_json["error"]);
    }
    return res.status;
}

export function getRandomInteger(min, max) {
    let n = Math.floor(Math.random()* (max - min) + min);
    console.log("random integer = " + n);
    return n;
}

export function getRandom(min, max) {
    return Math.random() * (max - min) + min;
}