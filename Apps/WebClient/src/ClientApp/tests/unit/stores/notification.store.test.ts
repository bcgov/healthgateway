import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ResultError } from "@/models/errors";
import {
    type Notification,
    NotificationActionType,
} from "@/models/notification";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import {
    newNotificationsOf,
    NOTIFICATION_CENTRE_SWITCHED_ON,
    useNotificationStore,
} from "@/stores/notification.store";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

const notification = (id: string, at: string): Notification => ({
    id,
    categoryName: "Immunization",
    displayText: id,
    actionUrl: "https://x.test",
    actionType: NotificationActionType.InternalLink,
    scheduledDateTimeUtc: at,
});

describe("useNotificationStore (stores/notification.ts port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("stays switched off for parity with the web client (AB#16927)", () => {
        expect(NOTIFICATION_CENTRE_SWITCHED_ON).toBe(false);
    });

    it("retrieves once, then serves the stored list", async () => {
        const get = jest
            .spyOn(services.notification, "getNotifications")
            .mockResolvedValue([notification("A", "2026-01-01T00:00:00Z")]);
        await useNotificationStore.getState().retrieve();
        await useNotificationStore.getState().retrieve();
        expect(get).toHaveBeenCalledTimes(1);
        expect(useNotificationStore.getState().notifications).toHaveLength(1);
    });

    it("dismisses one or all", async () => {
        jest.spyOn(
            services.notification,
            "dismissNotification"
        ).mockResolvedValue();
        jest.spyOn(
            services.notification,
            "dismissNotifications"
        ).mockResolvedValue();
        useNotificationStore.setState({
            notifications: [
                notification("A", "2026-01-01T00:00:00Z"),
                notification("B", "2026-01-01T00:00:00Z"),
            ],
        });
        await useNotificationStore.getState().dismissNotification("A");
        expect(
            useNotificationStore.getState().notifications.map((n) => n.id)
        ).toEqual(["B"]);
        await useNotificationStore.getState().dismissAllNotifications();
        expect(useNotificationStore.getState().notifications).toEqual([]);
    });

    it("a failed dismiss reports 429 as too many requests, others as a banner", async () => {
        const dismiss = jest.spyOn(
            services.notification,
            "dismissNotification"
        );
        dismiss.mockRejectedValue(new ResultError("X", "busy", 429));
        await expect(
            useNotificationStore.getState().dismissNotification("A")
        ).rejects.toThrow();
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");
        dismiss.mockRejectedValue(new ResultError("X", "broken", 500));
        await expect(
            useNotificationStore.getState().dismissNotification("A")
        ).rejects.toThrow();
        expect(useErrorStore.getState().errors.length).toBeGreaterThan(0);
    });

    it("new notifications are those after the previous login, or all without one", () => {
        const old = notification("OLD", "2026-01-01T00:00:00Z");
        const fresh = notification("NEW", "2026-03-01T00:00:00Z");
        expect(
            newNotificationsOf([old, fresh], "2026-02-01T00:00:00Z")
        ).toEqual([fresh]);
        expect(newNotificationsOf([old, fresh], undefined)).toEqual([
            old,
            fresh,
        ]);
    });
});
