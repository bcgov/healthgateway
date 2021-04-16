import { ActionTree } from "vuex";

import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import { Dictionary } from "@/models/baseTypes";
import { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import { LaboratoryOrder } from "@/models/laboratory";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import { UserComment } from "@/models/userComment";
import { CommentState } from "@/store/modules/comment/types";
import { ErrorBannerState } from "@/store/modules/error/types";
import { ImmunizationState } from "@/store/modules/immunization/types";
import { LaboratoryState } from "@/store/modules/laboratory/types";
import { MedicationStatementState } from "@/store/modules/medication/modules/statement/types";
import { NavbarState } from "@/store/modules/navbar/types";
import { GatewayStoreOptions, RootState } from "@/store/types";
import userStub from "./user";
import configStub from "./config";

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

const laboratoryActions: ActionTree<LaboratoryState, RootState> = {
    getOrders(): Promise<RequestResult<LaboratoryOrder[]>> {
        return new Promise((resolve) => {
            resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        });
    },
};

const laboratoryGetters = {
    getStoredLaboratoryOrders: () => (): LaboratoryOrder[] => {
        return [];
    },
};

const medicationActions: ActionTree<MedicationStatementState, RootState> = {
    getMedicationStatements(
        context,
        params: {
            hdid: string;
            protectiveWord?: string;
        }
    ): Promise<RequestResult<MedicationStatementHistory[]>> {
        return new Promise((resolve, reject) => {
            if (params.hdid === "hdid_with_results") {
                context.state.medicationStatements = medicationStatements;
                resolve({
                    totalResultCount: medicationStatements.length,
                    pageIndex: 0,
                    pageSize: medicationStatements.length,
                    resultStatus: ResultType.Success,
                    resourcePayload: medicationStatements,
                });
            } else if (params.hdid === "hdid_no_results") {
                resolve({
                    totalResultCount: 0,
                    pageIndex: 0,
                    pageSize: 0,
                    resultStatus: ResultType.Success,
                    resourcePayload: [],
                });
            } else {
                reject({
                    error: "User with " + params.hdid + " not found.",
                });
            }
        });
    },
};

const medicationGetters = {};

const sidebarActions: ActionTree<NavbarState, RootState> = {
    toggleSidebar(context) {
        console.log("toggleSidebar called", context);
    },
    setSidebarState(context, isOpen: boolean) {
        console.log(context, "toggleSidebar called", isOpen);
    },
};

const sidebarGetters = {
    isOpen(state: NavbarState): boolean {
        console.log("isOpen called", state);
        return true;
    },
};

const commentActions: ActionTree<CommentState, RootState> = {
    retrieveProfileComments(): Promise<
        RequestResult<Dictionary<UserComment[]>>
    > {
        return new Promise((resolve) => {
            resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: {},
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        });
    },
};

const immunizationGetters = {
    getStoredImmunizations(state: ImmunizationState): ImmunizationEvent[] {
        console.log("getStoredImmunizations called", state);
        return [];
    },
    isDeferredLoad(state: ImmunizationState): boolean {
        console.log("isDeferredLoad called", state);
        return false;
    },
    getStoredRecommendations(state: ImmunizationState): Recommendation[] {
        console.log("getStoredRecommendations called", state);
        return [];
    },
};

const immunizationActions: ActionTree<ImmunizationState, RootState> = {
    retrieve(): Promise<RequestResult<ImmunizationEvent[]>> {
        return new Promise((resolve) => {
            resolve({
                pageIndex: 0,
                pageSize: 0,
                resourcePayload: [],
                resultStatus: ResultType.Success,
                totalResultCount: 0,
            });
        });
    },
};

const commentGetters = {
    getEntryComments: (state: CommentState) => (
        entryId: string
    ): UserComment[] | undefined => {
        console.log(state, "getEntryComments called", entryId);
        return [];
    },
};

const configGetters = {
    webClient: (): WebClientConfiguration => {
        return a;
    },
};

const errorBannerGetters = {
    isShowing(state: ErrorBannerState): boolean {
        return state.isShowing;
    },
    errors(state: ErrorBannerState): BannerError[] {
        return state.errors;
    },
};

const errorBannerActions: ActionTree<ErrorBannerState, RootState> = {
    dismiss(context) {
        console.log(context, "dismiss Called");
    },
    show(context) {
        console.log(context, "show Called");
    },
    setError(context, error: BannerError) {
        console.log(context, "setError Called", error);
    },
    addError(context, error: BannerError) {
        console.log(context, "addError Called", error);
    },
};

export const storeOptions: GatewayStoreOptions = {
    modules: {
        user: userStub,
        config: configStub,
        laboratory: laboratoryStub,
        medication: medicationStub,
        immunization: immunizationStub,
        comment: commentStub,
        sidebar: sidebarStub,
        errorBanner: errorStub,
    },
};
