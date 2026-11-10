import { HgAlert } from "@/components/common";
import { TooManyRequestsLocation } from "@/constants/errorType";
import { useErrorStore } from "@/stores/error.store";

/** Port of TooManyRequestsComponent.vue: shows the store's too-many-requests error / warning for one location. */
export function TooManyRequests({
    location = TooManyRequestsLocation.Page,
}: {
    location?: string;
}) {
    const error = useErrorStore((s) => s.tooManyRequestsError);
    const warning = useErrorStore((s) => s.tooManyRequestsWarning);
    const clearError = useErrorStore((s) => s.clearTooManyRequestsError);
    const clearWarning = useErrorStore((s) => s.clearTooManyRequestsWarning);
    if (error === location)
        return (
            <HgAlert
                testID="too-many-requests-error"
                type="error"
                live="assertive"
                closable
                centerContent
                onClose={clearError}
                text="Unable to complete action as the site is too busy. Please try again later."
            />
        );
    if (warning === location)
        return (
            <HgAlert
                testID="too-many-requests-warning"
                type="warning"
                live="polite"
                closable
                centerContent
                onClose={clearWarning}
                text="We are unable to complete all actions because the site is too busy. Please try again later."
            />
        );
    return null;
}
