import { beforeEach, describe, expect, it } from "@jest/globals";

import {
    Loader,
    selectIsLoading,
    selectLoaderActive,
    selectLoadingText,
    useLoadingStore,
} from "@/stores/loading.store";

describe("loading store", () => {
    beforeEach(() => useLoadingStore.setState({ requests: {} }));

    it("is idle with no requests", () => {
        expect(selectIsLoading(useLoadingStore.getState())).toBe(false);
        expect(selectLoadingText(useLoadingStore.getState())).toBeUndefined();
    });

    it("shows while any request is active and uses the latest text", () => {
        const { show, hide } = useLoadingStore.getState();
        show("a");
        show("b", "Logging In...");
        expect(selectIsLoading(useLoadingStore.getState())).toBe(true);
        expect(selectLoadingText(useLoadingStore.getState())).toBe(
            "Logging In..."
        );
        hide("b");
        expect(selectIsLoading(useLoadingStore.getState())).toBe(true);
        expect(selectLoadingText(useLoadingStore.getState())).toBeUndefined();
        hide("a");
        expect(selectIsLoading(useLoadingStore.getState())).toBe(false);
    });

    it("ignores hiding an unknown id", () => {
        const before = useLoadingStore.getState();
        useLoadingStore.getState().hide("nope");
        expect(useLoadingStore.getState()).toBe(before);
    });

    it("keyed loaders stay active until every reason has finished", async () => {
        const { applyLoader, setIsLoading } = useLoadingStore.getState();
        const active = () =>
            selectLoaderActive(Loader.UserProfile)(useLoadingStore.getState());
        let finishA: () => void = () => undefined;
        const a = new Promise<void>((r) => (finishA = r));
        const b = Promise.reject(new Error("x"));
        void applyLoader(Loader.UserProfile, "a", a);
        void applyLoader(Loader.UserProfile, "b", b).catch(() => undefined);
        expect(active()).toBe(true);
        await Promise.resolve();
        await Promise.resolve();
        expect(active()).toBe(true); // "a" still running after "b" failed
        finishA();
        await a;
        await Promise.resolve();
        expect(active()).toBe(false);
        setIsLoading(true, Loader.UserProfile);
        setIsLoading(true, Loader.UserProfile);
        setIsLoading(false, Loader.UserProfile);
        expect(active()).toBe(false);
    });
});
