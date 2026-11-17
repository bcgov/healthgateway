import { create } from "zustand";

import { type Communication, CommunicationType } from "@/models/communication";
import { isSessionEnded, isTooManyRequestsError } from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";

/** The service's name in `webClient.cache.services` (and `webClient.mobile.cache.services`). */
export const COMMUNICATIONS_CACHE_SERVICE = "Communications";

/** After a fetch failed, that type waits this long before asking again, so a failing call isn't repeated on every page. */
export const COMMUNICATION_RETRY_MS = 60 * 1000;

export const TOO_MANY_REQUESTS_TEXT =
    "We are experiencing higher than usual site traffic, which may cause delays in accessing your health records. Please try again later.";

export interface CommunicationEntry {
    /** The active communication, or null when there is none. */
    communication: Communication | null;
    /** When the last fetch succeeded. */
    fetchedAt?: number;
    /** When the last fetch failed: no new fetch until `COMMUNICATION_RETRY_MS` has passed. */
    failedAt?: number;
}

interface CommunicationState {
    entries: Partial<Record<CommunicationType, CommunicationEntry>>;
    /**
     * Fetches `type` unless what is held is younger than `maxAgeMs`, or a fetch of it failed less than
     * `COMMUNICATION_RETRY_MS` ago. A fetch already running is shared, so any number of callers make one call.
     */
    retrieve: (type: CommunicationType, maxAgeMs: number) => Promise<void>;
    /** Drops one type: the in-app message on sign-out, as it may be for the person who was signed in. */
    clear: (type: CommunicationType) => void;
    /** Drops everything: Reset App and environment switches. */
    reset: () => void;
}

/** Kept out of the state: a promise changing should not repaint anything. */
const inFlight = new Map<CommunicationType, Promise<void>>();
/** Raised by `clear` and `reset`, so a fetch that started before them cannot bring back what they dropped. */
const generations = new Map<CommunicationType, number>();
const generationOf = (type: CommunicationType) => generations.get(type) ?? 0;
const nextGeneration = (type: CommunicationType) =>
    generations.set(type, generationOf(type) + 1);

/**
 * The communication banner's text (CommunicationComponent.vue), one entry per type: Banner for the public site, InApp
 * once registered. Every banner on screen reads from here, and `useCommunicationRetrieval` asks for it once for the
 * whole app, so the five tabs iOS and Android keep mounted share one call.
 */
export const useCommunicationStore = create<CommunicationState>((set, get) => {
    const update = (
        type: CommunicationType,
        change: Partial<CommunicationEntry>
    ) =>
        set((s) => ({
            entries: {
                ...s.entries,
                [type]: {
                    communication: null,
                    ...s.entries[type],
                    ...change,
                },
            },
        }));

    async function fetchType(type: CommunicationType, generation: number) {
        try {
            const result = await services.communication.getActive(type);
            if (generation !== generationOf(type)) return;
            update(type, {
                communication: result.resourcePayload ?? null,
                fetchedAt: Date.now(),
                failedAt: undefined,
            });
        } catch (error) {
            if (isSessionEnded(error)) return; // signed out while it ran: nothing to show, nothing to wait for
            if (generation !== generationOf(type)) return;
            update(type, {
                failedAt: Date.now(),
                // what was shown stays, unless the server is turning requests away
                ...(isTooManyRequestsError(error)
                    ? {
                          communication: {
                              text: TOO_MANY_REQUESTS_TEXT,
                              communicationTypeCode: type,
                          } as Communication,
                      }
                    : {}),
            });
            logger.debug(
                `Communication retrieval failed: ${describeError(error)}`
            );
        }
    }

    return {
        entries: {},

        retrieve: (type, maxAgeMs) => {
            const running = inFlight.get(type);
            if (running) return running;
            const entry = get().entries[type];
            const now = Date.now();
            if (
                entry?.fetchedAt !== undefined &&
                now - entry.fetchedAt < maxAgeMs
            )
                return Promise.resolve();
            if (
                entry?.failedAt !== undefined &&
                now - entry.failedAt < COMMUNICATION_RETRY_MS
            )
                return Promise.resolve();
            const request = fetchType(type, generationOf(type)).finally(() => {
                if (inFlight.get(type) === request) inFlight.delete(type);
            });
            inFlight.set(type, request);
            return request;
        },

        clear: (type) => {
            nextGeneration(type);
            inFlight.delete(type);
            set((s) => {
                const entries = { ...s.entries };
                delete entries[type];
                return { entries };
            });
        },

        reset: () => {
            for (const type of [
                CommunicationType.Banner,
                CommunicationType.Email,
                CommunicationType.InApp,
                CommunicationType.Mobile,
            ])
                nextGeneration(type);
            inFlight.clear();
            set({ entries: {} });
        },
    };
});
