import { useEffect, useState } from "react";

import { announce, AppText, Dialog, HgButton } from "@/components/ui";
import { ReliableTimer } from "@/utils/reliableTimer";

interface Props {
    /** countdown in ms; undefined = hidden */
    countdown?: number;
    onStillHere: () => void;
    onExpired: () => void;
}

/** Port of IdleDialogComponent.vue: "Are you still there?" with a live countdown to the automatic logout. */
export function IdleDialog({ countdown, onStillHere, onExpired }: Props) {
    const [remaining, setRemaining] = useState<number | undefined>(undefined);

    useEffect(() => {
        if (countdown === undefined) return;
        // said once as the dialog opens; the ticking text itself is not a live region
        announce(
            `You will be automatically logged out in ${Math.ceil(countdown / 1000)} seconds.`,
            "assertive"
        );
        const timer = new ReliableTimer(onExpired, Math.max(countdown, 1000));
        const tick = () => setRemaining(timer.remainingTime);
        timer.start();
        const interval = setInterval(tick, 1000);
        tick();
        return () => {
            clearInterval(interval);
            timer.cancel();
        };
        // the dialog is keyed on the countdown value by its owner; callbacks are stable there
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [countdown]);

    const seconds = Math.ceil((remaining ?? countdown ?? 0) / 1000);
    return (
        <Dialog
            visible={countdown !== undefined}
            title="Are you still there?"
            tone="primary"
            persistent
            onRequestClose={onStillHere}
            testID="idleModal"
            actions={
                <HgButton
                    text="I'm here!"
                    uppercase={false}
                    onPress={onStillHere}
                />
            }
        >
            <AppText testID="idleModalText">
                You will be automatically logged out in {seconds} seconds.
            </AppText>
        </Dialog>
    );
}
