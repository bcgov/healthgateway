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
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

export let passwd = __ENV.HG_PASSWORD;

export let maxVus = __ENV.HG_VUS ? parseInt(__ENV.HG_VUS) : 500;
maxVus = (maxVus < 1) ? 1 : maxVus;
export let rampVus = (maxVus / 4).toFixed(0);
rampVus = (rampVus < 1) ? 1 : rampVus;

export let AuthSuccess = new Rate("authentication_successful");
export let ErrorRate = new Rate("errors");

export let RefreshTokenSuccess = new Rate("auth_refresh_successful");

export let SpecialHeaderKey = __ENV.HG_KEY ? __ENV.HG_KEY : "nokey"; // special key 

export let Environment = __ENV.HG_ENV ? __ENV.HG_ENV : "test"; // default to test environment. choice of dev, test (never prod)
export let OptionsType = __ENV.HG_TYPE ? __ENV.HG_TYPE : "smoke"; // choice of load, smoke, soak, spike, stress

export let groupDuration = Trend("batch");

export let BaseSiteUrl = "https://" + Environment + ".healthgateway.gov.bc.ca/";

// Service Endpoints Configuration
export let ConfigurationUrl = BaseSiteUrl + "configuration";

// OpenID Connect Endpoints
export let OpenIdConnect = {
    AuthorityEndpoint: "",
    TokenEndpoint: "",
    AuthorizationEndpoint: "",
    Audience: "healthgateway",
    ClientId: "k6",
    Scope: "openid patient/Laboratory.read patient/MedicationStatement.read patient/Immunization.read patient/Encounter.read patient/Patient.read",
};

// Gateway Service Endpoints
export let ServiceEndpoints = {
    Encounter: "",
    GatewayApi: "",
    Immunization: "",
    Laboratory: "",
    Medication: "",
    Patient: "",
};
//-------------------------------------------------------------------------
export let loadOptions = {
    vus: maxVus,
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
export let smokeOptions = {
    vus: 1,
    iterations: 1,
};
export let soakOptions = {
    stages: [
        { duration: "1m", target: 10 }, // below normal load
        { duration: "2m", target: maxVus },
        { duration: "3h56m", target: maxVus }, // stay at high users for hours 'soaking' the system
        { duration: "2m", target: 0 }, // drop back down
    ],
};
export let spikeOptions = {
    stages: [
        { duration: "20s", target: 10 }, // below normal load
        { duration: "1m", target: 10 },
        { duration: "1m", target: maxVus }, // spike to super high users
        { duration: "5m", target: maxVus }, // stay there
        { duration: "1m", target: rampVus }, // scale down
        { duration: "3m", target: 10 },
        { duration: "10s", target: 0 }, //
    ],
};

let stressVus = maxVus + 50;
let stressMaxVus = maxVus + 150;

export let stressOptions = {
    stages: [
        { duration: "2m", target: 10 }, // below normal load
        { duration: "5m", target: 50 },
        { duration: "5m", target: rampVus },
        { duration: "5m", target: maxVus }, // around the breaking point
        { duration: "2m", target: stressVus }, // beyond the breaking point
        { duration: "5m", target: stressMaxVus },
        { duration: "5m", target: 0 }, // scale down. Recovery stage.
    ],
};
//----------------------------------------------------------------------
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
    var middlepart = String(jwt).split(".")[1];
    var decoded = b64decode(String(middlepart), "rawurl", 's');
    var token_json = JSON.parse(decoded);
    return token_json;
}

function parseHdid(accessToken) {
    var json = parseJwt(accessToken);
    var hdid = json["hdid"];
    console.log("hdid= " + hdid);
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
        return loadOptions;
    }
    if (OptionsType === "smoke") {
        return smokeOptions;
    }
    if (OptionsType === "soak") {
        return soakOptions;
    }
    if (OptionsType === "spike") {
        return spikeOptions;
    }
    if (OptionsType === "stress") {
        return stressOptions;
    }
    return smokeOptions;
}

export function getExpiresTime(seconds) {
    return Date.now() + seconds * 1000;
}

export function getConfigurations() {
    if (__ITER == 0) {
        let response = http.get(ConfigurationUrl);
        if (response.status == 200) {
            var responseJson = JSON.parse(response.body);
            var endpoints = responseJson["serviceEndpoints"];
            ServiceEndpoints.Encounter = endpoints["Encounter"];
            ServiceEndpoints.GatewayApi = endpoints["GatewayApi"];
            ServiceEndpoints.Immunization = endpoints["Immunization"];
            ServiceEndpoints.Laboratory = endpoints["Laboratory"];
            ServiceEndpoints.Medication = endpoints["Medication"];
            ServiceEndpoints.Patient = endpoints["Patient"];

            let openIdConnect = responseJson["openIdConnect"];
            OpenIdConnect.AuthorityEndpoint = openIdConnect["authority"];
            OpenIdConnect.Audience = openIdConnect["audience"];
            OpenIdConnect.ClientId = openIdConnect["clientId"];
        }

        else {
            console.warn("Failed to get HG configuration");
        }
    }
}

export function getOpenIdConfigurations() {
    if (__ITER == 0) {
        let response = http.get(OpenIdConnect.AuthorityEndpoint + "/.well-known/openid-configuration");
        if (response.status == 200) {
            var responseJson = JSON.parse(response.body);
            OpenIdConnect.TokenEndpoint = responseJson["token_endpoint"];
            OpenIdConnect.AuthorizationEndpoint = responseJson["authorization_endpoint"];
        }
        else {
            console.warn("Failed to get OpenId Configuration ");
        }
    }
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

export function authenticateUser(user) {
    let auth_form_data = {
        grant_type: "password",
        client_id: OpenIdConnect.ClientId,
        audience: OpenIdConnect.Audience,
        scope: OpenIdConnect.Scope,
        username: user.username,
        password: user.password,
    };

    var res = http.post(OpenIdConnect.TokenEndpoint, auth_form_data,
        {
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        });
    if (res.status == 200) {
        var res_json = JSON.parse(res.body);
        user.token = res_json["access_token"];
        user.refresh = res_json["refresh_token"];
        var seconds = res_json["expires_in"];
        user.expires = getExpiresTime(seconds);
        user.hdid = parseHdid(user.token);
        AuthSuccess.add(1);
    } else {
        console.log(
            "Authentication Error for user= " +
            user.username +
            ". ResponseCode[" +
            res.status +
            "] " +
            res.error
        );
        AuthSuccess.add(0);
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
    let res = http.post(OpenIdConnect.TokenEndpoint, refresh_form_data);

    if (res.status == 200) {
        var res_json = JSON.parse(res.body);
        user.token = res_json["access_token"];
        user.refresh = res_json["refresh_token"];
        var seconds = res_json["expires_in"];
        user.expires = getExpiresTime(seconds);
        RefreshTokenSuccess.add(1);
    } else {
        console.log(
            "Token Refresh Error for user= " +
            user.username +
            ". ResponseCode[" +
            res.status +
            "] " +
            res.error
        );
        RefreshTokenSuccess.add(0);
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

export let HttpHeaders = {
    'User-Agent': 'k6',
    'X-API-KEY': SpecialHeaderKey,
}

export function params(user) {
    var params = {
        headers: {
            httpHeaders: HttpHeaders,
            Authorization: "Bearer " + user.token,
        },
    };
    return params;
}

export function timelineRequests(user) {
    let timelineRequests = {
        comments: {
            method: "GET",
            url: common.ServiceEndpoints.GatewayApi + "UserProfile/" + user.hdid + "/Comment",
            params: params(user),
        },
        immz: {
            method: "GET",
            url: common.ServiceEndpoints.Immunization + "Immunization?hdid=" + user.hdid,
            params: params(user),
        },
        labs: {
            method: "GET",
            url: common.ServiceEndpoints.Laboratory + "Laboratory/LaboratoryOrders?hdid=" + user.hdid,
            params: params(user),
        },

        meds: {
            method: "GET",
            url: common.ServiceEndpoints.Medication + "MedicationStatement/" + user.hdid,
            params: params(user),
        },
        notes: {
            method: "GET",
            url: common.ServiceEndpoints.GatewayApi + "Note/" + user.hdid,
            params: params(user),
        },

    };
    return timelineRequests;
}

export function getBaseWebApp() {
    let baseWebAppRequest = {
        baseSite: {
            method: "GET",
            url: BaseSiteUrl,
            params: { headers: HttpHeaders }
        }
    };
    return baseWebAppRequest
}

export function spaAssetRequests() {

    let spaAssetRequests = {
        baseSite: {
            method: "GET",
            url: BaseSiteUrl,
            params: { headers: HttpHeaders }
        },
        vendorChunk: {
            method: "GET",
            url: BaseSiteUrl + "/js/chunk-vendors.c61f122d.js",
            params: { headers: HttpHeaders }
        },
        siteChunk: {
            method: "GET",
            url: BaseSiteUrl + "/js/app.8136e1c8.js",
            params: { headers: HttpHeaders }
        },
        css: {
            method: "GET",
            url: BaseSiteUrl + "/css/app.c90e9393.css",
            params: { headers: HttpHeaders }
        },
        cssVendor: {
            method: "GET",
            url: BaseSiteUrl + "/css/chunk-vendors.21f4bba7.css",
            params: { headers: HttpHeaders }
        },

    }
    return spaAssetRequests;
}

export function webClientRequests(user) {
    let webClientRequests = {
        patient: {
            method: "GET",
            url: common.ServiceEndpoints.Patient + "Patient/" + user.hdid,
            params: params(user),
        },
        profile: {
            method: "GET",
            url: common.GatewayApi + "UserProfile/" + user.hdid,
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

export function checkForRequestResult(response) {
    if (isObject(response)) {

        if (response.status === 200) {
            // console.log(response.body)
            var json = JSON.parse(response.body);
            check(json, {
                "Returned 200 Ok with NO JSON": (r) => isObject(r)
            });
            if (isObject(json)) {
                var resultJson = json["resultError"];
                check(resultJson, {
                    "ActionCode is NOT REFRESH": (r) => isObject(r) && r.actionCode != "REFRESH"
                });
            }
        }

    }
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
            }) || ErrorRate.add(1);
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
            }) || ErrorRate.add(1);
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
        }) || ErrorRate.add(1);
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
        }) || ErrorRate.add(1);
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
        }) || ErrorRate.add(1);
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
        }) || ErrorRate.add(1);
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
        }) || ErrorRate.add(1);
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
        }) || ErrorRate.add(1);
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
        }) || ErrorRate.add(1);
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
        }) || ErrorRate.add(1);
    }
}
