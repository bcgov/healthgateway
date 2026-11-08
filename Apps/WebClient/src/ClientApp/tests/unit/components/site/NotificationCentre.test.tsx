import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { NotificationCentre } from "@/components/site/NotificationCentre";
import { EntryType } from "@/constants/entryType";
import { NotificationActionType } from "@/models/notification";
import { services } from "@/services/container";
import { useNotificationStore } from "@/stores/notification.store";
import { useTimelineStore } from "@/stores/timeline.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";

const notification = (id: string, categoryName: string) => ({
    id,
    categoryName,
    displayText: `Text ${id}`,
    actionUrl: "https://x.test/path",
    actionType: NotificationActionType.InternalLink,
    scheduledDateTimeUtc: "2026-01-01T00:00:00Z",
});

const open = (notifications: ReturnType<typeof notification>[]) =>
    updateStores(() =>
        useNotificationStore.setState({
            notifications,
            isNotificationCenterOpen: true,
        })
    );

describe("NotificationCentre (NotificationCentreComponent.vue port)", () => {
    beforeEach(() => {
        resetRouter("/home");
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("shows the empty state", async () => {
        open([]);
        await render(<NotificationCentre />);
        expect(screen.getByText("No Notifications")).toBeTruthy();
        expect(screen.queryByTestId("notifications-div")).toBeNull();
    });

    it("a record notification filters the timeline and closes", async () => {
        open([notification("A", "Immunization")]);
        await render(<NotificationCentre />);
        await press(screen.getByTestId("notification-A-action-button"));
        expect(router.push).toHaveBeenCalledWith("/timeline");
        expect(useTimelineStore.getState().filter.entryTypes).toContain(
            EntryType.Immunization
        );
        expect(useNotificationStore.getState().isNotificationCenterOpen).toBe(
            false
        );
    });

    it("an organ donor notification opens Services; other internal links their path", async () => {
        open([notification("A", "BctOdr"), notification("B", "Other")]);
        await render(<NotificationCentre />);
        await press(screen.getByTestId("notification-A-action-button"));
        expect(router.push).toHaveBeenCalledWith("/services");
        open([notification("B", "Other")]);
        await press(screen.getByTestId("notification-B-action-button"));
        expect(router.push).toHaveBeenCalledWith("/path");
    });

    it("dismisses one and all", async () => {
        const one = jest
            .spyOn(services.notification, "dismissNotification")
            .mockResolvedValue();
        const all = jest
            .spyOn(services.notification, "dismissNotifications")
            .mockResolvedValue();
        open([notification("A", "Other"), notification("B", "Other")]);
        await render(<NotificationCentre />);
        await press(screen.getByTestId("notification-A-dismiss-button"));
        expect(one).toHaveBeenCalled();
        await press(
            screen.getByTestId("notification-centre-dismiss-all-button")
        );
        expect(all).toHaveBeenCalled();
        expect(screen.getByText("No Notifications")).toBeTruthy();
    });
});
