import { jest } from "@jest/globals";
import {
    cloneElement,
    createElement,
    isValidElement,
    type ReactElement,
    type ReactNode,
} from "react";
import { Text } from "react-native";

/** The router every component test sees; inspect `router.push` / `router.replace`, set `pathname` / `params`. */
export const router = {
    push: jest.fn(),
    replace: jest.fn(),
    back: jest.fn(),
    navigate: jest.fn(),
    dismissTo: jest.fn(),
    setParams: jest.fn(),
};
export const routerState = {
    /** what `useIsFocused` reports (native tabs in the background: false) */
    focused: true,
    pathname: "/",
    params: {} as Record<string, string>,
};

export const useRouter = () => router;
export const usePathname = () => routerState.pathname;
export const useLocalSearchParams = () => routerState.params;
export const useSegments = () =>
    routerState.pathname.split("/").filter(Boolean);
export const useFocusEffect = () => undefined;
export const useIsFocused = () => routerState.focused;
export const Slot = ({ children }: { children?: ReactNode }) =>
    children ?? null;
export const Stack = ({ children }: { children?: ReactNode }) =>
    children ?? null;
/** expo-router Link: forwards its props (href, test ids) and navigates with the mocked router on press. */
export const Link = ({
    children,
    href,
    onPress,
    asChild,
    ...rest
}: {
    children?: ReactNode;
    href: string;
    onPress?: (e: unknown) => void;
    asChild?: boolean;
} & Record<string, unknown>) => {
    const handlePress = (e: unknown) => {
        let prevented = false;
        onPress?.({
            preventDefault: () => {
                prevented = true;
                (
                    e as { preventDefault?: () => void } | undefined
                )?.preventDefault?.();
            },
        });
        if (prevented) return;
        // like expo-router on web: in-app navigation instead of the browser following the anchor
        (e as { preventDefault?: () => void } | undefined)?.preventDefault?.();
        router.push(href);
    };
    if (asChild && isValidElement(children))
        return cloneElement(children as ReactElement<Record<string, unknown>>, {
            ...rest,
            href,
            onPress: handlePress,
        });
    return createElement(
        Text,
        { ...rest, href, onPress: handlePress } as Record<string, unknown>,
        children
    );
};

export function resetRouter(
    pathname = "/",
    params: Record<string, string> = {}
) {
    router.push.mockClear();
    router.replace.mockClear();
    router.back.mockClear();
    router.navigate.mockClear();
    router.dismissTo.mockClear();
    router.setParams.mockClear();
    routerState.pathname = pathname;
    routerState.params = params;
}
