import { createElement, useId } from "react";
import { Platform, Pressable, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import { spaceToggleProps } from "@/utils/spaceToggle";

import { AppText } from "./AppText";
import { Icon } from "./Icon";
import { LiveRegion } from "./LiveRegion";

interface Props {
    checked: boolean;
    onChange: (checked: boolean) => void;
    label: string;
    /** the #label slot: richer content than `label`, which stays the accessible name */
    labelContent?: React.ReactNode;
    /** `checkbox-top`: the box aligned with the first line of a long label */
    alignTop?: boolean;
    disabled?: boolean;
    errorMessages?: string[];
    testID?: string;
    className?: string;
}

/**
 * Port of v-checkbox (color primary) with Vuetify's 40px row and error messages below. On web it also holds a real
 * hidden `<input type="checkbox">`, because Cypress drives it with `[data-testid=…] input` and `.check()`.
 */
export function HgCheckbox({
    checked,
    onChange,
    label,
    labelContent,
    alignTop = false,
    disabled,
    errorMessages = [],
    testID,
    className = "",
}: Props) {
    const colors = useThemeColors();
    const errorsId = `${useId()}-errors`;
    const hasError = errorMessages.length > 0;
    return (
        <View className={className}>
            <Pressable
                testID={testID}
                accessibilityRole="checkbox"
                aria-checked={checked}
                aria-disabled={!!disabled}
                accessibilityLabel={label}
                // the error messages describe the box (web) or follow its name (native)
                accessibilityHint={
                    hasError ? errorMessages.join(". ") : undefined
                }
                {...(Platform.OS === "web" && hasError
                    ? ({
                          "aria-invalid": true,
                          "aria-describedby": errorsId,
                      } as object)
                    : {})}
                disabled={disabled}
                onPress={() => onChange(!checked)}
                {...spaceToggleProps(() => onChange(!checked), disabled)}
                className={`flex-row min-h-[40px] ${alignTop ? "items-start" : "items-center"} ${disabled ? "opacity-50" : ""}`}
            >
                <View className="w-10 items-center">
                    <Icon
                        name={checked ? "square-check" : "square"}
                        solid={checked}
                        size={22}
                        color={checked ? colors.primary : colors["bc-grey"]}
                    />
                </View>
                {Platform.OS === "web"
                    ? createElement("input", {
                          type: "checkbox",
                          checked,
                          disabled,
                          "aria-hidden": true,
                          tabIndex: -1,
                          // over the box, as Vuetify's is: `.check()` and `.uncheck()` click it
                          style: {
                              position: "absolute",
                              top: 0,
                              left: 0,
                              opacity: 0,
                              width: 40,
                              height: "100%",
                              margin: 0,
                              cursor: "inherit",
                          },
                          // the row's own press must not toggle it a second time
                          onClick: (e: { stopPropagation: () => void }) =>
                              e.stopPropagation(),
                          onChange: () => onChange(!checked),
                      })
                    : null}
                <View className="flex-1">
                    {labelContent ?? <AppText>{label}</AppText>}
                </View>
            </Pressable>
            {hasError ? (
                <LiveRegion politeness="polite" nativeID={errorsId}>
                    {errorMessages.map((m) => (
                        <AppText
                            key={m}
                            variant="body-medium"
                            className="text-error ml-10"
                        >
                            {m}
                        </AppText>
                    ))}
                </LiveRegion>
            ) : null}
        </View>
    );
}
