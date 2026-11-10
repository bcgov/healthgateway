import { useRouteFeaturesEnabled } from "@/hooks/useRouteFeaturesEnabled";

/**
 * Web: the shell's scroll view scrolls the page (see `PageScroll.tsx` for native). Holds the page back on a route
 * whose feature is switched off, until the route guard sends the user on (`useRouteFeaturesEnabled`).
 */
export function PageScroll({ children }: { children: React.ReactNode }) {
    const enabled = useRouteFeaturesEnabled();
    return enabled ? <>{children}</> : null;
}
