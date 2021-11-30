import { voidMethod, voidPromise } from "@test/stubs/util";

import BannerError from "@/models/bannerError";
import {
    LaboratoryOrder,
    PublicCovidTestResponseResult,
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
    public: {
        publicCovidTestResponseResult: undefined,
        error: undefined,
        status: LoadStatus.NONE,
        statusMessage: "",
    },
    authenticated: {
        laboratoryOrders: [],
        statusMessage: "",
        status: LoadStatus.NONE,
    },
};

const laboratoryGetters: LaboratoryGetters = {
    laboratoryOrders(): LaboratoryOrder[] {
        return [];
    },
    laboratoryCount(): number {
        return 0;
    },
    isLoading(): boolean {
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
    retrieve: voidPromise,
    handleError: voidMethod,
    retrievePublicCovidTests: voidPromise,
    handlePublicCovidTestsError: voidMethod,
    resetPublicCovidTestResponseResult: voidMethod,
};

const laboratoryMutations: LaboratoryMutations = {
    setRequested: voidMethod,
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
