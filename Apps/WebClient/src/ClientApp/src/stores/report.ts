import { defineStore } from "pinia";

import { DateWrapper } from "@/models/dateWrapper";
import ReportFilter from "@/models/reportFilter";
import { useDependentStore } from "@/stores/dependent";
import { useUserStore } from "@/stores/user";

export const useReportStore = defineStore("reports", () => {
    const dependentsStore = useDependentStore();
    const userStore = useUserStore();

    function getHeaderData(hdid: string, reportFilter: ReportFilter) {
        const dependent = dependentsStore.dependents.find(
            (d) => d.dependentInformation.hdid === hdid
        );
        if (dependent) {
            return {
                phn: dependent.dependentInformation.PHN,
                dateOfBirth: DateWrapper.format(
                    dependent.dependentInformation.dateOfBirth ?? ""
                ),
                name: dependent.dependentInformation
                    ? dependent.dependentInformation.firstname +
                      " " +
                      dependent.dependentInformation.lastname
                    : "",
                isRedacted: reportFilter.hasMedicationsFilter(),
                datePrinted: new DateWrapper(
                    new DateWrapper().toISO()
                ).format(),
                filterText: reportFilter.filterText,
            };
        } else {
            return {
                phn: userStore.patient.personalHealthNumber,
                dateOfBirth: DateWrapper.format(
                    userStore.patient.birthdate ?? ""
                ),
                name: userStore.patient
                    ? userStore.patient.preferredName.givenName +
                      " " +
                      userStore.patient.preferredName.surname
                    : "",
                isRedacted: reportFilter.hasMedicationsFilter(),
                datePrinted: new DateWrapper(
                    new DateWrapper().toISO()
                ).format(),
                filterText: reportFilter.filterText,
            };
        }
    }

    return {
        getHeaderData,
    };
});
