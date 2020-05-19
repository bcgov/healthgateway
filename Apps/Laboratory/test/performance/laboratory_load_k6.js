import http from 'k6/http';
import { check, sleep } from 'k6';
import { b64decode } from 'k6/encoding';
import { Rate } from 'k6/metrics';


export let errorRate = new Rate('errors');

let TokenEndpointUrl = 'https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token';
let ServiceEndPointUrl = 'https://dev.healthgateway.gov.bc.ca/api/laboratoryservice/v1/api/Laboratory';

let passwd = __ENV.USER_PASSWORD;

let auth_form_data = {
  grant_type: "password",
  client_id: "healthgateway",
  audience: "healthgateway",
  scope: "openid",
  username: "2gateway",
  password: passwd
};

let access_token = null;
let hdid = null;

export default function () {

  if (access_token == null) {
    console.log('USER_PASSWORD = ' + __ENV.USER_PASSWORD);

    var res = http.post(TokenEndpointUrl, auth_form_data);
    var res_json = JSON.parse(res.body);
    access_token = res_json['access_token'];

    var b64Token = access_token.split('.')[1];
    var decoded = b64decode(b64Token);
    var token_json = JSON.parse(decoded);
    hdid = token_json['hdid'];
  }
  var url = ServiceEndPointUrl + '/' + hdid;
  var params = {
    headers: {
      Authorization: 'Bearer ' + access_token,
      'Content-Type': 'application/json'
    }
  };
  check(http.get(url, params), {
    'status is 200': r => r.status == 200
  }) ||  errorRate.add(1);

  sleep(0.3);
}