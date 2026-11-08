import {
    enableLinkClickTracking,
    LinkClickTrackingPlugin,
} from "@snowplow/browser-plugin-link-click-tracking";
import { PerformanceTimingPlugin } from "@snowplow/browser-plugin-performance-timing";
import {
    enableActivityTracking,
    newTracker,
    trackPageView,
    trackSelfDescribingEvent,
} from "@snowplow/browser-tracker";

import { ACTION_SCHEMA, type EventData } from "@/constants/tracking";
import { logger } from "@/services/logger";

import type { ITrackingService, TrackingConfig } from "./trackingService";

/**
 * Web: equivalent of the web client's public/snowplow.js (sp-2-14-0 "rt" tracker) using the
 * maintained @snowplow/browser-tracker: POST, secure, webPage + performanceTiming contexts,
 * activity ping 30 s / 30 s, link-click tracking, page view on every route change.
 */
class WebTrackingService implements ITrackingService {
    private enabled = false;

    init(config: TrackingConfig) {
        this.enabled = config.enabled;
        if (!this.enabled) {
            logger.info("Snowplow tracking disabled by configuration");
            return;
        }
        newTracker("rt", config.collector, {
            appId: config.appId,
            platform: "web",
            cookieLifetime: 86400 * 548,
            eventMethod: "post",
            contexts: { webPage: true },
            plugins: [PerformanceTimingPlugin(), LinkClickTrackingPlugin()],
        });
        enableActivityTracking({ minimumVisitLength: 30, heartbeatDelay: 30 });
        enableLinkClickTracking();
        logger.info(`Snowplow tracking enabled (${config.collector})`);
    }

    trackEvent(data: EventData) {
        logger.debug(`Tracking event: ${data.action}`);
        if (!this.enabled) return;
        trackSelfDescribingEvent({
            event: { schema: ACTION_SCHEMA, data: { ...data } },
        });
    }

    trackPageView(_path: string) {
        logger.debug("Tracking page view");
        if (!this.enabled) return;
        trackPageView();
    }
}

export function createTrackingService(): ITrackingService {
    return new WebTrackingService();
}
