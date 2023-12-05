import { defineStore } from "pinia";

import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";

export enum EventName {
    OpenNoteDialog = "openNoteDialog",
    OpenFullscreenTimelineEntry = "openFullscreenTimelineEntry",
    UpdateTimelineEntry = "updateTimelineEntry",
}

export const useEventStore = defineStore("event", () => {
    // eslint-disable-next-line unused-imports/no-unused-vars
    function emit(eventName: EventName, payload?: any): void {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.debug(`Emitting event "${eventName}"`);
    }

    function subscribe(
        this: ReturnType<typeof useEventStore>,
        eventName: EventName,
        callback: (payload: any) => void
    ): () => void {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        return this.$onAction(({ name, args, after }) => {
            if (name === "emit" && args[0] === eventName) {
                after(() => {
                    logger.debug(`Responding to event "${eventName}"`);
                    callback(args[1]);
                });
            }
        });
    }

    return { emit, subscribe };
});
