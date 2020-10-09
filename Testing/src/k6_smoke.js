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
import { check, group, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import * as common from './inc/common.js';

let groupDuration = Trend('batch');

export let options = {
  vus: 5,
  iterations: 5,
}

function groupWithDurationMetric(name, group_function) {
  let start = new Date();
  group(name, group_function);
  let end = new Date();
  groupDuration.add(end - start, { groupName: name });
}

export default function () {

  let user = common.users[common.getRandomInteger(0, common.users.length - 1)];

  common.authorizeUser(user);

  groupWithDurationMetric('batch', function () {

    let webClientBatchResponses = http.batch(common.webClientRequests(user));
    let timelineBatchResponses = http.batch(common.timelineRequests(user));

    common.checkResponses(webClientBatchResponses);
    common.checkResponses(timelineBatchResponses);
  });

  sleep(1);
}