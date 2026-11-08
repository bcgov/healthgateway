import { describeError, logger } from "@/services/logger";

interface TickPlayer {
    seekTo: (seconds: number) => Promise<void>;
    play: () => void;
}

let player: TickPlayer | undefined;

/**
 * The pull that starts a refresh: a light haptic and a short quiet tick. The sound obeys the iOS silent switch and mixes
 * with whatever else is playing. Lazy so a dev client built before these modules were added fails softly (AGENTS.md);
 * neither failing may get in the way of the refresh, or of the other.
 */
export async function playRefreshFeedback(): Promise<void> {
    try {
        const haptics = await import("expo-haptics");
        await haptics.impactAsync(haptics.ImpactFeedbackStyle.Light);
    } catch (error) {
        logger.debug(`Refresh haptic unavailable: ${describeError(error)}`);
    }
    try {
        const audio = await import("expo-audio");
        if (!player) {
            await audio.setAudioModeAsync({
                playsInSilentMode: false,
                interruptionMode: "mixWithOthers",
            });
            player = audio.createAudioPlayer(
                require("@/assets/sounds/refresh-tick.wav") as number
            );
        }
        await player.seekTo(0);
        player.play();
    } catch (error) {
        logger.debug(`Refresh sound unavailable: ${describeError(error)}`);
    }
}
