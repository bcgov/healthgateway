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
import { b64decode } from "k6/encoding";
import { check, group, sleep } from "k6";
import { Rate, Trend } from "k6/metrics";
import * as TestOptions from "./testOptions";

export let passwd = __ENV.HG_PASSWORD;

export let maxVus = __ENV.HG_VUS ? __ENV.HG_VUS : 300;
maxVus = maxVus < 1 ? 1 : maxVus;
export let rampVus = (maxVus / 4).toFixed(0);
rampVus = rampVus < 1 ? 1 : rampVus;

export let authSuccess = new Rate("authentication_successful");
export let errorRate = new Rate("errors");

export let refreshTokenSuccess = new Rate("auth_refresh_successful");

export let environment = __ENV.HG_ENV ? __ENV.HG_ENV : "test"; // default to test environment. choice of dev, test
export let OptionsType = __ENV.HG_TYPE ? __ENV.HG_TYPE : "smoke"; // choice of load, smoke, soak, spike, stress

export let groupDuration = Trend("batch");

export let baseUrl = "https://" + environment + ".healthgateway.gov.bc.ca"; // with this, we can be confident that production can't be hit.
export let TokenEndpointUrl =
    "https://" +
    environment +
    ".oidc.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token";
export let MedicationServiceUrl =
    baseUrl + "/api/medicationservice/v1/api/MedicationStatement";
export let LaboratoryServiceUrl =
    baseUrl + "/api/laboratoryservice/v1/api/Laboratory/LaboratoryOrders";
export let PatientServiceUrl = baseUrl + "/api/PatientService/v1/api/Patient";

// Health Gateway WebClient app APIs:
export let CommentUrl = baseUrl + "/v1/api/Comment";
export let CommunicationUrl = baseUrl + "/v1/api/Communication";
export let ConfigurationUrl = baseUrl + "/v1/api/Configuration";
export let NoteUrl = baseUrl + "/v1/api/Note";
export let UserProfileUrl = baseUrl + "/v1/api/UserProfile";

export let ClientId = __ENV.HG_CLIENT ? __ENV.HG_CLIENT : "k6"; // default to k6 client id

export let users = [
    {
        username: "loadtest_01",
        password: passwd,
        hdid: null,
        token: null,
        refresh: null,
        expires: null,
    },
    {
        username: "loadtest_02",
        password: passwd,
        hdid: null,
        token: null,
        refresh: null,
        expires: null,
    },
    {
        username: "loadtest_03",
        password: passwd,
        hdid: null,
        token: null,
        refresh: null,
        expires: null,
    },
    {
        username: "loadtest_04",
        password: passwd,
        hdid: null,
        token: null,
        refresh: null,
        expires: null,
    },
    {
        username: "loadtest_05",
        password: passwd,
        hdid: null,
        token: null,
        refresh: null,
        expires: null,
    },
    {
        username: "loadtest_06",
        password: passwd,
        hdid: null,
        token: null,
        refresh: null,
        expires: null,
    },
    {
        username: "loadtest_07",
        password: passwd,
        hdid: null,
        token: null,
        refresh: null,
        expires: null,
    },
    {
        username: "loadtest_08",
        password: passwd,
        hdid: null,
        token: null,
        refresh: null,
        expires: null,
    }
];

function parseJwt(jwt) {
    var accessToken = jwt.split(".")[1];

    var decoded = b64decode(accessToken, "rawurl");
    var token_json = JSON.parse(decoded);
    return token_json;
}

function parseHdid(accessToken) {
    var json = parseJwt(accessToken);
    var hdid = json["hdid"];
    return hdid;
}

export function groupWithDurationMetric(name, group_function) {
    let start = new Date();
    group(name, group_function);
    let end = new Date();
    groupDuration.add(end - start, { groupName: name });
}

export function OptionConfig() {
    if (OptionsType == "load") {
        return TestOptions.loadOptions;
    }
    if (OptionsType === "smoke") {
        return TestOptions.smokeOptions;
    }
    if (OptionsType === "soak") {
        return TestOptions.soakOptions;
    }
    if (OptionsType === "spike") {
        return TestOptions.soakOptions;
    }
    if (OptionsType === "stress") {
        return soakOptions;
    }
    return TestOptions.smokeOptions;
}

export function getExpiresTime(seconds) {
    return Date.now() + seconds * 1000;
}

export function authorizeUser(user) {
    if ((__ITER == 0 && user.token == null) || user.hdid == null) {
        let loginRes = authenticateUser(user);
        check(loginRes, {
            "Authenticated successfully": loginRes === 200,
        });
    }
    refreshTokenIfNeeded(user);
}

function authenticateUser(user) {
    let auth_form_data = {
        grant_type: "password",
        client_id: ClientId,
        audience: "healthgateway",
        scope:
            "openid patient/Laboratory.read patient/MedicationStatement.read patient/Immunization.read",
        username: user.username,
        password: user.password,
    };
    console.log(
        "Authenticating username: " +
        auth_form_data.username +
        ", KeyCloak client_id: " +
        ClientId
    );
    var res = http.post(TokenEndpointUrl, auth_form_data);
    if (res.status == 200) {
        var res_json = JSON.parse(res.body);
        user.token = res_json["access_token"];
        user.refresh = res_json["refresh_token"];
        var seconds = res_json["expires_in"];
        user.expires = getExpiresTime(seconds);
        user.hdid = parseHdid(user.token);
        authSuccess.add(1);
    } else {
        console.log(
            "Authentication Error for user= " +
            user.username +
            ". ResponseCode[" +
            res.status +
            "] " +
            res.error
        );
        authSuccess.add(0);
        user.token = null;
        user.hdid = null;
    }

    return res.status;
}

function refreshTokenIfNeeded(user) {
    if (user.refresh != null && user.expires < Date.now() + 45000) {
        // refresh 45 seconds before expiry
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
        client_id: ClientId,
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
    } else {
        console.log(
            "Token Refresh Error for user= " +
            user.username +
            ". ResponseCode[" +
            res.status +
            "] " +
            res.error
        );
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

export function getHdid(user) {
    return user.hdid;
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
    let timelineRequests = {
        comments: {
            method: "GET",
            url: common.CommentUrl + "/" + user.hdid,
            params: params(user),
        },
        notes: {
            method: "GET",
            url: common.NoteUrl + "/" + user.hdid,
            params: params(user),
        },
        meds: {
            method: "GET",
            url: common.MedicationServiceUrl + "/" + user.hdid,
            params: params(user),
        },

        labs: {
            method: "GET",
            url: common.LaboratoryServiceUrl + "?hdid=" + user.hdid,
            params: params(user),
        },
    };
    return timelineRequests;
}

export function webClientRequests(user) {
    let webClientRequests = {
        patient: {
            method: "GET",
            url: common.PatientServiceUrl + "/" + user.hdid,
            params: params(user),
        },
        communication: {
            method: "GET",
            url: common.CommunicationUrl + "/" + user.hdid,
            params: params(user),
        },
        configuration: {
            method: "GET",
            url: common.ConfigurationUrl + "/" + user.hdid,
            params: params(user),
        },
        profile: {
            method: "GET",
            url: common.UserProfileUrl + "/" + user.hdid,
            params: params(user),
        },
    };
    return webClientRequests;
}

function isObject(val) {
    if (val === null) {
        return false;
    }
    return typeof val === "object";
}

export function checkResponse(response) {
    if (isObject(response)) {
        var ok =
            check(response, {
                "HttpStatusCode is 200": (r) => r.status === 200,
                "HttpStatusCode is NOT 3xx Redirection": (r) =>
                    !(r.status >= 300 && r.status <= 306),
                "HttpStatusCode is NOT 401 Unauthorized": (r) =>
                    r.status != 401,
                "HttpStatusCode is NOT 402 Payment Required": (r) =>
                    r.status != 402,
                "HttpStatusCode is NOT 403 Forbidden": (r) => r.status != 403,
                "HttpStatusCode is NOT 404 Not Found": (r) => r.status != 404,
                "HttpStatusCode is NOT 405 Method Not Allowed": (r) =>
                    r.status != 405,
                "HttpStatusCode is NOT 406 Not Acceptable": (r) =>
                    r.status != 406,
                "HttpStatusCode is NOT 407 Proxy Authentication Required": (
                    r
                ) => r.status != 407,
                "HttpStatusCode is NOT 408 Request Timeout": (r) =>
                    r.status != 408,
                "HttpStatusCode is NOT 4xx Client Error": (r) =>
                    !(r.status >= 409 && r.status <= 499),
                "HttpStatusCode is NOT 5xx Server Error": (r) =>
                    !(r.status >= 500 && r.status <= 598),
                "HttpStatusCode is NOT 0 (Timeout Error)": (r) => r.status != 0,
            }) || errorRate.add(1);
        return;
    }
}

export function checkResponses(responses) {
    if (responses["beta"]) {
        var ok =
            check(responses["beta"], {
                "Beta HttpStatusCode is 200": (r) => r.status === 200,
                "Beta HttpStatusCode is NOT 3xx Redirection": (r) =>
                    !(r.status >= 300 && r.status <= 306),
                "Beta HttpStatusCode is NOT 401 Unauthorized": (r) =>
                    r.status != 401,
                "Beta HttpStatusCode is NOT 4xx Client Error": (r) =>
                    !(r.status >= 400 && r.status <= 499),
                "Beta HttpStatusCode is NOT 5xx Server Error": (r) =>
                    !(r.status >= 500 && r.status <= 598),
                "Beta HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                    r.status != 0,
            }) || errorRate.add(1);
    }
    if (responses["comments"]) {
        check(responses["comments"], {
            "Beta HttpStatusCode is 200": (r) => r.status === 200,
            "Beta HttpStatusCode is NOT 3xx Redirection": (r) =>
                !(r.status >= 300 && r.status <= 306),
            "Beta HttpStatusCode is NOT 401 Unauthorized": (r) =>
                r.status != 401,
            "Beta HttpStatusCode is NOT 4xx Client Error": (r) =>
                !(r.status >= 400 && r.status <= 499),
            "Beta HttpStatusCode is NOT 5xx Server Error": (r) =>
                !(r.status >= 500 && r.status <= 598),
            "Beta HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                r.status != 0,
        }) || errorRate.add(1);
    }
    if (responses["communication"]) {
        check(responses["communication"], {
            "Communication HttpStatusCode is 200": (r) => r.status === 200,
            "Communication HttpStatusCode is NOT 3xx Redirection": (r) =>
                !(r.status >= 300 && r.status <= 306),
            "Communication HttpStatusCode is NOT 401 Unauthorized": (r) =>
                r.status != 401,
            "Communication HttpStatusCode is NOT 4xx Client Error": (r) =>
                !(r.status >= 400 && r.status <= 499),
            "Communication HttpStatusCode is NOT 5xx Server Error": (r) =>
                !(r.status >= 500 && r.status <= 598),
            "Communication HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                r.status != 0,
        }) || errorRate.add(1);
    }
    if (responses["configuration"]) {
        check(responses["configuration"], {
            "Configuration HttpStatusCode is 200": (r) => r.status === 200,
            "Configuration HttpStatusCode is NOT 3xx Redirection": (r) =>
                !(r.status >= 300 && r.status <= 306),
            "Configuration HttpStatusCode is NOT 401 Unauthorized": (r) =>
                r.status != 401,
            "Configuration HttpStatusCode is NOT 4xx Client Error": (r) =>
                !(r.status >= 400 && r.status <= 499),
            "Configuration HttpStatusCode is NOT 5xx Server Error": (r) =>
                !(r.status >= 500 && r.status <= 598),
            "Configuration HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                r.status != 0,
        }) || errorRate.add(1);
    }
    if (responses["labs"]) {
        check(responses["labs"], {
            "LaboratoryService HttpStatusCode is 200": (r) => r.status === 200,
            "LaboratoryService HttpStatusCode is NOT 3xx Redirection": (r) =>
                !(r.status >= 300 && r.status <= 306),
            "LaboratoryService HttpStatusCode is NOT 401 Unauthorized": (r) =>
                r.status != 401,
            "LaboratoryService HttpStatusCode is NOT 4xx Client Error": (r) =>
                !(r.status >= 400 && r.status <= 499),
            "LaboratoryService HttpStatusCode is NOT 5xx Server Error": (r) =>
                !(r.status >= 500 && r.status <= 598),
            "LaboratoryService HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                r.status != 0,
        }) || errorRate.add(1);
    }
    if (responses["meds"]) {
        check(responses["meds"], {
            "MedicationService HttpStatusCode is 200": (r) => r.status === 200,
            "MedicationService HttpStatusCode is NOT 3xx Redirection": (r) =>
                !(r.status >= 300 && r.status <= 306),
            "MedicationService HttpStatusCode is NOT 401 Unauthorized": (r) =>
                r.status != 401,
            "MedicationService HttpStatusCode is NOT 4xx Client Error": (r) =>
                !(r.status >= 400 && r.status <= 499),
            "MedicationService HttpStatusCode is NOT 5xx Server Error": (r) =>
                !(r.status >= 500 && r.status <= 598),
            "MedicationService HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                r.status != 0,
        }) || errorRate.add(1);
    }
    if (responses["notes"]) {
        check(responses["notes"], {
            "Note HttpStatusCode is 200": (r) => r.status === 200,
            "Note HttpStatusCode is NOT 3xx Redirection": (r) =>
                !(r.status >= 300 && r.status <= 306),
            "Note HttpStatusCode is NOT 401 Unauthorized": (r) =>
                r.status != 401,
            "Note HttpStatusCode is NOT 4xx Client Error": (r) =>
                !(r.status >= 400 && r.status <= 499),
            "Note HttpStatusCode is NOT 5xx Server Error": (r) =>
                !(r.status >= 500 && r.status <= 598),
            "Note HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                r.status != 0,
        }) || errorRate.add(1);
    }
    if (responses["patient"]) {
        check(responses["patient"], {
            "PatientService HttpStatusCode is 200": (r) => r.status === 200,
            "PatientService HttpStatusCode is NOT 3xx Redirection": (r) =>
                !(r.status >= 300 && r.status <= 306),
            "PatientService HttpStatusCode is NOT 401 Unauthorized": (r) =>
                r.status != 401,
            "PatientService HttpStatusCode is NOT 4xx Client Error": (r) =>
                !(r.status >= 400 && r.status <= 499),
            "PatientService HttpStatusCode is NOT 5xx Server Error": (r) =>
                !(r.status >= 500 && r.status <= 598),
            "PatientService HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                r.status != 0,
        }) || errorRate.add(1);
    }
    if (responses["profile"]) {
        check(responses["profile"], {
            "UserProfile HttpStatusCode is 200": (r) => r.status === 200,
            "UserProfile HttpStatusCode is NOT 3xx Redirection": (r) =>
                !(r.status >= 300 && r.status <= 306),
            "UserProfile HttpStatusCode is NOT 401 Unauthorized": (r) =>
                r.status != 401,
            "UserProfile HttpStatusCode is NOT 4xx Client Error": (r) =>
                !(r.status >= 400 && r.status <= 499),
            "UserProfile HttpStatusCode is NOT 5xx Server Error": (r) =>
                !(r.status >= 500 && r.status <= 598),
            "UserProfile HttpStatusCode is NOT 0 (Timeout Error)": (r) =>
                r.status != 0,
        }) || errorRate.add(1);
    }
}
