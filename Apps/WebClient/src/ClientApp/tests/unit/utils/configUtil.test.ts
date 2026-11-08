import { afterEach, describe, expect, it } from "@jest/globals";

import { EntryType, ServiceName } from "@/constants/entryType";
import {
    ProfileNotificationPreference,
    ProfileNotificationType,
} from "@/constants/profileNotifications";
import { useConfigStore } from "@/stores/config.store";
import { configFixture, resetStores } from "@/test/fixtures";
import { ConfigUtil } from "@/utils/configUtil";

describe("ConfigUtil (utility/configUtil.ts port)", () => {
    afterEach(resetStores);

    it("reports everything off without a configuration", () => {
        expect(ConfigUtil.isDatasetEnabled(EntryType.Immunization)).toBe(false);
        expect(
            ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration)
        ).toBe(false);
        expect(ConfigUtil.isOtherRecordSourcesFeatureEnabled()).toBe(false);
        expect(ConfigUtil.isOtherRecordSourcesCardEnabled("Fraser")).toBe(
            false
        );
        expect(ConfigUtil.isImmunizationRecordLinkEnabled()).toBe(false);
        expect(ConfigUtil.showRecommendationsLink()).toBe(false);
    });

    it("reads datasets, services, cards and home links case-insensitively", () => {
        useConfigStore.setState({ config: configFixture });
        expect(ConfigUtil.isDatasetEnabled(EntryType.Immunization)).toBe(true);
        expect(ConfigUtil.isDatasetEnabled(EntryType.Medication)).toBe(false);
        expect(
            ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration)
        ).toBe(true);
        expect(
            ConfigUtil.isServiceEnabled(ServiceName.HealthConnectRegistry)
        ).toBe(false);
        expect(ConfigUtil.isOtherRecordSourcesFeatureEnabled()).toBe(true);
        expect(ConfigUtil.isOtherRecordSourcesCardEnabled("fraser")).toBe(true);
        expect(ConfigUtil.isOtherRecordSourcesCardEnabled("interior")).toBe(
            false
        );
        expect(ConfigUtil.isImmunizationRecordLinkEnabled()).toBe(true);
        expect(ConfigUtil.showRecommendationsLink()).toBe(true);
    });

    it("reads profile notification types and channels case-insensitively", () => {
        const t = configFixture.webClient.featureToggleConfiguration;
        const set = (enabled: boolean, typeEnabled = true) =>
            useConfigStore.setState({
                config: {
                    ...configFixture,
                    webClient: {
                        ...configFixture.webClient,
                        featureToggleConfiguration: {
                            ...t,
                            profile: {
                                notifications: {
                                    enabled,
                                    type: [
                                        {
                                            name: "BCCANCERSCREENING",
                                            enabled: typeEnabled,
                                            preferences: {
                                                email: true,
                                                sms: false,
                                            },
                                        },
                                    ],
                                },
                            },
                        },
                    },
                },
            });
        const bc = ProfileNotificationType.BcCancerScreening;
        set(true);
        expect(ConfigUtil.isProfileNotificationsFeatureEnabled()).toBe(true);
        expect(ConfigUtil.isProfileNotificationTypeEnabled(bc)).toBe(true);
        expect(
            ConfigUtil.isProfileNotificationPreferenceEnabled(
                bc,
                ProfileNotificationPreference.Email
            )
        ).toBe(true);
        expect(
            ConfigUtil.isProfileNotificationPreferenceEnabled(
                bc,
                ProfileNotificationPreference.Sms
            )
        ).toBe(false);
        set(true, false);
        expect(ConfigUtil.isProfileNotificationTypeEnabled(bc)).toBe(false);
        expect(
            ConfigUtil.isProfileNotificationPreferenceEnabled(
                bc,
                ProfileNotificationPreference.Email
            )
        ).toBe(false);
        set(false);
        expect(ConfigUtil.isProfileNotificationsFeatureEnabled()).toBe(false);
        expect(ConfigUtil.isProfileNotificationTypeEnabled(bc)).toBe(false);
    });

    it("a dataset is on for dependents unless their override switches it off", () => {
        const base = configFixture.webClient.featureToggleConfiguration;
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...base,
                        datasets: [
                            { name: "immunization", enabled: true },
                            { name: "labResult", enabled: true },
                        ],
                        dependents: {
                            enabled: true,
                            timelineEnabled: false,
                            datasets: [{ name: "labResult", enabled: false }],
                        },
                    },
                },
            },
            status: "loaded",
        });
        expect(
            ConfigUtil.isDependentDatasetEnabled(EntryType.Immunization)
        ).toBe(true);
        expect(ConfigUtil.isDependentDatasetEnabled(EntryType.LabResult)).toBe(
            false
        );
        expect(ConfigUtil.isDependentDatasetEnabled(EntryType.Medication)).toBe(
            false
        );
    });
});
