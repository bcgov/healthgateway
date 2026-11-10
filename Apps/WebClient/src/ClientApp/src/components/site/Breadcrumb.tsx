import { Link, useRouter } from "expo-router";
import { Platform, Pressable, View } from "react-native";

import { AppText } from "@/components/common";
import { webClassRef } from "@/components/common/webClass";
import { Path } from "@/constants/path";
import {
    Action,
    Destination,
    InternalUrl,
    Origin,
    Text,
} from "@/constants/tracking";
import type { BreadcrumbItem } from "@/models/breadcrumbItem";
import { toHref } from "@/navigation/href";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import {
    selectIsValidIdentityProvider,
    useUserStore,
} from "@/stores/user.store";
import { cn } from "@/utils/cn";

const home: BreadcrumbItem = {
    text: "Home",
    to: Path.Home,
    testId: "breadcrumb-home",
};

/** Port of BreadcrumbComponent.vue. */
export function Breadcrumb({ items = [] }: { items?: BreadcrumbItem[] }) {
    const router = useRouter();
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const isValidIdp = useUserStore(selectIsValidIdentityProvider);
    if (!isAuthenticated || !isValidIdp) return null;

    function follow(item: BreadcrumbItem) {
        if (!item.to) return;
        if (item.testId === "breadcrumb-home" || item.to === Path.Home) {
            services.tracking.trackEvent({
                action: Action.InternalLink,
                text: Text.HomeBreadcrumb,
                origin: Origin.Breadcrumb,
                destination: Destination.Home,
                url: InternalUrl.Home,
            });
        }
        router.push(toHref(item.to));
    }

    const all = [home, ...items];
    return (
        <View
            testID="breadcrumbs"
            // a <nav> on web, so the label is allowed and the landmark is listed; Android has no such role
            accessibilityRole={Platform.OS === "web" ? "navigation" : undefined}
            accessibilityLabel="Breadcrumb Nav"
            className="flex-row flex-wrap items-center mb-4"
        >
            {all.map((item, i) => (
                <View
                    key={`${i}-${item.text}`}
                    className="flex-row items-center"
                >
                    {item.active || !item.to ? (
                        // the current page: text, not a link (v-breadcrumbs-item disabled), named as such
                        <View
                            ref={webClassRef({
                                "v-breadcrumbs-item--active": !!item.active,
                            })}
                            testID={item.testId}
                            {...(item.active
                                ? ({ "aria-current": "page" } as object)
                                : {})}
                        >
                            <AppText
                                // v-breadcrumbs: on-surface text, the current page at the disabled opacity
                                className={cn(
                                    "tracking-normal",
                                    item.active && "opacity-40"
                                )}
                            >
                                {item.text}
                            </AppText>
                        </View>
                    ) : (
                        // a real link on web, so the keyboard can follow it; the press tracks and navigates itself
                        <Link
                            href={toHref(item.to)}
                            asChild
                            onPress={(event) => {
                                event.preventDefault();
                                follow(item);
                            }}
                        >
                            <Pressable
                                testID={item.testId}
                                accessibilityRole="link"
                            >
                                <AppText className="tracking-normal">
                                    {item.text}
                                </AppText>
                            </Pressable>
                        </Link>
                    )}
                    {i < all.length - 1 ? (
                        <AppText className="mx-2 tracking-normal">/</AppText>
                    ) : null}
                </View>
            ))}
        </View>
    );
}
