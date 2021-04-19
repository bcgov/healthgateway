import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import { GatewayStoreOptions } from "@/store/types";
import userStub from "./user";
import configStub from "./config";
import laboratoryStub from "./laboratory";
import medicationStub from "./medication";
import immunizationStub from "./immunization";
import commentStub from "./comment";
import navbarStub from "./navbar";
import encounterStub from "./encounter";
import authStub from "./auth";

const today = new DateWrapper();
const yesterday = today.subtract({ day: 1 });

const medicationStatements: MedicationStatementHistory[] = [
    {
        medicationSummary: {
            din: "1233",
            brandName: "brand_name_A",
            genericName: "generic_name_A",
            isPin: false,
        },
        prescriptionIdentifier: "abcmed1",
        dispensedDate: today.toISODate(),
        dispensingPharmacy: {},
    },
    {
        medicationSummary: {
            din: "1234",
            brandName: "brand_name_B",
            genericName: "generic_name_B",
            isPin: false,
        },
        prescriptionIdentifier: "abcmed2",
        dispensedDate: today.toISODate(),
        dispensingPharmacy: {},
    },
    {
        medicationSummary: {
            din: "1235",
            brandName: "brand_name_C",
            genericName: "generic_name_C",
            isPin: true,
        },
        prescriptionIdentifier: "abcmed3",
        dispensedDate: yesterday.toISODate(),
        dispensingPharmacy: {},
    },
];

export const storeOptions: GatewayStoreOptions = {
    modules: {
        auth: authStub,
        config: configStub,
        user: userStub,
        medication: medicationStub,
        laboratory: laboratoryStub,
        comment: commentStub,
        immunization: immunizationStub,
        encounter: encounterStub,
        note: NoteModule,
        navbar: navbarStub,
        idle: IdleModule,
        errorBanner: ErrorBannerModule,
        timeline: TimelineModule,
    },
};
