import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { createElement } from "react";
import { Text } from "react-native";

import { PageScroll } from "@/components/site/PageScroll";
import { usePageRefresh } from "@/components/site/PageScrollContext";
import { Path } from "@/constants/path";
import { usePageRefreshStore } from "@/stores/pageRefresh.store";
import { resetRouter, routerState } from "@/test/expoRouterMock";
import { resetStores, updateStores } from "@/test/fixtures";
import { advanceTimers, inAct, render, unmount } from "@/test/ui";

// native only (a .ts test): web has no navigation bar Refresh

jest.mock("@/utils/refreshFeedback", () => ({
    playRefreshFeedback: () => Promise.resolve(),
}));

let pulls = 0;
function RefreshablePage() {
    usePageRefresh(async () => {
        pulls += 1;
    });
    return createElement(Text, null, "page");
}

const page = (refreshable: boolean) =>
    createElement(
        PageScroll,
        null,
        refreshable
            ? createElement(RefreshablePage)
            : createElement(Text, null, "page")
    );

describe("PageScroll: the navigation bar's Refresh", () => {
    beforeEach(() => resetRouter(Path.Timeline));
    afterEach(() => {
        jest.useRealTimers();
        pulls = 0;
        routerState.focused = true;
        updateStores(() =>
            usePageRefreshStore.setState({
                refresh: undefined,
                refreshing: false,
            })
        );
        resetStores();
    });

    it("runs the page's pull to refresh, once at a time", async () => {
        jest.useFakeTimers();
        await render(page(true));
        const refresh = usePageRefreshStore.getState().refresh;
        expect(refresh).toBeDefined();
        await inAct(() => refresh?.());
        expect(pulls).toBe(1);
        expect(usePageRefreshStore.getState().refreshing).toBe(true);
        // a second press while it runs does nothing
        await inAct(() => usePageRefreshStore.getState().refresh?.());
        expect(pulls).toBe(1);
        await advanceTimers(1500);
        expect(usePageRefreshStore.getState().refreshing).toBe(false);
    });

    it("is not offered by a page without pull to refresh", async () => {
        await render(page(false));
        expect(usePageRefreshStore.getState().refresh).toBeUndefined();
    });

    it("is not offered by a page in the background", async () => {
        routerState.focused = false;
        await render(page(true));
        expect(usePageRefreshStore.getState().refresh).toBeUndefined();
    });

    it("goes when the page does", async () => {
        await render(page(true));
        await unmount();
        expect(usePageRefreshStore.getState().refresh).toBeUndefined();
    });
});
