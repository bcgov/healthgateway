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
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

let passwd = __ENV.HG_PASSWORD;

export let authSuccess = new Rate('authentication_successful');
export let errorRate = new Rate('errors');

export let refreshTokenSuccess = new Rate('auth_refresh_successful');

let environment = (__ENV.HG_ENV) ? __ENV.HG_ENV : 'test'; // default to test environment

let baseUrl = "https://" + environment + ".healthgateway.gov.bc.ca"; // with this, we can be confident that production can't be hit.
let TokenEndpointUrl = "https://" + environment + ".oidc.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token";
export let MedicationServiceUrl = baseUrl + "/api/medicationservice/v1/api/MedicationStatement";
export let LaboratoryServiceUrl = baseUrl + "/api/laboratoryservice/v1/api/Laboratory";
export let PatientServiceUrl = baseUrl + "/api/PatientService/v1/api/Patient";

// console.log("Running tests against baseUrl := " + baseUrl);

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

export function authorizeUser(user) {
    if (((__ITER == 0) && (user.hdid == null)) || (user.hdid == null)) {
        let loginRes = authenticateUser(user);
        check(loginRes, {
            'Authenticated successfully': loginRes === 200
        });
    }
    refreshTokenIfNeeded(user);
}

function authenticateUser(user) {

    let auth_form_data = {
        grant_type: "password",
        client_id: "healthgateway",
        audience: "healthgateway",
        scope: "openid patient/Laboratory.read patient/MedicationStatement.read patient/Immunization.read",
        username: user.username,
        password: user.password,
    };
    console.log("Authenticating username: " + auth_form_data.username);
    var res = http.post(TokenEndpointUrl, auth_form_data);
    if (res.status == 200) {
        var res_json = JSON.parse(res.body);
        user.token = res_json["access_token"];
        user.refresh = res_json["refresh_token"];
        var seconds = res_json["expires_in"];
        user.expires = getExpiresTime(seconds);
        user.hdid = parseHdid(user.token);
        authSuccess.add(1);

    }
    else {
        console.log("Authentication Error for user= " + user.username + ". ResponseCode[" + res.status + "] " + res.error);
        authSuccess.add(0);
        user.token = null;
        user.hdid = null;
    }

    return res.status;
}

 function refreshTokenIfNeeded(user) {

    if ((user.refresh != null) && (user.expires < (Date.now() + 45000))) // refresh 45 seconds before expiry
    {
        refreshUser(user);
    }
}

export function refreshUser(user) {

    if (user.token == null) {
        // means our previous refresh failed.
        return authenticateUser(user);
    }

    let refresh_form_data = {
        grant_type: "refresh_token",
        client_id: "healthgateway",
        refresh_token: user.refresh,
    };

    console.log("Getting Refresh Token for username: " + user.username);
    let res = http.post(TokenEndpointUrl, refresh_form_data);

    if (res.status == 200) {
        var res_json = JSON.parse(res.body);
        user.token = res_json["access_token"];
        user.refresh = res_json["refresh_token"];
        var seconds = res_json["expires_in"];
        user.expires = getExpiresTime(seconds);
        refreshTokenSuccess.add(1);
    }
    else {
        console.log("Token Refresh Error for user= " + user.username + ". ResponseCode[" + res.status + "] " + res.error);
        refreshTokenSuccess.add(0);
        user.token = null; // clear out the expiring token, forcing to re-authenticate.
        user.hdid = null;
        sleep(1);
        return authenticateUser(user);
    }
    return res.status;
}

export function getRandomInteger(min, max) {
    return Math.floor(Math.random() * (max - min) + min);
}

export function getRandom(min, max) {
    return Math.random() * (max - min) + min;
}

export function params(user) {
    var params = {
        headers: {
            "Content-Type": "application/json",
            Authorization: "Bearer " + user.token,
        },
    };
    return params;
}

export function timelineRequests(user) {
    let timelineRequest = {
        'comment': {
            method: 'GET',
            url: common.CommentUrl + "/" + user.hdid,
            params: params(user)
        },
        'note': {
            method: 'GET',
            url: common.NoteUrl + "/" + user.hdid,
            params: params(user)
        },
        'meds': {
            method: 'GET',
            url: common.MedicationServiceUrl + "/" + user.hdid,
            params: params(user)
        },

        'labs': {
            method: 'GET',
            url: common.LaboratoryServiceUrl + "?hdid=" + user.hdid,
            params: params(user)
        }
    };
    return timelineRequest;
}

export function webClientRequests(user) {
    let webClientRequests = {
        'patient': {
            method: 'GET',
            url: common.PatientServiceUrl + "/" + user.hdid,
            params: params(user)
        },
        'beta': {
            method: 'GET',
            url: common.BetaRequestUrl + "/" + user.hdid,
            params: params(user)
        },
        'communication': {
            method: 'GET',
            url: common.CommunicationUrl + "/" + user.hdid,
            params: params(user)
        },
        'conf': {
            method: 'GET',
            url: common.ConfigurationUrl + "/" + user.hdid,
            params: params(user)
        }
    };
    return webClientRequests;
}

export function checkResponses(responses) {

    if (responses['beta']) {
        var ok = check(responses['beta'], {
            "Beta HttpStatusCode is 200": (r) => r.status === 200,
            "Beta HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "Beta HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "Beta HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "Beta HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "Beta HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }

    if (responses['comment']) {
        check(responses['comment'], {
            "Beta HttpStatusCode is 200": (r) => r.status === 200,
            "Beta HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "Beta HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "Beta HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "Beta HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "Beta HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }

    if (responses['communication']) {
        check(responses['communication'], {
            "Communication HttpStatusCode is 200": (r) => r.status === 200,
            "Communication HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "Communication HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "Communication HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "Communication HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "Communication HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }

    if (responses['conf']) {
        check(responses['conf'], {
            "Configuration HttpStatusCode is 200": (r) => r.status === 200,
            "Configuration HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "Configuration HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "Configuration HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "Configuration HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "Configuration HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }

    if (responses['labs']) {
        check(responses['labs'], {
            "LaboratoryService HttpStatusCode is 200": (r) => r.status === 200,
            "LaboratoryService HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "LaboratoryService HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "LaboratoryService HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "LaboratoryService HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "LaboratoryService HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }

    if (responses['meds']) {
        check(responses['meds'], {
            "MedicationService HttpStatusCode is 200": (r) => r.status === 200,
            "MedicationService HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "MedicationService HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "MedicationService HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "MedicationService HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "MedicationService HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }
    if (responses['note']) {
        check(responses['note'], {
            "Note HttpStatusCode is 200": (r) => r.status === 200,
            "Note HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "Note HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "Note HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "Note HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "Note HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }

    if (responses['patient']) {
        check(responses['patient'], {
            "PatientService HttpStatusCode is 200": (r) => r.status === 200,
            "PatientService HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "PatientService HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "PatientService HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "PatientService HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "PatientService HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }
    if (responses['profile']) {
        check(responses['profile'], {
            "UserProfile HttpStatusCode is 200": (r) => r.status === 200,
            "UserProfile HttpStatusCode is NOT 3xx Redirection": (r) => !((r.status >= 300) && (r.status <= 306)),
            "UserProfile HttpStatusCode is NOT 401 Unauthorized": (r) => (r.status != 401),
            "UserProfile HttpStatusCode is NOT 4xx Client Error": (r) => !((r.status >= 400) && (r.status <= 499)),
            "UserProfile HttpStatusCode is NOT 5xx Server Error": (r) => !((r.status >= 500) && (r.status <= 598)),
            "UserProfile HttpStatusCode is NOT 0 (Timeout Error)": (r) => (r.status != 0),
        }) || errorRate.add(1);
    }
}
