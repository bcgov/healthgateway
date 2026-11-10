import { ErrorSourceType } from "@/constants/errorType";
import { Dataset } from "@/constants/tracking";
import type { Encounter } from "@/models/encounter";
import { services } from "@/services/container";

import { createRequestResultStore } from "./utils/createRequestResultStore";

/** Port of stores/healthVisit.ts */
const healthVisits = createRequestResultStore<Encounter>({
    name: "Health Visits",
    fetch: (hdid) => services.encounter.getPatientEncounters(hdid),
    dataset: Dataset.HealthVisits,
    errorSource: ErrorSourceType.Encounter,
});

export const useHealthVisitStore = healthVisits.store;
export const selectHealthVisits = healthVisits.selectData;
export const selectHealthVisitsAreLoading = healthVisits.selectIsLoading;
