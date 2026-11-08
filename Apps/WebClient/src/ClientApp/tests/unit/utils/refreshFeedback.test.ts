import { beforeEach, describe, expect, it, jest } from "@jest/globals";
import * as audio from "expo-audio";
import * as haptics from "expo-haptics";

jest.mock("expo-haptics", () => ({
    ImpactFeedbackStyle: { Light: "light" },
    impactAsync: jest.fn(async () => undefined),
}));
// the fresh module copy each test takes would otherwise get a logger not silenced by jest.setup.js
jest.mock("@/services/logger", () => ({
    describeError: () => "error",
    logger: { debug: jest.fn(), warn: jest.fn() },
}));
jest.mock("expo-audio", () => ({
    setAudioModeAsync: jest.fn(async () => undefined),
    createAudioPlayer: jest.fn(),
}));

const player = {
    seekTo: jest.fn<(seconds: number) => Promise<void>>(async () => undefined),
    play: jest.fn(),
};

describe("playRefreshFeedback", () => {
    let playRefreshFeedback: () => Promise<void>;

    beforeEach(() => {
        jest.clearAllMocks();
        jest.mocked(audio.createAudioPlayer).mockReturnValue(player as never);
        jest.mocked(haptics.impactAsync).mockResolvedValue();
        // a fresh module each time: it keeps one player
        jest.isolateModules(() => {
            ({ playRefreshFeedback } =
                // eslint-disable-next-line @typescript-eslint/no-require-imports
                require("@/utils/refreshFeedback") as typeof import("@/utils/refreshFeedback"));
        });
    });

    it("gives a light haptic and plays the tick from the start", async () => {
        await playRefreshFeedback();
        expect(haptics.impactAsync).toHaveBeenCalledWith("light" as never);
        expect(player.seekTo).toHaveBeenCalledWith(0);
        expect(player.play).toHaveBeenCalledTimes(1);
    });

    it("obeys the iOS silent switch and does not interrupt other audio", async () => {
        await playRefreshFeedback();
        expect(audio.setAudioModeAsync).toHaveBeenCalledWith({
            playsInSilentMode: false,
            interruptionMode: "mixWithOthers",
        });
    });

    it("reuses one player", async () => {
        await playRefreshFeedback();
        await playRefreshFeedback();
        expect(audio.createAudioPlayer).toHaveBeenCalledTimes(1);
        expect(player.play).toHaveBeenCalledTimes(2);
    });

    it("still plays the sound when the haptic fails", async () => {
        jest.mocked(haptics.impactAsync).mockRejectedValue(new Error("none"));
        await playRefreshFeedback();
        expect(player.play).toHaveBeenCalledTimes(1);
    });

    it("never throws when the sound fails", async () => {
        jest.mocked(audio.createAudioPlayer).mockImplementation(() => {
            throw new Error("no native module");
        });
        await expect(playRefreshFeedback()).resolves.toBeUndefined();
        expect(haptics.impactAsync).toHaveBeenCalled();
    });
});
