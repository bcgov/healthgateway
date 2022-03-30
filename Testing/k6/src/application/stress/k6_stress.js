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
import { sleep } from "k6";
import * as common from "../../inc/common.js";

export let options = {
    stages: [
        { duration: "2m", target: 50 }, // below normal load
        { duration: "5m", target: 100 },
        { duration: "2m", target: 200 }, // normal load
        { duration: "5m", target: 200 },
        { duration: "2m", target: 400 }, // around the breaking point
        { duration: "4m", target: 400 },
        { duration: "2m", target: 500 }, // beyond the breaking point
        { duration: "5m", target: 550 },
        { duration: "5m", target: 0 }, // scale down. Recovery stage.
    ],
};

export default function () {
    let user = common.users[__VU % common.users.length];

    common.authorizeUser(user);

    let webClientBatchResponses = http.batch(common.webClientRequests(user));
    let timelineBatchResponses = http.batch(common.timelineRequests(user));

    common.checkResponses(webClientBatchResponses);
    common.checkResponses(timelineBatchResponses);

    sleep(common.getRandom(0.5, 3.0));
}
