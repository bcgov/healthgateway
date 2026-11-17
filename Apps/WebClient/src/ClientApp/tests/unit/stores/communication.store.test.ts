import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { type Communication, CommunicationType } from "@/models/communication";
import { HttpError, SessionEndedError } from "@/models/errors";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import {
    COMMUNICATION_RETRY_MS,
    TOO_MANY_REQUESTS_TEXT,
    useCommunicationStore,
} from "@/stores/communication.store";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";

const HOUR = 60 * 60 * 1000;
const START = Date.UTC(2026, 10, 16, 12);

const payload = (text: string, type = CommunicationType.Banner) => ({
    resourcePayload: { text, communicationTypeCode: type } as Communication,
});
const textOf = (type: CommunicationType) =>
    useCommunicationStore.getState().entries[type]?.communication?.text;
const retrieve = (type: CommunicationType, maxAgeMs = HOUR) =>
    useCommunicationStore.getState().retrieve(type, maxAgeMs);

describe("communication store", () => {
    let now: number;
    let getActive: jest.SpiedFunction<typeof services.communication.getActive>;

    beforeEach(() => {
        now = START;
        jest.spyOn(Date, "now").mockImplementation(() => now);
        services.initialize(configFixture, fakeAuth());
        getActive = jest
            .spyOn(services.communication, "getActive")
            .mockResolvedValue(payload("first") as never);
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("makes one call for any number of callers asking at once", async () => {
        await Promise.all([
            retrieve(CommunicationType.Banner),
            retrieve(CommunicationType.Banner),
            retrieve(CommunicationType.Banner),
            retrieve(CommunicationType.Banner),
            retrieve(CommunicationType.Banner),
        ]);
        expect(getActive).toHaveBeenCalledTimes(1);
        expect(textOf(CommunicationType.Banner)).toBe("first");
    });

    it("keeps each type on its own: one call per type", async () => {
        getActive.mockImplementation(
            async (type) =>
                payload(
                    type === CommunicationType.InApp ? "in app" : "public",
                    type
                ) as never
        );
        await retrieve(CommunicationType.Banner);
        await retrieve(CommunicationType.InApp);
        await retrieve(CommunicationType.Banner);
        await retrieve(CommunicationType.InApp);
        expect(getActive).toHaveBeenCalledTimes(2);
        expect(textOf(CommunicationType.Banner)).toBe("public");
        expect(textOf(CommunicationType.InApp)).toBe("in app");
    });

    it("asks again only once what it holds is older than the cache time", async () => {
        await retrieve(CommunicationType.Banner, HOUR);
        now = START + HOUR - 1;
        await retrieve(CommunicationType.Banner, HOUR);
        expect(getActive).toHaveBeenCalledTimes(1);

        getActive.mockResolvedValue(payload("second") as never);
        now = START + HOUR;
        await retrieve(CommunicationType.Banner, HOUR);
        expect(getActive).toHaveBeenCalledTimes(2);
        expect(textOf(CommunicationType.Banner)).toBe("second");
    });

    it("asks every time with a cache time of zero, still sharing a call in flight", async () => {
        await retrieve(CommunicationType.Banner, 0);
        await Promise.all([
            retrieve(CommunicationType.Banner, 0),
            retrieve(CommunicationType.Banner, 0),
        ]);
        expect(getActive).toHaveBeenCalledTimes(2);
    });

    it("holds that there is none, without asking again within the cache time", async () => {
        getActive.mockResolvedValue({ resourcePayload: null } as never);
        await retrieve(CommunicationType.Banner);
        await retrieve(CommunicationType.Banner);
        expect(getActive).toHaveBeenCalledTimes(1);
        expect(textOf(CommunicationType.Banner)).toBeUndefined();
    });

    it("after a failure keeps what was shown and waits a minute before asking again", async () => {
        await retrieve(CommunicationType.Banner, 0);
        getActive.mockRejectedValue(new HttpError("down", 500));
        now += 1;
        await retrieve(CommunicationType.Banner, 0);
        expect(getActive).toHaveBeenCalledTimes(2);
        expect(textOf(CommunicationType.Banner)).toBe("first");

        now += COMMUNICATION_RETRY_MS - 1;
        await retrieve(CommunicationType.Banner, 0);
        expect(getActive).toHaveBeenCalledTimes(2);

        getActive.mockResolvedValue(payload("back") as never);
        now += 1;
        await retrieve(CommunicationType.Banner, 0);
        expect(getActive).toHaveBeenCalledTimes(3);
        expect(textOf(CommunicationType.Banner)).toBe("back");
    });

    it("shows the high-traffic message when requests are turned away, then waits", async () => {
        getActive.mockRejectedValue(new HttpError("busy", 429));
        await retrieve(CommunicationType.InApp);
        expect(textOf(CommunicationType.InApp)).toBe(TOO_MANY_REQUESTS_TEXT);
        await retrieve(CommunicationType.InApp);
        expect(getActive).toHaveBeenCalledTimes(1);
    });

    it("ignores a fetch ended by signing out: nothing shown, nothing to wait for", async () => {
        getActive.mockRejectedValue(new SessionEndedError());
        await retrieve(CommunicationType.InApp);
        expect(
            useCommunicationStore.getState().entries[CommunicationType.InApp]
        ).toBeUndefined();
        await retrieve(CommunicationType.InApp);
        expect(getActive).toHaveBeenCalledTimes(2);
    });

    it("clearing a type drops it, and a fetch already running cannot bring it back", async () => {
        let answer: (value: unknown) => void = () => undefined;
        getActive.mockImplementation(
            () => new Promise((resolve) => (answer = resolve)) as never
        );
        const running = retrieve(CommunicationType.InApp);
        useCommunicationStore.getState().clear(CommunicationType.InApp);
        answer(payload("old person's message", CommunicationType.InApp));
        await running;
        expect(textOf(CommunicationType.InApp)).toBeUndefined();
    });

    it("signing out drops the in-app message and keeps the public banner", async () => {
        getActive.mockImplementation(
            async (type) =>
                payload(
                    type === CommunicationType.InApp ? "in app" : "public",
                    type
                ) as never
        );
        await retrieve(CommunicationType.Banner);
        await retrieve(CommunicationType.InApp);
        useAuthStore.getState().clearStorage();
        expect(textOf(CommunicationType.InApp)).toBeUndefined();
        expect(textOf(CommunicationType.Banner)).toBe("public");
    });

    it("an entry from another environment is neither shown nor kept: the current one is asked", async () => {
        await retrieve(CommunicationType.Banner);
        expect(textOf(CommunicationType.Banner)).toBe("first");
        // the app switched environment: new services, another endpoint
        services.initialize(
            {
                ...configFixture,
                serviceEndpoints: {
                    ...configFixture.serviceEndpoints,
                    GatewayApi: "https://api.other/gateway/",
                },
            },
            fakeAuth()
        );
        const other = jest
            .spyOn(services.communication, "getActive")
            .mockResolvedValue(payload("other environment") as never);
        await retrieve(CommunicationType.Banner);
        expect(other).toHaveBeenCalledTimes(1);
        expect(textOf(CommunicationType.Banner)).toBe("other environment");
        expect(
            useCommunicationStore.getState().entries[CommunicationType.Banner]
                ?.source
        ).toBe("https://api.other/gateway/Communication");
    });

    it("reset drops every type", async () => {
        await retrieve(CommunicationType.Banner);
        useCommunicationStore.getState().reset();
        expect(useCommunicationStore.getState().entries).toEqual({});
        await retrieve(CommunicationType.Banner);
        expect(getActive).toHaveBeenCalledTimes(2);
    });
});
