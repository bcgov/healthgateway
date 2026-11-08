import { beforeEach, describe, expect, it, jest } from "@jest/globals";

import { environments } from "@/config/environments";
import { ACTION_SCHEMA, type EventData } from "@/constants/tracking";
import { createTrackingService } from "@/services/tracking/trackingService.impl.native";

const mockPayload = jest.fn();
interface TrackerConfig {
    appId: string;
    endpoint: string;
    plugins: { beforeTrack: (p: { add: typeof mockPayload }) => void }[];
}
/** Runs the plugin as the real tracker does before each event */
const mockTracker = (config: TrackerConfig) => {
    const stamp = () => config.plugins[0].beforeTrack({ add: mockPayload });
    return {
        trackPageViewEvent: jest.fn<(event: unknown) => void>(stamp),
        trackSelfDescribingEvent: jest.fn<(event: unknown) => void>(stamp),
    };
};
const mockNewTracker = jest.fn(async (config: TrackerConfig) =>
    mockTracker(config)
);

jest.mock("@snowplow/react-native-tracker", () => ({
    newTracker: (config: TrackerConfig) => mockNewTracker(config),
}));

const settle = () => new Promise((resolve) => setImmediate(resolve));
const tracking = (id: string) =>
    environments.find((e) => e.id === id)!.tracking;
const lastUrl = () => mockPayload.mock.lastCall;

describe("native tracking follows the environment", () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    it("uses the web client's collector and app id on every environment", async () => {
        for (const { tracking: config } of environments) {
            expect(config).toMatchObject({
                enabled: true,
                collector: "spt.apps.gov.bc.ca",
                appId: "Snowplow_standalone",
            });
        }
    });

    it.each([
        ["local", "https://dev.healthgateway.gov.bc.ca/home"],
        ["dev", "https://dev.healthgateway.gov.bc.ca/home"],
        ["test", "https://test.healthgateway.gov.bc.ca/home"],
        ["prod", "https://www.healthgateway.gov.bc.ca/home"],
    ])("%s reports its pages on %s", async (id, url) => {
        const service = createTrackingService();
        service.init(tracking(id));
        service.trackPageView("/home");
        await settle();
        expect(lastUrl()).toEqual(["url", url]);
    });

    it("sends the first page, tracked before the environment was known", async () => {
        const service = createTrackingService();
        service.trackPageView("/first");
        await settle();
        expect(mockPayload).not.toHaveBeenCalled();
        service.init(tracking("dev"));
        await settle();
        expect(lastUrl()).toEqual([
            "url",
            "https://dev.healthgateway.gov.bc.ca/first",
        ]);
    });

    it("sends a page view, as the web client does, with the page address", async () => {
        const service = createTrackingService();
        service.init(tracking("test"));
        service.trackPageView("/timeline");
        await settle();
        const tracker = (await mockNewTracker.mock.results[0]
            .value) as ReturnType<typeof mockTracker>;
        expect(tracker.trackPageViewEvent).toHaveBeenCalledWith({
            pageUrl: "https://test.healthgateway.gov.bc.ca/timeline",
            pageTitle: "Health Gateway",
        });
    });

    it("switching keeps the one tracker and later events carry the new host", async () => {
        const service = createTrackingService();
        service.init(tracking("dev"));
        service.trackPageView("/home");
        await settle();
        service.init(tracking("prod"));
        service.trackEvent({ action: "click" } as unknown as EventData);
        await settle();
        expect(mockNewTracker).toHaveBeenCalledTimes(1);
        expect(lastUrl()).toEqual([
            "url",
            "https://www.healthgateway.gov.bc.ca/home",
        ]);
        const tracker = (await mockNewTracker.mock.results[0]
            .value) as ReturnType<typeof mockTracker>;
        expect(tracker.trackSelfDescribingEvent).toHaveBeenCalledWith({
            schema: ACTION_SCHEMA,
            data: { action: "click" },
        });
    });

    it("gives the tracker's uuid a random source before loading it", async () => {
        const global = globalThis as unknown as { crypto?: unknown };
        const original = global.crypto;
        Object.defineProperty(globalThis, "crypto", {
            value: undefined,
            configurable: true,
            writable: true,
        });
        createTrackingService().init(tracking("dev"));
        await settle();
        expect(
            (global.crypto as { getRandomValues?: unknown }).getRandomValues
        ).toEqual(expect.any(Function));
        Object.defineProperty(globalThis, "crypto", {
            value: original,
            configurable: true,
            writable: true,
        });
    });

    it("a tracker that fails to start does not break tracking calls", async () => {
        mockNewTracker.mockRejectedValueOnce(new Error("no storage"));
        const service = createTrackingService();
        service.init(tracking("dev"));
        service.trackPageView("/a");
        await settle();
        expect(mockPayload).not.toHaveBeenCalled();
    });
});
