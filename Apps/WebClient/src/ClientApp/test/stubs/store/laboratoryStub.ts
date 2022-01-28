import { voidMethod, voidPromise } from "@test/stubs/util";

import BannerError from "@/models/bannerError";
import {
    Covid19LaboratoryOrder,
    PublicCovidTestResponseResult,
    LaboratoryOrder,
} from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";
import {
    LaboratoryActions,
    LaboratoryGetters,
    LaboratoryModule,
    LaboratoryMutations,
    LaboratoryState,
} from "@/store/modules/laboratory/types";

const laboratoryState: LaboratoryState = {
    publicCovid19: {
        publicCovidTestResponseResult: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },
    authenticatedCovid19: {
        laboratoryOrders: [],
        statusMessage: "",
        status: LoadStatus.NONE,
    },
    authenticated: {
        laboratoryOrders: [],
        statusMessage: "",
        status: LoadStatus.NONE,
    },
};

const laboratoryGetters: LaboratoryGetters = {
    covid19LaboratoryOrders(): Covid19LaboratoryOrder[] {
        return [];
    },
    covid19LaboratoryOrdersCount(): number {
        return 0;
    },
    covid19LaboratoryOrdersAreLoading(): boolean {
        return false;
    },
    laboratoryOrders(): LaboratoryOrder[] {
        return [];
    },
    laboratoryOrdersCount(): number {
        return 0;
    },
    laboratoryOrdersAreLoading(): boolean {
        return false;
    },
    publicCovidTestResponseResult(): PublicCovidTestResponseResult | undefined {
        return undefined;
    },
    isPublicCovidTestResponseResultLoading(): boolean {
        return false;
    },
    publicCovidTestResponseResultError(): BannerError | undefined {
        return undefined;
    },
    publicCovidTestResponseResultStatusMessage(): string {
        return "";
    },
};

const laboratoryActions: LaboratoryActions = {
    retrieveCovid19LaboratoryOrders: voidPromise,
    handleCovid19LaboratoryError: voidMethod,
    retrieveLaboratoryOrders: voidPromise,
    handleLaboratoryError: voidMethod,
    retrievePublicCovidTests: voidPromise,
    handlePublicCovidTestsError: voidMethod,
    resetPublicCovidTestResponseResult: voidMethod,
};

const laboratoryMutations: LaboratoryMutations = {
    setCovid19LaboratoryOrdersRequested: voidMethod,
    setCovid19LaboratoryOrders: voidMethod,
    covid19LaboratoryError: voidMethod,
    setLaboratoryOrdersRequested: voidMethod,
    setLaboratoryOrders: voidMethod,
    laboratoryError: voidMethod,
    setPublicCovidTestResponseResultRequested: voidMethod,
    setPublicCovidTestResponseResult: voidMethod,
    setPublicCovidTestResponseResultError: voidMethod,
    setPublicCovidTestResponseResultStatusMessage: voidMethod,
    resetPublicCovidTestResponseResult: voidMethod,
};

const laboratoryStub: LaboratoryModule = {
    namespaced: true,
    state: laboratoryState,
    getters: laboratoryGetters,
    actions: laboratoryActions,
    mutations: laboratoryMutations,
};

export default laboratoryStub;
