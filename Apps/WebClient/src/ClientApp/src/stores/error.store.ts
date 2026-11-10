import { create } from "zustand";

import {
    AppErrorType,
    ErrorSourceType,
    ErrorType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { isSessionEnded, ResultError } from "@/models/errors";
import {
    type BannerError,
    toBannerError,
    toCustomBannerError,
} from "@/services/errorTranslator";
import { describeError, logger } from "@/services/logger";

import {
    selectCanFetch,
    selectIsUnreachable,
    useConnectivityStore,
} from "./connectivity.store";

export type { BannerError };

interface ErrorState {
    errors: BannerError[];
    /** location key of the "too many requests" warning / error (TooManyRequestsComponent `location`) */
    tooManyRequestsWarning?: string;
    tooManyRequestsError?: string;
    appError?: AppErrorType;

    addError: (
        type: ErrorType,
        source: ErrorSourceType,
        traceId?: string
    ) => void;
    addCustomError: (
        title: string,
        source: ErrorSourceType,
        traceId?: string
    ) => void;
    setTooManyRequestsError: (location?: string) => void;
    setTooManyRequestsWarning: (location?: string) => void;
    clearTooManyRequestsError: () => void;
    clearTooManyRequestsWarning: () => void;
    setAppError: (type: AppErrorType | undefined) => void;
    /** Clears the error banner (not the too-many-requests messages), like the web client. */
    clearErrors: () => void;
    handleResultError: (
        error: unknown,
        type: ErrorType,
        source: ErrorSourceType
    ) => void;
}

/** Offline, service unreachable or login on hold: failures are expected, and the session banner explains them. */
const bannerSaysWhy = () => {
    const connectivity = useConnectivityStore.getState();
    return !selectCanFetch(connectivity) || selectIsUnreachable(connectivity);
};

/** The first background sync: nobody asked for these records yet. Opening the page tries again, and says so if it fails. */
const syncing = () => useConnectivityStore.getState().syncing;

/** Port of stores/error.ts */
export const useErrorStore = create<ErrorState>((set, get) => ({
    errors: [],
    tooManyRequestsWarning: undefined,
    tooManyRequestsError: undefined,
    appError: undefined,

    addError: (type, source, traceId) => {
        if (bannerSaysWhy() || (syncing() && type === ErrorType.Retrieve))
            return;
        set((s) => ({
            errors: [...s.errors, toBannerError(type, source, traceId)],
        }));
    },
    addCustomError: (title, source, traceId) => {
        if (bannerSaysWhy()) return;
        set((s) => ({
            errors: [...s.errors, toCustomBannerError(title, source, traceId)],
        }));
    },
    setTooManyRequestsError: (location = TooManyRequestsLocation.Page) =>
        set({ tooManyRequestsError: location }),
    setTooManyRequestsWarning: (location = TooManyRequestsLocation.Page) => {
        if (!syncing()) set({ tooManyRequestsWarning: location });
    },
    clearTooManyRequestsError: () => set({ tooManyRequestsError: undefined }),
    clearTooManyRequestsWarning: () =>
        set({ tooManyRequestsWarning: undefined }),
    setAppError: (type) => set({ appError: type }),
    clearErrors: () => set({ errors: [] }),
    handleResultError: (error, type, source) => {
        if (isSessionEnded(error)) return;
        logger.error(`Error: ${describeError(error)}`);
        if (error instanceof ResultError && error.statusCode === 429) {
            if (type === ErrorType.Retrieve) get().setTooManyRequestsWarning();
            else get().setTooManyRequestsError();
        } else {
            get().addError(
                type,
                source,
                error instanceof ResultError ? error.traceId : undefined
            );
        }
    },
}));
