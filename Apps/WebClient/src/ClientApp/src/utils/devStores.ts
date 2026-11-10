import { useClinicalDocumentStore } from "@/stores/clinicalDocument.store";
import { useCommentStore } from "@/stores/comment.store";
import { useConfigStore } from "@/stores/config.store";
import { useCovid19TestResultStore } from "@/stores/covid19TestResult.store";
import { useErrorStore } from "@/stores/error.store";
import { useHealthVisitStore } from "@/stores/healthVisit.store";
import { useHospitalVisitStore } from "@/stores/hospitalVisit.store";
import { useImmunizationStore } from "@/stores/immunization.store";
import { useLabResultStore } from "@/stores/labResult.store";
import { useLoadingStore } from "@/stores/loading.store";
import { useMedicationStore } from "@/stores/medication.store";
import { useNoteStore } from "@/stores/note.store";
import { usePatientDataStore } from "@/stores/patientData.store";
import { useSpecialAuthorityRequestStore } from "@/stores/specialAuthorityRequest.store";
import { useTimelineStore } from "@/stores/timeline.store";
import { useUserStore } from "@/stores/user.store";

/**
 * Development builds only: exposes the stores as `globalThis.hgStores` so a feature that dev, test and prod all
 * switch off (e.g. `homepage.showRecommendationsLink`) can be switched on in one browser tab or simulator to check
 * it, the way the Cypress suite does with `cy.configureSettings`, and lets a check read each dataset's load status.
 * Never registered in production builds.
 */
export function registerDevStores() {
    if (!__DEV__) return;
    (globalThis as { hgStores?: unknown }).hgStores = {
        clinicalDocument: useClinicalDocumentStore,
        comment: useCommentStore,
        config: useConfigStore,
        covid19TestResult: useCovid19TestResultStore,
        error: useErrorStore,
        healthVisit: useHealthVisitStore,
        hospitalVisit: useHospitalVisitStore,
        immunization: useImmunizationStore,
        labResult: useLabResultStore,
        loading: useLoadingStore,
        medication: useMedicationStore,
        note: useNoteStore,
        patientData: usePatientDataStore,
        specialAuthorityRequest: useSpecialAuthorityRequestStore,
        timeline: useTimelineStore,
        user: useUserStore,
    };
}
