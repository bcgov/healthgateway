import { useIsFocused } from "expo-router";
import { useEffect, useId } from "react";

import { useLoadingStore } from "@/stores/loading.store";

interface Props {
    isLoading: boolean;
    text?: string;
}

/**
 * Port of LoadingComponent.vue. The web client's v-overlay covers the whole viewport, so this component only
 * registers the request; `LoadingOverlay` in the root layout draws the scrim and spinner over the app.
 * (An inline View rather than a native Modal: a Modal unmounted mid-presentation while a route changes can
 * leave an invisible window on iOS that blocks all touches.)
 */
export function Loading({ isLoading, text }: Props) {
    const id = useId();
    // Native tabs stay mounted in the background: only the screen in front may cover the app
    const active = useIsFocused() && isLoading;
    const show = useLoadingStore((s) => s.show);
    const hide = useLoadingStore((s) => s.hide);
    useEffect(() => {
        if (!active) return;
        show(id, text);
        return () => hide(id);
    }, [active, text, id, show, hide]);
    return null;
}
