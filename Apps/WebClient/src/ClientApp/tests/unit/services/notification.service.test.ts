import { describe, expect, it } from "@jest/globals";

import { HttpError, ResultError } from "@/models/errors";
import { NotificationService } from "@/services/notification.service";
import { configFixture, FakeHttp } from "@/test/fixtures";

const disabledConfig = {
    ...configFixture,
    webClient: {
        ...configFixture.webClient,
        featureToggleConfiguration: {
            ...configFixture.webClient.featureToggleConfiguration,
            notificationCentre: { enabled: false },
        },
    },
};

describe("NotificationService (restNotificationService port)", () => {
    it("reads and dismisses on GatewayApi Notification/{hdid}", async () => {
        const http = new FakeHttp().reply([]).reply(undefined).reply(undefined);
        const svc = new NotificationService(http, configFixture);
        await expect(svc.getNotifications("H")).resolves.toEqual([]);
        await svc.dismissNotification("H", "N1");
        await svc.dismissNotifications("H");
        expect(http.calls.map((c) => [c.method, c.url])).toEqual([
            ["GET", "https://api.test/gateway/Notification/H"],
            ["DELETE", "https://api.test/gateway/Notification/H/N1"],
            ["DELETE", "https://api.test/gateway/Notification/H"],
        ]);
    });

    it("makes no calls when the notification centre is off", async () => {
        const http = new FakeHttp();
        const svc = new NotificationService(http, disabledConfig);
        await expect(svc.getNotifications("H")).resolves.toEqual([]);
        await svc.dismissNotification("H", "N1");
        await svc.dismissNotifications("H");
        expect(http.calls).toHaveLength(0);
    });

    it("translates transport errors", async () => {
        const http = new FakeHttp().reply(new HttpError("nope", 500));
        const svc = new NotificationService(http, configFixture);
        await expect(svc.getNotifications("H")).rejects.toBeInstanceOf(
            ResultError
        );
    });
});
