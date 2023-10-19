import { defineStore } from "pinia";

import { DateWrapper } from "@/models/dateWrapper";
import { IReportFilter } from "@/models/reportFilter";
import { useDependentStore } from "@/stores/dependent";
import { useUserStore } from "@/stores/user";

export const useReportStore = defineStore("reports", () => {
    const dependentsStore = useDependentStore();
    const userStore = useUserStore();

    function getHeaderData(hdid: string, reportFilter: IReportFilter) {
        const dependent = dependentsStore.dependents.find(
            (d) => d.dependentInformation.hdid === hdid
        );
        if (dependent) {
            return {
                phn: dependent.dependentInformation.PHN,
                dateOfBirth: dependent.dependentInformation.dateOfBirth
                    ? DateWrapper.fromIsoDate(
                          dependent.dependentInformation.dateOfBirth
                      ).format()
                    : "",
                name: dependent.dependentInformation
                    ? dependent.dependentInformation.firstname +
                      " " +
                      dependent.dependentInformation.lastname
                    : "",
                isRedacted: reportFilter.hasMedicationsFilter(),
                datePrinted: DateWrapper.now().format(),
                filterText: reportFilter.filterText,
            };
        } else {
            return {
                phn: userStore.patient.personalHealthNumber,
                dateOfBirth: userStore.patient.birthdate
                    ? DateWrapper.fromIsoDate(
                          userStore.patient.birthdate
                      ).format()
                    : "",
                name: userStore.patient
                    ? userStore.patient.preferredName.givenName +
                      " " +
                      userStore.patient.preferredName.surname
                    : "",
                isRedacted: reportFilter.hasMedicationsFilter(),
                datePrinted: DateWrapper.now().format(),
                filterText: reportFilter.filterText,
            };
        }
    }

    return {
        getHeaderData,
    };
});
