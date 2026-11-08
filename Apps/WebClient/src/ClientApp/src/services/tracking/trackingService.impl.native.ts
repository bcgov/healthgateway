import { ACTION_SCHEMA, type EventData } from "@/constants/tracking";
import { describeError, logger } from "@/services/logger";

import type { ITrackingService, TrackingConfig } from "./trackingService";

type RnTracker = import("@snowplow/react-native-tracker").ReactNativeTracker;

/**
 * Native: @snowplow/react-native-tracker sending what the web client sends: its collector, app id, action schema and page
 * views. The tracker marks the platform as mobile. Every event carries a page address on the selected environment's host,
 * which is how the web client's analytics are told apart, so `init` runs once that is known and again after switching.
 * Anything tracked before the first `init` (the first page) waits for it.
 */
class NativeTrackingService implements ITrackingService {
    private tracker?: Promise<RnTracker | undefined>;
    private origin?: string;
    private path = "/";
    private waiting: ((tracker: RnTracker) => void)[] = [];

    init(config: TrackingConfig) {
        if (!config.enabled) {
            logger.info("Snowplow tracking disabled by configuration");
            return;
        }
        this.origin = config.origin;
        if (this.tracker) return;
        this.tracker = this.start(config);
        const waiting = this.waiting;
        this.waiting = [];
        waiting.forEach((send) => this.send(send));
    }

    private async start(
        config: TrackingConfig
    ): Promise<RnTracker | undefined> {
        try {
            /* eslint-disable @typescript-eslint/no-require-imports */
            // The tracker's uuid needs this. Its usual provider's native module is not linked, as it only comes in
            // through the tracker; expo-crypto is, and that provider leaves one that is already there alone.
            const random = globalThis as unknown as {
                crypto?: { getRandomValues?: unknown };
            };
            random.crypto ??= {};
            random.crypto.getRandomValues ??= (
                require("expo-crypto") as typeof import("expo-crypto")
            ).getRandomValues;
            const { newTracker } =
                require("@snowplow/react-native-tracker") as typeof import("@snowplow/react-native-tracker");
            /* eslint-enable @typescript-eslint/no-require-imports */
            const tracker = await newTracker({
                namespace: "rt",
                appId: config.appId,
                endpoint: `https://${config.collector}`,
                eventMethod: "post",
                plugins: [
                    {
                        beforeTrack: (payload) =>
                            payload.add("url", `${this.origin}${this.path}`),
                    },
                ],
            });
            logger.info(`Snowplow tracking enabled (${config.collector})`);
            return tracker;
        } catch (error) {
            logger.warn(
                `Snowplow tracker unavailable: ${describeError(error)}`
            );
            return undefined;
        }
    }

    private send(event: (tracker: RnTracker) => void) {
        if (!this.tracker) {
            this.waiting.push(event);
            return;
        }
        void this.tracker.then((tracker) => {
            if (tracker) event(tracker);
        });
    }

    trackEvent(data: EventData) {
        logger.debug(`Tracking event: ${data.action}`);
        this.send((t) =>
            t.trackSelfDescribingEvent({
                schema: ACTION_SCHEMA,
                data: { ...data },
            })
        );
    }

    trackPageView(path: string) {
        logger.debug("Tracking page view");
        this.send((t) => {
            this.path = path || "/";
            t.trackPageViewEvent({
                pageUrl: `${this.origin}${this.path}`,
                pageTitle: "Health Gateway",
            });
        });
    }
}

export function createTrackingService(): ITrackingService {
    return new NativeTrackingService();
}
