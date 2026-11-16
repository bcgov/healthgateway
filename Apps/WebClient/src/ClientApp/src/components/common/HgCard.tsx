import { useId } from "react";
import { Platform, Pressable, View } from "react-native";

import { usePressHighlight } from "@/hooks/usePressHighlight";
import { useThemeColors } from "@/hooks/useTheme";
import { cn } from "@/utils/cn";

import { AppText } from "./AppText";
import { elevation1 } from "./elevation";
import { HgMenu } from "./HgMenu";
import { Icon } from "./Icon";

interface Props {
    title?: string;
    /** the card's name for the screen reader when it has no title (the health connect and other sources cards) */
    accessibilityLabel?: string;
    icon?: React.ReactNode;
    onPress: () => void;
    onRemove?: () => void;
    /** #action-icon slot (the dependent dashboard's chevron) */
    actionIcon?: React.ReactNode;
    testID?: string;
    /** body text: shown under the title on the full card, read as the screen reader's hint on the compact one */
    description?: string;
    /** icon top left, title bottom left with an arrow, no body (Home's two-column phone grid) */
    compact?: boolean;
    children?: React.ReactNode;
    className?: string;
}

/**
 * Port of HgCardComponent.vue as used on Home, Other record sources and the dependent dashboard: elevated card
 * (elevation 1, no border, since healthgateway d7ab5c429), icon + title row, optional "⋮" menu with Remove, body
 * text, hover = surfaceHover (the web client keeps the hover for clickable cards only, and this card always is). A press
 * shades it too, as the web client's `:hover` does on a tap in a mobile browser.
 * `compact` is the phone tile of Home's two-column grid (a stated deviation, after the earlier iOS app): icon top left,
 * title and arrow along the bottom, the ⋮ menu in the top right corner, and no body; the description becomes the hint.
 */
export function HgCard({
    title,
    accessibilityLabel,
    icon,
    onPress,
    onRemove,
    actionIcon,
    testID,
    description,
    compact = false,
    children,
    className = "",
}: Props) {
    const colors = useThemeColors();
    const { highlighted, handlers } = usePressHighlight();
    const descriptionId = `${useId()}-description`;
    const name = title ?? accessibilityLabel;
    const web = Platform.OS === "web";
    const decorative = {
        accessibilityElementsHidden: true,
        importantForAccessibility: "no-hide-descendants" as const,
        ...(web ? { "aria-hidden": true } : {}),
    };
    const menu = onRemove ? (
        <HgMenu
            // v-menu location="bottom end": right edges aligned, as wide as "Remove" needs
            align="end"
            items={[
                {
                    title: "Remove",
                    testID: "remove-quick-link-button",
                    onPress: onRemove,
                },
            ]}
        >
            {(open, isOpen) => (
                <Pressable
                    testID="card-menu-button"
                    accessibilityRole="button"
                    accessibilityLabel="Menu"
                    aria-expanded={isOpen}
                    {...(web ? ({ "aria-haspopup": "menu" } as object) : {})}
                    hitSlop={8}
                    onPress={(e) => {
                        // the card itself is pressable
                        e.stopPropagation();
                        open();
                    }}
                    // v-icon-btn: 40 by 40, a 24px v-icon, in its own column 16px after the title
                    className={cn(
                        "w-10 h-10 items-center justify-center",
                        !compact && "ml-4"
                    )}
                >
                    <Icon
                        name="ellipsis-vertical"
                        size={24}
                        color={colors["bc-grey"]}
                        fixedWidth
                    />
                </Pressable>
            )}
        </HgMenu>
    ) : null;

    if (compact) {
        return (
            <Pressable
                testID={testID}
                accessible={false}
                tabIndex={-1}
                onPress={onPress}
                {...handlers}
                style={elevation1}
                className={cn(
                    "grow rounded",
                    highlighted ? "bg-surface-hover" : "bg-surface",
                    "web:cursor-pointer",
                    className
                )}
            >
                {/* the whole tile is the one control: icon top left, the title and an arrow along the bottom */}
                <Pressable
                    accessibilityRole="button"
                    accessibilityLabel={name}
                    // the description follows the name: a hint on native; on web, where react-native-web drops the
                    // hint, the hidden text below describes the button
                    accessibilityHint={description}
                    {...(web && description
                        ? ({ "aria-describedby": descriptionId } as object)
                        : {})}
                    onPress={onPress}
                    onPressIn={handlers.onPressIn}
                    onPressOut={handlers.onPressOut}
                    className="grow min-h-[140px] p-4"
                >
                    {icon ? (
                        // leaves the top right corner to the ⋮ button
                        <View className="self-start mr-10" {...decorative}>
                            {icon}
                        </View>
                    ) : null}
                    <View className="grow min-h-4" />
                    <View className="flex-row items-end">
                        <AppText
                            testID="card-button-title"
                            numberOfLines={3}
                            className="flex-1 text-[17px] leading-[22px] tracking-normal"
                        >
                            {name}
                        </AppText>
                        <View className="ml-2 mb-0.5" {...decorative}>
                            <Icon
                                name="arrow-right"
                                size={16}
                                color={colors.primary}
                            />
                        </View>
                    </View>
                </Pressable>
                {web && description ? (
                    // display: none still describes: aria-describedby reads hidden content
                    <AppText nativeID={descriptionId} className="hidden">
                        {description}
                    </AppText>
                ) : null}
                {menu ? (
                    <View className="absolute top-1 right-1">{menu}</View>
                ) : null}
            </Pressable>
        );
    }

    return (
        // the whole card takes a press (mouse, touch), but the control the keyboard and the screen reader get is the
        // title row inside: a button containing the ⋮ button or a link would hide them from assistive technology
        <Pressable
            testID={testID}
            accessible={false}
            tabIndex={-1}
            onPress={onPress}
            {...handlers}
            style={elevation1}
            className={cn(
                "grow rounded",
                highlighted ? "bg-surface-hover" : "bg-surface",
                "web:cursor-pointer",
                className
            )}
        >
            {/* v-card-item: 10px top, 16px sides; v-row density comfortable: 16px between its columns */}
            <View className="flex-row items-center px-4 pt-2.5">
                <Pressable
                    accessibilityRole="button"
                    accessibilityLabel={name}
                    onPress={onPress}
                    // a press here is the card's own, so it shades the card
                    onPressIn={handlers.onPressIn}
                    onPressOut={handlers.onPressOut}
                    className="flex-1 flex-row items-center"
                >
                    {icon ? (
                        <View
                            className="mr-4 py-2"
                            // the icon is decorative: the name says what the card is
                            accessibilityElementsHidden
                            importantForAccessibility="no-hide-descendants"
                            {...(web ? { "aria-hidden": true } : {})}
                        >
                            {icon}
                        </View>
                    ) : null}
                    {title ? (
                        <AppText
                            testID="card-button-title"
                            className="flex-1 text-[22px] leading-7 tracking-normal py-2"
                        >
                            {title}
                        </AppText>
                    ) : (
                        <View className="flex-1" />
                    )}
                </Pressable>
                {actionIcon}
                {menu}
            </View>
            <View className="px-4 pb-4 pt-2">
                {description ? <AppText>{description}</AppText> : null}
                {children}
            </View>
        </Pressable>
    );
}
