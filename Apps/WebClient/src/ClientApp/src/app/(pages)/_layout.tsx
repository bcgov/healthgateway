import { Slot } from "expo-router";

import { PageScroll } from "@/components/site/PageScroll";

/** Routes outside the tab bar. Native: each gets the page scroll view; web: the shell scrolls. */
export default function PagesLayout() {
    return (
        <PageScroll>
            <Slot />
        </PageScroll>
    );
}
