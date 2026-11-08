import { afterEach, describe, expect, it } from "@jest/globals";

import { ReportFilterBuilder } from "@/models/reportFilter";
import { getReportHeaderData } from "@/stores/report";
import { useUserStore } from "@/stores/user.store";
import { resetStores, signInRegistered, updateStores } from "@/test/fixtures";
import { formatDate, now } from "@/utils/dateUtil";

describe("report header (stores/report.ts getHeaderData port)", () => {
    afterEach(() => resetStores());

    it("uses the patient's PHN, birth date, preferred name and the filter", () => {
        signInRegistered();
        updateStores(() =>
            useUserStore.setState({
                patient: {
                    hdid: "HDID1",
                    personalHealthNumber: "9735353315",
                    birthdate: "1980-02-29",
                    preferredName: { givenName: "Ada", surname: "Lovelace" },
                },
            })
        );
        const header = getReportHeaderData(
            "HDID1",
            ReportFilterBuilder.create().withMedication("X").build()
        );
        expect(header).toEqual({
            phn: "9735353315",
            dateOfBirth: "1980-Feb-29",
            name: "Ada Lovelace",
            isRedacted: true,
            datePrinted: formatDate(now()),
            filterText: "",
        });
    });

    it("leaves out a missing part of the name rather than printing undefined", () => {
        signInRegistered();
        updateStores(() =>
            useUserStore.setState({
                patient: {
                    hdid: "HDID1",
                    personalHealthNumber: "9735353315",
                    birthdate: "1980-02-29",
                    preferredName: { surname: "Lovelace" },
                },
            })
        );
        const header = getReportHeaderData(
            "HDID1",
            ReportFilterBuilder.create().build()
        );
        expect(header.name).toBe("Lovelace");
    });
});
