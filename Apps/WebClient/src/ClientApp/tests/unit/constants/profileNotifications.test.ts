import { describe, expect, it } from "@jest/globals";

import { ExternalUrl } from "@/constants/externalUrls";
import {
    getNotificationPreferenceTypes,
    getProfileNotificationType,
    getUserProfileNotificationSettings,
    getUserProfileNotificationType,
    ProfileNotificationPreference,
    ProfileNotificationType,
} from "@/constants/profileNotifications";

describe("profile notification definitions (constants/profileNotifications.ts port)", () => {
    it("has one entry per notification type with the web client's id, label and dialog copy", () => {
        const types = getNotificationPreferenceTypes();
        expect(types.map((t) => t.type).sort()).toEqual(
            Object.values(ProfileNotificationType).sort()
        );
        const bc = types[0];
        expect(bc.id).toBe("bc-cancer-screening");
        expect(bc.label).toBe("BC Cancer Screening Letters");
        expect(bc.modal.content.title).toBe(
            "BC Cancer Screening Program Letters"
        );
        expect(bc.modal.content.paragraphs).toHaveLength(3);
        const link = bc.modal.content.paragraphs[2].segments[1];
        expect(link).toMatchObject({
            type: "link",
            text: "screeningbc.ca/contact",
            href: ExternalUrl.ScreeningContact,
        });
        expect(ExternalUrl.ScreeningContact).toBe(
            "https://www.bccancer.bc.ca/screening/contact"
        );
    });

    it("maps between the API and UI type names", () => {
        expect(
            getUserProfileNotificationType(
                ProfileNotificationType.BcCancerScreening
            )
        ).toBe("BcCancerScreening");
        expect(getProfileNotificationType("BcCancerScreening")).toBe(
            ProfileNotificationType.BcCancerScreening
        );
        expect(getProfileNotificationType("SomethingNew")).toBeNull();
    });

    it("turns saved settings into enabled channel lists and drops unknown types", () => {
        expect(
            getUserProfileNotificationSettings([
                {
                    type: "BcCancerScreening",
                    emailEnabled: true,
                    smsEnabled: null,
                },
                { type: "SomethingNew", emailEnabled: true },
            ])
        ).toEqual([
            {
                type: ProfileNotificationType.BcCancerScreening,
                preferences: [ProfileNotificationPreference.Email],
            },
        ]);
        expect(getUserProfileNotificationSettings(undefined)).toEqual([]);
    });
});
