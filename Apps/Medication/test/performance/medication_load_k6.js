import http from 'k6/http';
import { check, sleep } from 'k6';
import { b64decode } from 'k6/encoding';
import { Rate } from 'k6/metrics';

export let errorRate = new Rate('errors');

let environment = (__ENV.HG_ENV != undefined) ? __ENV.HG_ENV : 'dev';

let TokenEndpointUrl = "https://sso-" + environment + ".pathfinder.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token";
let ServiceEndPointUrl = "https://" + environment + ".healthgateway.gov.bc.ca/api/medicationservice/v1/api/MedicationStatement";

let passwd = __ENV.HG_PASSWORD;

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
  { username: "loadtest_12", password: passwd, hdid: null, token: null },
  { username: "loadtest_13", password: passwd, hdid: null, token: null },
  { username: "loadtest_14", password: passwd, hdid: null, token: null },
  { username: "loadtest_15", password: passwd, hdid: null, token: null },
  { username: "loadtest_20", password: passwd, hdid: null, token: null },

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

function httpError(r) {
  if (r.status != 200) {
    console.log("Response Code is " + r.status);
    errorRate.add(1);
  }
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
      console.log(TokenEndpointUrl);
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

  let r = http.get(url, params);

  check(r, {
    "Response Code is 200": r => r.status == 200
  }) || httpError(r);

  sleep(getRandom(3, 15));
}

