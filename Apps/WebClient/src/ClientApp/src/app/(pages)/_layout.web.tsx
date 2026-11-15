import { Slot } from "expo-router";

import { PageScroll } from "@/components/site/PageScroll";

/** Web: routes outside the tab bar, under the shell, which scrolls them (native: `_layout.tsx`). */
export default function PagesLayout() {
    return (
        <PageScroll>
            <Slot />
        </PageScroll>
    );
}
