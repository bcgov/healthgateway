import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';


export let errorRate = new Rate('errors');

let TokenEndpointUrl = 'https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token';

let auth_form_data = {
  grant_type: "password",
  client_id: "healthgateway",
  audience: "healthgateway",
  scope: "openid",
  username: "loadtest_01",
  password: __ENV.HG_USER_PASSWORD,
};

export default function () {
  check(http.post(TokenEndpointUrl, auth_form_data), {
    'status is 200': r => r.status == 200
  }) || errorRate.add(1);
  sleep(1);
}