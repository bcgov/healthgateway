import { create } from "zustand";

/** Port of constants/loader.ts */
export enum Loader {
    AddDependent = "AddDependent",
    UserProfile = "UserProfile",
    ValidateEmail = "ValidateEmail",
    NoteDialog = "NoteDialog",
}

interface LoadingState {
    /** Active loading requests keyed by caller id; the overlay shows while any exist. */
    requests: Record<string, string | undefined>;
    show: (id: string, text?: string) => void;
    hide: (id: string) => void;
    /** Keyed loaders (web client loading store): reasons per loader; a loader is active while any remain. */
    loaders: Partial<Record<Loader, string[]>>;
    setIsLoading: (value: boolean, loader: Loader, reason?: string) => void;
    /** Marks `loader` busy for `reason` until `promise` settles; returns the same promise. */
    applyLoader: <T>(
        loader: Loader,
        reason: string,
        promise: Promise<T>
    ) => Promise<T>;
}

export const useLoadingStore = create<LoadingState>((set, get) => ({
    requests: {},
    show: (id, text) =>
        set((s) => ({ requests: { ...s.requests, [id]: text } })),
    hide: (id) =>
        set((s) => {
            if (!(id in s.requests)) return s;
            const requests = { ...s.requests };
            delete requests[id];
            return { requests };
        }),
    loaders: {},
    setIsLoading: (value, loader, reason = "default") =>
        set((s) => {
            const current = s.loaders[loader] ?? [];
            const next = value
                ? current.includes(reason)
                    ? current
                    : [...current, reason]
                : current.filter((r) => r !== reason);
            const loaders = { ...s.loaders };
            if (next.length > 0) loaders[loader] = next;
            else delete loaders[loader];
            return { loaders };
        }),
    applyLoader: (loader, reason, promise) => {
        get().setIsLoading(true, loader, reason);
        const clear = () => get().setIsLoading(false, loader, reason);
        void promise.then(clear, clear);
        return promise;
    },
}));

export const selectLoaderActive = (loader: Loader) => (s: LoadingState) =>
    (s.loaders[loader]?.length ?? 0) > 0;

export const selectIsLoading = (s: LoadingState) =>
    Object.keys(s.requests).length > 0;
/** The most recently requested text, like the single v-overlay on the web client. */
export const selectLoadingText = (s: LoadingState) => {
    const texts = Object.values(s.requests);
    return texts[texts.length - 1];
};
