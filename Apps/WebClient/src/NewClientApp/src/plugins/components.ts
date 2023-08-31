import type { App } from "vue";

import CancerScreeningResultTimelineComponent from "@/components/private/timeline/entry/CancerScreeningResultTimelineComponent.vue";
import ClinicalDocumentTimelineComponent from "@/components/private/timeline/entry/ClinicalDocumentTimelineComponent.vue";
import Covid19TestResultTimelineComponent from "@/components/private/timeline/entry/Covid19TestResultTimelineComponent.vue";
import DiagnosticImagingTimelineComponent from "@/components/private/timeline/entry/DiagnosticImagingTimelineComponent.vue";
import HealthVisitTimelineComponent from "@/components/private/timeline/entry/HealthVisitTimelineComponent.vue";
import HospitalVisitTimelineComponent from "@/components/private/timeline/entry/HospitalVisitTimelineComponent.vue";
import ImmunizationTimelineComponent from "@/components/private/timeline/entry/ImmunizationTimelineComponent.vue";
import LabResultTimelineComponent from "@/components/private/timeline/entry/LabResultTimelineComponent.vue";
import MedicationTimelineComponent from "@/components/private/timeline/entry/MedicationTimelineComponent.vue";
import NoteTimelineComponent from "@/components/private/timeline/entry/NoteTimelineComponent.vue";
import SpecialAuthorityRequestTimelineComponent from "@/components/private/timeline/entry/SpecialAuthorityRequestTimelineComponent.vue";

export function registerGlobalComponents(app: App) {
    app.component(
        "ClinicalDocumentTimelineComponent",
        ClinicalDocumentTimelineComponent
    )
        .component(
            "Covid19TestResultTimelineComponent",
            Covid19TestResultTimelineComponent
        )
        .component(
            "DiagnosticImagingTimelineComponent",
            DiagnosticImagingTimelineComponent
        )
        .component("HealthVisitTimelineComponent", HealthVisitTimelineComponent)
        .component(
            "HospitalVisitTimelineComponent",
            HospitalVisitTimelineComponent
        )
        .component(
            "ImmunizationTimelineComponent",
            ImmunizationTimelineComponent
        )
        .component("LabResultTimelineComponent", LabResultTimelineComponent)
        .component("MedicationTimelineComponent", MedicationTimelineComponent)
        .component("NoteTimelineComponent", NoteTimelineComponent)
        .component(
            "SpecialAuthorityRequestTimelineComponent",
            SpecialAuthorityRequestTimelineComponent
        )
        .component(
            "CancerScreeningResultTimelineComponent",
            CancerScreeningResultTimelineComponent
        );
}
