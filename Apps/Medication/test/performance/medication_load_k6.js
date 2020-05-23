import http from 'k6/http';
import { check, sleep } from 'k6';
import { b64decode } from 'k6/encoding';
import { Rate } from 'k6/metrics';

export let errorRate = new Rate('errors');

let TokenEndpointUrl = "https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token";
let ServiceEndPointUrl = "https://dev.healthgateway.gov.bc.ca/api/medicationservice/v1/api/MedicationStatement";

let passwd = __ENV.HG_USER_PASSWORD;

let users = [
  { username: "loadtest_01", password: passwd, hdid: null, token: null },
  { username: "loadtest_02", password: passwd, hdid: null, token: null },
  { username: "loadtest_03", password: passwd, hdid: null, token: null },
  { username: "loadtest_04", password: passwd, hdid: null, token: null },
  { username: "loadtest_05", password: passwd, hdid: null, token: null },
  { username: "loadtest_06", password: passwd, hdid: null, token: null },
  { username: "loadtest_07", password: passwd, hdid: null, token: null },
  { username: "loadtest_08", password: passwd, hdid: null, token: null },
  { username: "loadtest_09", password: passwd, hdid: null, token: null },
  { username: "loadtest_10", password: passwd, hdid: null, token: null },
  { username: "loadtest_11", password: passwd, hdid: null, token: null },
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

function parseHdid(accessToken) {
  var json = parseJwt(accessToken);
  var hdid = json["hdid"];
  return hdid;
}

export default function () {

  let access_token = null;
  let hdid = null;

  let user = users[__VU % users.length];

  let auth_form_data = {
    grant_type: "password",
    client_id: "healthgateway",
    audience: "healthgateway",
    scope: "openid",
    username: user.username,
    password: user.password,
  };

  if (__ITER == 0) {
    if (user.hdid == null) {
      console.log("Authenticating username: " + auth_form_data.username);
      var res = http.post(TokenEndpointUrl, auth_form_data);
      var res_json = JSON.parse(res.body);
      user.token = res_json["access_token"];
      user.hdid = parseHdid(user.token);
    }
  }

  var url = ServiceEndPointUrl + "/" + user.hdid;
  console.log('url = ' + url);

  var params = {
    headers: {
      "Content-Type": "application/json",
      Authorization: "Bearer " + user.token,
    },
  };

  console.log("Medications for username: " + auth_form_data.username);

  check(http.get(url, params), {
    "Status is 200": r => r.status == 200
  }) || errorRate.add(1);

  sleep(getRandom(2, 10));
}

