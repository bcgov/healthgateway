import { Platform, Pressable, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import { cn } from "@/utils/cn";

import { AppText } from "./AppText";
import { Icon } from "./Icon";

interface Props {
    length: number;
    page: number;
    onChange: (page: number) => void;
    totalVisible?: number;
    density?: "default" | "comfortable";
    testID?: string;
}

const ELLIPSIS = "...";

function createRange(length: number, start: number): number[] {
    return Array.from({ length }, (_, i) => start + i);
}

/** Vuetify VPagination getRange(visible), start 1. */
export function paginationRange(
    length: number,
    page: number,
    visible: number
): (number | string)[] {
    if (length <= 0) return [];
    if (visible <= 0) return [];
    if (visible === 1) return [page];
    if (length <= visible) return createRange(length, 1);
    const even = visible % 2 === 0;
    const middle = even ? visible / 2 : Math.floor(visible / 2);
    const left = even ? middle : middle + 1;
    const right = length - middle;
    if (left - page >= 0)
        return [...createRange(Math.max(1, visible - 1), 1), ELLIPSIS, length];
    if (page - right >= (even ? 1 : 0)) {
        const rangeLength = visible - 1;
        return [
            1,
            ELLIPSIS,
            ...createRange(rangeLength, length - rangeLength + 1),
        ];
    }
    const rangeLength = Math.max(1, visible - 2);
    const rangeStart =
        rangeLength === 1 ? page : page - Math.ceil(rangeLength / 2) + 1;
    return [
        1,
        ELLIPSIS,
        ...createRange(rangeLength, rangeStart),
        ELLIPSIS,
        length,
    ];
}

/**
 * Port of Vuetify's v-pagination as the timeline uses it: previous / pages / next as round text buttons
 * (48px, 36px when comfortable), the current page tinted, ellipses for skipped ranges.
 */
export function HgPagination({
    length,
    page,
    onChange,
    totalVisible = 8,
    density = "default",
    testID,
}: Props) {
    const colors = useThemeColors();
    const size = density === "comfortable" ? 36 : 48;
    const button = (
        key: string | number,
        content: React.ReactNode,
        opts: {
            label: string;
            disabled?: boolean;
            active?: boolean;
            onPress?: () => void;
        }
    ) => (
        <Pressable
            key={key}
            accessibilityRole="button"
            accessibilityLabel={opts.label}
            aria-current={opts.active ? "page" : undefined}
            disabled={opts.disabled}
            onPress={opts.onPress}
            style={{ width: size, height: size, margin: 4.8 }}
            className={cn(
                "rounded-full items-center justify-center",
                opts.active
                    ? "bg-border-light"
                    : "hover:bg-surface-hover active:bg-surface-hover",
                opts.disabled && !opts.active && "opacity-40"
            )}
        >
            {content}
        </Pressable>
    );
    const items = paginationRange(length, page, totalVisible);
    return (
        <View
            testID={testID}
            // Android throws on a role it doesn't know, and "navigation" is web only
            accessibilityRole={Platform.OS === "web" ? "navigation" : undefined}
            accessibilityLabel="Pagination Navigation"
            className="flex-row flex-wrap justify-center items-center"
        >
            {button(
                "prev",
                <Icon
                    name="chevron-left"
                    size={16}
                    color={colors["bc-black"]}
                />,
                {
                    label: "Previous page",
                    disabled: page <= 1,
                    onPress: () => onChange(page - 1),
                }
            )}
            {items.map((item, index) =>
                typeof item === "string" ? (
                    // a gap in the pages, not a control (Vuetify draws a div): nothing for the reader
                    <View
                        key={`ellipsis-${index}`}
                        aria-hidden
                        style={{ width: size, height: size, margin: 4.8 }}
                        className="items-center justify-center"
                    >
                        <AppText>{item}</AppText>
                    </View>
                ) : (
                    button(item, <AppText>{item}</AppText>, {
                        label:
                            item === page
                                ? `Page ${item}, Current page`
                                : `Go to page ${item}`,
                        active: item === page,
                        disabled: length < 2,
                        onPress: () => onChange(item),
                    })
                )
            )}
            {button(
                "next",
                <Icon
                    name="chevron-right"
                    size={16}
                    color={colors["bc-black"]}
                />,
                {
                    label: "Next page",
                    disabled: page >= length,
                    onPress: () => onChange(page + 1),
                }
            )}
        </View>
    );
}
