import type { ReportHeader } from "@/models/report";
import type { IReportFilter } from "@/models/reportFilter";
import { formatDate, fromIsoDate, now } from "@/utils/dateUtil";

import { useDependentStore } from "./dependent.store";
import { useUserStore } from "./user.store";

/** Port of stores/report.ts getHeaderData: the dependent's details for a dependent's hdid, else the user's. */
export function getReportHeaderData(
    hdid: string,
    reportFilter: IReportFilter
): ReportHeader {
    const dependent = useDependentStore
        .getState()
        .dependents.find((d) => d.dependentInformation.hdid === hdid);
    if (dependent) {
        const info = dependent.dependentInformation;
        return {
            phn: info.PHN,
            dateOfBirth: formatDate(fromIsoDate(info.dateOfBirth)),
            name: `${info.firstname} ${info.lastname}`,
            isRedacted: reportFilter.hasMedicationsFilter(),
            datePrinted: formatDate(now()),
            filterText: reportFilter.filterText,
        };
    }
    const patient = useUserStore.getState().patient;
    return {
        phn: patient?.personalHealthNumber ?? "",
        dateOfBirth: formatDate(fromIsoDate(patient?.birthdate)),
        name: patient
            ? `${patient.preferredName?.givenName ?? ""} ${patient.preferredName?.surname ?? ""}`.trim()
            : "",
        isRedacted: reportFilter.hasMedicationsFilter(),
        datePrinted: formatDate(now()),
        filterText: reportFilter.filterText,
    };
}
