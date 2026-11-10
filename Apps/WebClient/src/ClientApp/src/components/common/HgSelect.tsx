import { useState } from "react";
import {
    Platform,
    Pressable,
    ScrollView,
    useWindowDimensions,
    View,
} from "react-native";

import { useThemeColors } from "@/hooks/useTheme";

import { AppModal } from "./AppModal";
import { AppText } from "./AppText";
import { Backdrop } from "./Backdrop";
import { elevation1 } from "./elevation";
import { Icon } from "./Icon";
import { webClassRef } from "./webClass";
import { WebDropdown } from "./WebDropdown";

export interface HgSelectOption {
    title: string;
    value: string;
}

interface BaseProps {
    items: HgSelectOption[];
    label?: string;
    /** shown under an empty field's label as a placeholder (v-select `hint`) */
    hint?: string;
    disabled?: boolean;
    testID?: string;
    nativeID?: string;
    className?: string;
}

interface SingleProps extends BaseProps {
    multiple?: false;
    value: string | undefined;
    onChange: (value: string) => void;
}

interface MultipleProps extends BaseProps {
    multiple: true;
    value: string[];
    onChange: (value: string[]) => void;
}

/**
 * Port of Vuetify's v-select as the reports page uses it: a field that opens a v-list under itself. The list keeps
 * Vuetify's classes on web for `cy.vSelect` (`.v-overlay--active.v-menu .v-list-item`), and no backdrop
 * (`WebDropdown`). A multi-select stays open on native; on web it closes after each pick, so it can't cover the
 * buttons under it.
 */
export function HgSelect(props: SingleProps | MultipleProps) {
    const { items, label, hint, disabled, testID, nativeID, className } = props;
    const colors = useThemeColors();
    const { width: windowWidth, height: windowHeight } = useWindowDimensions();
    const [open, setOpen] = useState(false);
    const [anchor, setAnchor] = useState<{
        top: number;
        left: number;
        width: number;
    }>();
    const [activator, setActivator] = useState<React.ComponentRef<
        typeof View
    > | null>(null);

    const selected = props.multiple
        ? props.value
        : props.value !== undefined
          ? [props.value]
          : [];
    const display = selected
        .map((v) => items.find((i) => i.value === v)?.title ?? v)
        .join(", ");

    const show = () => {
        if (Platform.OS === "web") {
            setOpen(true);
            return;
        }
        setAnchor(undefined);
        setOpen(true);
        activator?.measureInWindow((x, y, w, h) =>
            setAnchor({
                top: y + h,
                left: Math.max(8, Math.min(x, windowWidth - w - 8)),
                width: w,
            })
        );
    };

    const pick = (value: string) => {
        if (props.multiple) {
            props.onChange(
                props.value.includes(value)
                    ? props.value.filter((v) => v !== value)
                    : [...props.value, value]
            );
            if (Platform.OS === "web") setOpen(false);
        } else {
            props.onChange(value);
            setOpen(false);
        }
    };

    const list = (
        <ScrollView keyboardShouldPersistTaps="handled">
            {items.map((item) => {
                const isSelected = selected.includes(item.value);
                return (
                    <Pressable
                        ref={webClassRef({
                            "v-list-item": true,
                        })}
                        key={item.value}
                        // web: an option in a listbox (`aria-selected` belongs there); native: Android knows menuitem
                        accessibilityRole={
                            Platform.OS === "web" ? undefined : "menuitem"
                        }
                        {...(Platform.OS === "web"
                            ? ({ role: "option" } as object)
                            : {})}
                        aria-selected={isSelected}
                        onPress={() => pick(item.value)}
                        className={`flex-row items-center px-4 py-3 hover:bg-surface-hover active:bg-surface-hover ${
                            isSelected ? "bg-surface-hover" : ""
                        }`}
                    >
                        {props.multiple ? (
                            <View className="mr-3">
                                <Icon
                                    name={
                                        isSelected ? "square-check" : "square"
                                    }
                                    solid={isSelected}
                                    size={18}
                                    color={
                                        isSelected
                                            ? colors.primary
                                            : colors["bc-grey"]
                                    }
                                />
                            </View>
                        ) : null}
                        <AppText className="flex-1">{item.title}</AppText>
                    </Pressable>
                );
            })}
        </ScrollView>
    );

    return (
        <View className={className}>
            <View ref={setActivator} collapsable={false}>
                <Pressable
                    testID={testID}
                    nativeID={nativeID}
                    accessibilityRole="button"
                    accessibilityLabel={label ?? hint}
                    aria-disabled={!!disabled}
                    // it opens a list to pick from (the list's own role is WP3)
                    aria-expanded={open}
                    {...(Platform.OS === "web"
                        ? ({ "aria-haspopup": "listbox" } as object)
                        : {})}
                    disabled={disabled}
                    onPress={show}
                    style={elevation1}
                    className={`flex-row items-center bg-surface rounded min-h-[40px] px-3 py-1 ${
                        disabled ? "opacity-50" : ""
                    }`}
                >
                    <View className="flex-1">
                        {label && display ? (
                            <AppText className="text-body-small text-bc-grey">
                                {label}
                            </AppText>
                        ) : null}
                        <AppText className={display ? "" : "text-bc-grey"}>
                            {display || label || hint || ""}
                        </AppText>
                    </View>
                    <Icon
                        name="caret-down"
                        size={16}
                        color={colors["bc-grey"]}
                    />
                </Pressable>
            </View>
            {Platform.OS === "web" ? (
                open && activator ? (
                    <WebDropdown
                        role="listbox"
                        accessibilityLabel={label ?? hint}
                        anchor={activator}
                        onClose={() => setOpen(false)}
                    >
                        {list}
                    </WebDropdown>
                ) : null
            ) : (
                <AppModal
                    supportedOrientations={["portrait", "landscape"]}
                    transparent
                    visible={open}
                    animationType="fade"
                    onRequestClose={() => setOpen(false)}
                >
                    <Backdrop className="flex-1" onPress={() => setOpen(false)}>
                        <View
                            accessibilityRole="menu"
                            className="absolute bg-surface rounded shadow-lg border border-border-light py-1"
                            style={{
                                top: anchor?.top ?? 0,
                                left: anchor?.left ?? 0,
                                width: anchor?.width,
                                maxHeight: Math.max(
                                    160,
                                    windowHeight - (anchor?.top ?? 0) - 16
                                ),
                                opacity: anchor ? 1 : 0,
                            }}
                        >
                            {list}
                        </View>
                    </Backdrop>
                </AppModal>
            )}
        </View>
    );
}
