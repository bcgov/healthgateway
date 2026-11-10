import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";

import { getAccessLinks } from "@/constants/accessLinks";
import { onboardingScreens } from "@/constants/onboarding";
import { menuItems } from "@/navigation/menu";
import {
    selectOnboardingPending,
    useOnboardingStore,
} from "@/stores/onboarding.store";

const SEEN = "hg.onboarding.seen";
const OFFERED = "hg.onboarding.securityOffered";
const store = () => useOnboardingStore.getState();
const types = () => store().screens.map((s) => s.type);
const seenAll = onboardingScreens.map(({ type, version }) => ({
    type,
    version,
}));

describe("onboarding store", () => {
    beforeEach(async () => {
        await AsyncStorage.clear();
        useOnboardingStore.setState({ screens: [], securityOffered: true });
    });

    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("shows every screen and the security step on first launch", async () => {
        await store().hydrate(false);
        expect(types()).toEqual(onboardingScreens.map((s) => s.type));
        expect(store().securityOffered).toBe(false);
        expect(selectOnboardingPending(store())).toBe(true);
    });

    it("has nothing pending once both were completed", async () => {
        await store().hydrate(false);
        await store().completeTutorial();
        await store().completeSecurityOffer();
        expect(selectOnboardingPending(store())).toBe(false);

        useOnboardingStore.setState({ screens: [], securityOffered: false });
        await store().hydrate(false);
        expect(types()).toEqual([]);
        expect(selectOnboardingPending(store())).toBe(false);
    });

    it("asks for the security step next launch when the app closed before it was answered", async () => {
        await store().hydrate(false);
        await store().completeTutorial();

        await store().hydrate(false);
        expect(types()).toEqual([]);
        expect(selectOnboardingPending(store())).toBe(true);
    });

    it("shows only a screen whose version moved on", async () => {
        await AsyncStorage.setItem(
            SEEN,
            JSON.stringify(
                seenAll.map((s) =>
                    s.type === "HealthServices" ? { ...s, version: 2 } : s
                )
            )
        );
        await AsyncStorage.setItem(OFFERED, "true");
        await store().hydrate(false);
        expect(types()).toEqual(["HealthServices"]);
    });

    it("keeps earlier visits when saving new ones", async () => {
        await AsyncStorage.setItem(SEEN, JSON.stringify([seenAll[0]]));
        await store().hydrate(false);
        await store().completeTutorial();
        const saved = JSON.parse(
            (await AsyncStorage.getItem(SEEN)) ?? "[]"
        ) as unknown;
        expect(saved).toEqual(expect.arrayContaining(seenAll));
    });

    it("does not offer the lock to someone who already has it on", async () => {
        await store().hydrate(true);
        expect(store().securityOffered).toBe(true);
    });

    it("treats a corrupt record as nothing seen", async () => {
        await AsyncStorage.setItem(SEEN, "{not json");
        await store().hydrate(false);
        expect(store().screens).toHaveLength(onboardingScreens.length);
        await AsyncStorage.setItem(SEEN, '"a string"');
        await store().hydrate(false);
        expect(store().screens).toHaveLength(onboardingScreens.length);
    });

    it("never holds the app back when storage fails", async () => {
        jest.spyOn(AsyncStorage, "getItem")
            .mockRejectedValueOnce(new Error("locked"))
            .mockRejectedValueOnce(new Error("locked"));
        await store().hydrate(false);
        expect(selectOnboardingPending(store())).toBe(false);
    });

    it("still finishes the steps when they cannot be saved", async () => {
        await store().hydrate(false);
        jest.spyOn(AsyncStorage, "setItem")
            .mockRejectedValueOnce(new Error("full"))
            .mockRejectedValueOnce(new Error("full"));
        await store().completeTutorial();
        await store().completeSecurityOffer();
        expect(selectOnboardingPending(store())).toBe(false);
    });

    it("has one screen per landing page tile, using its text", () => {
        expect(
            onboardingScreens.map(({ type, title, description }) => ({
                type,
                name: title,
                description,
            }))
        ).toEqual(
            getAccessLinks().map(({ type, name, description }) => ({
                type,
                name,
                description,
            }))
        );
        expect(onboardingScreens).toHaveLength(4);
    });

    it("follows the order of the main navigation", () => {
        expect(onboardingScreens.map((s) => s.type)).toEqual([
            "HealthRecords",
            "DependentRecords",
            "HealthServices",
            "RecordsManagement",
        ]);
        expect(
            menuItems
                .map((m) => m.title)
                .filter((t) =>
                    [
                        "Health Records",
                        "Dependents",
                        "Services",
                        "Download",
                    ].includes(t)
                )
        ).toEqual(["Health Records", "Dependents", "Services", "Download"]);
    });

    it("shows the intro again to someone who saw the old screens", async () => {
        await AsyncStorage.setItem(
            SEEN,
            JSON.stringify([{ type: "healthRecords", version: 3 }])
        );
        await store().hydrate(false);
        expect(store().screens).toHaveLength(onboardingScreens.length);
    });
});
