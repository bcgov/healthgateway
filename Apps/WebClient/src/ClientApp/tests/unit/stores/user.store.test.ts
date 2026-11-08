import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { AppErrorType } from "@/constants/errorType";
import { UserPreferenceType } from "@/constants/userPreferenceType";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import {
    selectHasTermsOfServiceUpdated,
    selectIsValidIdentityProvider,
    selectLastLoginDateTime,
    selectQuickLinksJson,
    selectUserInitials,
    selectUserIsActive,
    selectUserIsRegistered,
    selectUserName,
    useUserStore,
} from "@/stores/user.store";
import {
    bcscUserInfo,
    configFixture,
    fakeAuth,
    jsonResponse,
    makeProfile,
    resetStores,
    tokenDetails,
} from "@/test/fixtures";

type FetchMock = jest.Mock<typeof fetch>;
const fetchMock = () => global.fetch as unknown as FetchMock;

describe("user store selectors", () => {
    afterEach(resetStores);

    it("accepts BCSC or an unspecified identity provider only", () => {
        const s = useUserStore.getState();
        expect(selectIsValidIdentityProvider(useUserStore.getState())).toBe(
            false
        );
        s.setOidcUserInfo({ ...bcscUserInfo, idp: undefined });
        expect(selectIsValidIdentityProvider(useUserStore.getState())).toBe(
            true
        );
        s.setOidcUserInfo({ ...bcscUserInfo, idp: "idir" });
        expect(selectIsValidIdentityProvider(useUserStore.getState())).toBe(
            false
        );
    });

    it("derives initials and the display name from the userinfo claims", () => {
        const s = useUserStore.getState();
        expect(selectUserInitials(useUserStore.getState())).toBe("?");
        s.setOidcUserInfo(bcscUserInfo);
        expect(selectUserInitials(useUserStore.getState())).toBe("DG");
        expect(selectUserName(useUserStore.getState())).toBe("Dr Gateway");
        s.setOidcUserInfo({ ...bcscUserInfo, family_name: undefined });
        expect(selectUserInitials(useUserStore.getState())).toBe("D");
    });

    it("maps the profile onto the user: registration, activity, terms and the previous login", () => {
        const s = useUserStore.getState();
        s.setOidcUserInfo(bcscUserInfo);
        s.setProfile(makeProfile());
        const state = useUserStore.getState();
        expect(selectUserIsRegistered(state)).toBe(true);
        expect(selectUserIsActive(state)).toBe(true);
        expect(selectHasTermsOfServiceUpdated(state)).toBe(false);
        expect(selectLastLoginDateTime(state)).toBe("2026-10-28T10:00:00Z");
        expect(state.user.email).toBe("dr@example.test");
        expect(state.user.verifiedEmail).toBe(true);
        expect(state.user.sms).toBe("2505551234");
        expect(state.user.verifiedSms).toBe(false);
    });

    it("exposes the quick-links preference as a stable JSON string", () => {
        const s = useUserStore.getState();
        s.setProfile(
            makeProfile({
                preferences: {
                    [UserPreferenceType.QuickLinks]: {
                        preference: UserPreferenceType.QuickLinks,
                        value: '[{"name":"Immunizations","filter":{"modules":["Immunization"]}}]',
                        version: 1,
                        createdDateTime: "2026-10-01T00:00:00Z",
                    },
                },
            })
        );
        const a = selectQuickLinksJson(useUserStore.getState());
        const b = selectQuickLinksJson(useUserStore.getState());
        expect(a).toBe(b);
        expect((JSON.parse(a!) as { name: string }[])[0].name).toBe(
            "Immunizations"
        );
    });
});

describe("user store essential data (patient + profile over fetch)", () => {
    beforeEach(() => {
        global.fetch = jest.fn<typeof fetch>();
        services.initialize(configFixture, fakeAuth(tokenDetails));
        useUserStore.getState().setOidcUserInfo(bcscUserInfo);
    });
    afterEach(() => {
        jest.restoreAllMocks();
        resetStores();
    });

    it("loads the patient then the login profile and flags essential data as loaded", async () => {
        fetchMock()
            .mockResolvedValueOnce(
                jsonResponse({ hdid: "HDID1", firstname: "Dr" })
            )
            .mockResolvedValueOnce(jsonResponse(makeProfile()));
        await useUserStore.getState().retrieveEssentialData(true);
        const urls = fetchMock().mock.calls.map((c) => c[0] as string);
        expect(urls[0]).toBe(
            "https://api.test/patient/Patient/HDID1?api-version=2.0"
        );
        expect(urls[1]).toBe(
            "https://api.test/gateway/UserProfile/HDID1/login?api-version=2.0"
        );
        const state = useUserStore.getState();
        expect(state.patientRetrievalFailed).toBe(false);
        expect(state.essentialDataLoaded).toBe(true);
        expect(state.isLoading).toBe(false);
        expect(selectUserIsRegistered(state)).toBe(true);
    });

    it("marks patient retrieval failed and raises the app error on a 429", async () => {
        fetchMock().mockResolvedValueOnce(
            jsonResponse({ detail: "Too many" }, 429)
        );
        await useUserStore.getState().retrieveEssentialData();
        expect(useUserStore.getState().patientRetrievalFailed).toBe(true);
        expect(useErrorStore.getState().appError).toBe(
            AppErrorType.TooManyRequests
        );
        expect(useUserStore.getState().essentialDataLoaded).toBe(true);
    });

    it("adds a retrieve banner when the profile fails after the patient loaded", async () => {
        fetchMock()
            .mockResolvedValueOnce(jsonResponse({ hdid: "HDID1" }))
            .mockResolvedValueOnce(
                jsonResponse({ detail: "Boom", traceId: "t-1" }, 500)
            );
        await useUserStore.getState().retrieveEssentialData();
        expect(useUserStore.getState().patientRetrievalFailed).toBe(true);
        expect(useErrorStore.getState().errors[0]).toMatchObject({
            title: "Unable to retrieve users",
            source: "user",
            traceId: "t-1",
        });
    });

    it("creates the quick-links preference on first save and updates it afterwards", async () => {
        useUserStore.getState().setProfile(makeProfile());
        const created = {
            hdId: "HDID1",
            preference: UserPreferenceType.QuickLinks,
            value: "[]",
            version: 1,
            createdDateTime: "x",
        };
        fetchMock()
            .mockResolvedValueOnce(jsonResponse(created))
            .mockResolvedValueOnce(jsonResponse({ ...created, version: 2 }));
        await useUserStore.getState().updateQuickLinks([]);
        await useUserStore
            .getState()
            .updateQuickLinks([
                { name: "Labs", filter: { modules: ["Laboratory"] } },
            ]);
        const calls = fetchMock().mock.calls.map((c) => [
            (c[1] as RequestInit).method,
            c[0] as string,
        ]);
        expect(calls).toEqual([
            [
                "POST",
                "https://api.test/gateway/UserProfile/HDID1/preference?api-version=2.0",
            ],
            [
                "PUT",
                "https://api.test/gateway/UserProfile/HDID1/preference?api-version=2.0",
            ],
        ]);
        expect(
            useUserStore.getState().user.preferences[
                UserPreferenceType.QuickLinks
            ].version
        ).toBe(2);
    });

    it("gives a profile's unsaved hide-quick-link preferences a name to be saved under", async () => {
        const create = jest
            .spyOn(services.userProfile, "createUserPreference")
            .mockImplementation(async (_hdid, preference) => preference);
        useUserStore.getState().setProfile(makeProfile({ preferences: {} }));
        const preference =
            useUserStore.getState().user.preferences[
                UserPreferenceType.HideOrganDonorQuickLink
            ];
        expect(preference).toMatchObject({
            preference: UserPreferenceType.HideOrganDonorQuickLink,
            value: "false",
            version: 0,
        });
        await useUserStore
            .getState()
            .setUserPreference({ ...preference, value: "true" });
        expect(create).toHaveBeenCalledWith(
            expect.any(String),
            expect.objectContaining({
                preference: UserPreferenceType.HideOrganDonorQuickLink,
                value: "true",
            })
        );
    });
});
