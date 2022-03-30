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
        { duration: "20s", target: 10 }, // below normal load
        { duration: "1m", target: 10 },
        { duration: "1m", target: 400 }, // spike to super high users
        { duration: "5m", target: 400 }, // stay there
        { duration: "1m", target: 200 }, // scale down
        { duration: "3m", target: 10 },
        { duration: "10s", target: 0 }, //
    ],
};

export default function () {
    let user = common.users[__VU % common.users.length];

    common.authorizeUser(user); // only refreshes if needed.

    let webClientBatchResponses = http.batch(common.webClientRequests(user));
    let timelineBatchResponses = http.batch(common.timelineRequests(user));

    common.checkResponses(webClientBatchResponses);
    common.checkResponses(timelineBatchResponses);

    sleep(1);
}
