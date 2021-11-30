import BannerError from "@/models/bannerError";
import {
    LaboratoryOrder,
    PublicCovidTestResponseResult,
} from "@/models/laboratory";
import { LoadStatus } from "@/models/storeOperations";

import { LaboratoryGetters, LaboratoryState } from "./types";

export const getters: LaboratoryGetters = {
    laboratoryOrders(state: LaboratoryState): LaboratoryOrder[] {
        return state.authenticated.laboratoryOrders;
    },
    laboratoryCount(state: LaboratoryState): number {
        return state.authenticated.laboratoryOrders.length;
    },
    isLoading(state: LaboratoryState): boolean {
        return state.authenticated.status === LoadStatus.REQUESTED;
    },
    publicCovidTestResponseResult(
        state: LaboratoryState
    ): PublicCovidTestResponseResult | undefined {
        return state.public.publicCovidTestResponseResult;
    },
    isPublicCovidTestResponseResultLoading(state: LaboratoryState): boolean {
        return state.public.status === LoadStatus.REQUESTED;
    },
    publicCovidTestResponseResultError(
        state: LaboratoryState
    ): BannerError | undefined {
        return state.public.error;
    },
    publicCovidTestResponseResultStatusMessage(state: LaboratoryState): string {
        return state.public.statusMessage;
    },
};
