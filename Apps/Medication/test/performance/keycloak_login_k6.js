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


export let errorRate = new Rate('errors');

let TokenEndpointUrl = 'https://sso-dev.pathfinder.gov.bc.ca/auth/realms/ff09qn3f/protocol/openid-connect/token';

let auth_form_data = {
  grant_type: "password",
  client_id: "healthgateway",
  audience: "healthgateway",
  scope: "openid",
  username: "loadtest_09",
  password: __ENV.HG_USER_PASSWORD,
};

export default function () {
  check(http.post(TokenEndpointUrl, auth_form_data), {
    'status is 200': r => r.status == 200
  }) || errorRate.add(1);
  sleep(1);
}