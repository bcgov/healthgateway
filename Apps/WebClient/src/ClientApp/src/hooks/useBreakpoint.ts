import { Platform, useWindowDimensions } from "react-native";

import { breakpoints } from "@/theme";

/** Mirrors Vuetify's useDisplay() thresholds used by the web client. */
export function useBreakpoint() {
    const { width, height } = useWindowDimensions();
    return {
        /**
         * Timeline entries expand in place, rather than opening full screen. Web: the web client's rule, !isMobile.
         * Native also from 600pt on the window's shorter side (stated deviation): tablets and open foldables report
         * less than 960pt while being physically wide. The shorter side keeps a phone in landscape full screen, and
         * means rotating a tablet or foldable does not change the layout.
         */
        inlineEntryDetails:
            width >= breakpoints.md ||
            (Platform.OS !== "web" &&
                Math.min(width, height) >= breakpoints.sm),
        width,
        xs: width < breakpoints.sm,
        smAndDown: width < breakpoints.md,
        mdAndUp: width >= breakpoints.md,
        lgAndUp: width >= breakpoints.lg,
        /** layoutStore.isMobile in the web client = !mdAndUp */
        isMobile: width < breakpoints.md,
    };
}
