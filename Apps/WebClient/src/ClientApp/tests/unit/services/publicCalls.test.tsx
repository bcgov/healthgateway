import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { HgButton } from "@/components/ui/HgButton";
import { CommunicationType } from "@/models/communication";
import { HttpError } from "@/models/errors";
import { CommunicationService } from "@/services/communication.service";
import { ConfigService } from "@/services/config.service";
import { HttpClient } from "@/services/http/httpClient";
import { logger } from "@/services/logger";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { configFixture } from "@/test/fixtures";
import { isDisabled, render, screen, unmount } from "@/test/ui";

/** Web's test environment has no fetch or Response to spy on. */
const stubFetch = () => {
    const fetch = jest.fn(async (_url: unknown, _init?: unknown) => ({
        ok: true,
        status: 200,
        text: async () => "{}",
    }));
    global.fetch = fetch as unknown as typeof global.fetch;
    return fetch;
};

const hold = (status: "expired" | "offline") =>
    useConnectivityStore.getState().setSessionStatus(status);

describe("calls that need no login", () => {
    afterEach(async () => {
        await unmount();
        jest.restoreAllMocks();
        useConnectivityStore.getState().setSessionStatus("live");
        useConnectivityStore.setState({
            isOnline: true,
            serviceUnavailable: false,
        });
        logger.setFailuresExpected(false);
    });

    it.each(["expired", "offline"] as const)(
        "configuration and communication still go out with a login %s, without a token",
        async (status) => {
            hold(status);
            const fetch = stubFetch();
            const http = new HttpClient();
            http.setAuthorizationHeader("old");
            await new ConfigService(http).getConfiguration("https://cfg");
            await new CommunicationService(http, configFixture).getActive(
                CommunicationType.Banner
            );
            expect(fetch).toHaveBeenCalledTimes(2);
            const [, init] = fetch.mock.calls[0];
            expect(
                (init as { headers: Record<string, string> }).headers
                    .Authorization
            ).toBeUndefined();
        }
    );

    it("a call that needs a login still fails at once while it is on hold", async () => {
        hold("expired");
        const fetch = stubFetch();
        await expect(new HttpClient().get("https://x")).rejects.toThrow(
            HttpError
        );
        expect(fetch).not.toHaveBeenCalled();
    });

    it("with no connection they fail at once too", async () => {
        useConnectivityStore.setState({ isOnline: false });
        const fetch = stubFetch();
        await expect(new HttpClient().getPublic("https://x")).rejects.toThrow(
            HttpError
        );
        expect(fetch).not.toHaveBeenCalled();
    });

    it("failures with a login on hold are logged quietly, not as console errors", () => {
        if (Platform.OS === "web") return;
        const error = jest
            .spyOn(console, "error")
            .mockImplementation(() => undefined);
        logger.setLevel("debug");
        jest.spyOn(console, "debug").mockImplementation(() => undefined);
        hold("expired");
        logger.error("expected");
        expect(error).not.toHaveBeenCalled();
        useConnectivityStore.getState().setSessionStatus("live");
        logger.error("real");
        expect(error).toHaveBeenCalledTimes(1);
        logger.setLevel("silent");
    });
});

describe("requiresConnection", () => {
    afterEach(async () => {
        await unmount();
        useConnectivityStore.setState({
            isOnline: true,
            serviceUnavailable: false,
            sessionStatus: "live",
        });
    });

    const disabled = () => isDisabled(screen.getByTestId("b"));

    it("is on while the call can work", async () => {
        await render(<HgButton testID="b" text="B" requiresConnection />);
        expect(disabled()).toBe(false);
    });

    it.each([
        ["offline", { isOnline: false }],
        ["service unavailable", { serviceUnavailable: true }],
        ["login on hold", { sessionStatus: "expired" as const }],
    ])("is off when %s", async (_name, state) => {
        useConnectivityStore.setState(state);
        await render(<HgButton testID="b" text="B" requiresConnection />);
        expect(disabled()).toBe(true);
    });

    it("leaves other buttons alone", async () => {
        useConnectivityStore.setState({ isOnline: false });
        await render(<HgButton testID="b" text="B" />);
        expect(disabled()).toBe(false);
    });
});
