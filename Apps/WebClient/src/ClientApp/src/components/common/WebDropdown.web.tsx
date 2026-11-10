import { useEffect, useLayoutEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { View } from "react-native";

import { focusFirstMenuItem, menuKeyDown } from "./menuKeys";
import { webClassRef } from "./webClass";

interface Props {
    /** the field it opens under */
    anchor: unknown;
    onClose: () => void;
    /** the list's role: "menu" (default) or "listbox" */
    role?: "menu" | "listbox";
    /** the list's name (a listbox needs one) */
    accessibilityLabel?: string;
    children: React.ReactNode;
}

interface Placement {
    top: number;
    left: number;
    width: number;
    maxHeight: number;
}

const MIN_HEIGHT = 160;
const EDGE = 16;

/**
 * Vuetify's v-menu on web: no backdrop, so the page stays usable and its activator uncovered; it follows the activator
 * wherever it moves and closes on a press outside. `.v-overlay--active.v-menu` is for `cy.vSelect`.
 */
export function WebDropdown({
    anchor,
    onClose,
    role = "menu",
    accessibilityLabel,
    children,
}: Props) {
    const [placement, setPlacement] = useState<Placement>();
    const menu = useRef<HTMLElement | null>(null);
    const element = anchor as HTMLElement;

    useLayoutEffect(() => {
        let frame = 0;
        const place = () => {
            const rect = element.getBoundingClientRect();
            setPlacement((current) =>
                current?.top === rect.bottom &&
                current.left === rect.left &&
                current.width === rect.width
                    ? current
                    : {
                          top: rect.bottom,
                          left: rect.left,
                          width: rect.width,
                          maxHeight: Math.max(
                              MIN_HEIGHT,
                              window.innerHeight - rect.bottom - EDGE
                          ),
                      }
            );
            // every frame, not just on scroll: a banner that loads late moves the field too
            frame = requestAnimationFrame(place);
        };
        place();
        return () => cancelAnimationFrame(frame);
    }, [element]);

    // the keyboard: focus goes to the first item as the list opens and back to the field when it closes
    useEffect(() => {
        focusFirstMenuItem(menu.current);
        return () => {
            if (menu.current?.contains(document.activeElement)) element.focus();
        };
    }, [element]);

    useEffect(() => {
        const pressed = (event: MouseEvent) => {
            const target = event.target as Node;
            if (!element.contains(target) && !menu.current?.contains(target))
                onClose();
        };
        document.addEventListener("mousedown", pressed);
        return () => document.removeEventListener("mousedown", pressed);
    }, [element, onClose]);

    return createPortal(
        <View
            ref={(node) => {
                menu.current = node as unknown as HTMLElement | null;
                webClassRef({ "v-menu": true, "v-overlay--active": true })(
                    node
                );
            }}
            {...({
                role,
                "aria-label": accessibilityLabel,
                onKeyDown: (event: Parameters<typeof menuKeyDown>[0]) =>
                    menuKeyDown(event, onClose),
            } as object)}
            className="bg-surface rounded shadow-lg border border-border-light py-1"
            style={{
                position: "fixed",
                zIndex: 10000,
                ...placement,
                opacity: placement ? 1 : 0,
            }}
        >
            {children}
        </View>,
        document.body
    );
}
