import { useState } from "react";

import { usePageRefresh } from "@/components/site/PageScrollContext";
import {
    selectCanFetch,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import { useErrorStore } from "@/stores/error.store";

/** The page's refresh indicator goes after this; slower records still arrive. As on Health Records. */
const PULL_MAX_MS = 10000;

/**
 * Pull to refresh for a page (native), while its records can be fetched. Returns whether a pull is running, so the page
 * can keep what it shows instead of covering it with a loading state: the refresh indicator says this already.
 */
export function usePagePull(refresh: () => Promise<unknown>): boolean {
    const canFetch = useConnectivityStore(selectCanFetch);
    const [pulling, setPulling] = useState(false);
    usePageRefresh(
        canFetch
            ? async () => {
                  const errors = useErrorStore.getState();
                  errors.clearErrors();
                  errors.clearTooManyRequestsWarning();
                  setPulling(true);
                  let timer: ReturnType<typeof setTimeout> | undefined;
                  await Promise.race([
                      refresh().catch(() => undefined),
                      new Promise((resolve) => {
                          timer = setTimeout(resolve, PULL_MAX_MS);
                      }),
                  ]);
                  clearTimeout(timer);
                  setPulling(false);
              }
            : undefined
    );
    return pulling;
}
