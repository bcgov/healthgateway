import { useEffect, useId } from "react";
import {
    Platform,
    Pressable,
    TextInput,
    type TextInputProps,
    View,
} from "react-native";

import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import { logger } from "@/services/logger";
import { cn } from "@/utils/cn";

import { AppText } from "./AppText";
import { elevation1 } from "./elevation";
import { Icon } from "./Icon";
import { LiveRegion } from "./LiveRegion";
import { webClassRef } from "./webClass";

interface Props extends TextInputProps {
    label?: string;
    errorMessages?: string[];
    className?: string;
    /** v-text-field `clearable`: an × that empties the field while it is editable and has a value */
    clearable?: boolean;
    /** v-text-field #append-inner slot (e.g. a validation spinner) */
    appendInner?: React.ReactNode;
    /** v-text-field prepend-inner-icon (e.g. the search icon) */
    prependInner?: React.ReactNode;
    /** Vuetify `counter`: "length/maxLength" under the field, right-aligned */
    counter?: boolean;
    /** v-text-field `label`: small, inside the field above the value, instead of above the field */
    floatingLabel?: boolean;
    /** v-text-field density="compact": 40px */
    compact?: boolean;
}

/**
 * Solo-style text field like the web client's VTextField default. The test id sits where the web client's
 * `data-testid` does: on the field wrapper on web (selectors such as `[data-testid=email-input] input`), on the
 * input itself on native (Maestro and the native tests type into it).
 *
 * Accessibility: the input's name is `accessibilityLabel`, else `label`, else the placeholder (WCAG 3.3.2 wants a
 * name on every field; the web client shows some fields with a placeholder only, which stays for parity). Error
 * messages and the counter describe the input (`aria-describedby`, `aria-invalid` on web; the hint on native) and a
 * new error is announced without moving focus (WCAG 3.3.1, 4.1.3). `autoComplete` defaults to off; callers say what
 * a field collects (WCAG 1.3.5).
 */
export function HgTextField({
    label,
    errorMessages = [],
    className = "",
    clearable,
    appendInner,
    prependInner,
    counter = false,
    compact = false,
    floatingLabel = false,
    testID,
    autoComplete = "off",
    ...rest
}: Props) {
    const colors = useThemeColors();
    // dark mode: the elevation shadow is lost on a dark surface, so the field box gets an outline (WCAG 1.4.11);
    // light mode keeps the web client's shadow-only box (owner decision, docs/accessibility-plan.md section 5)
    const dark = useColorScheme() === "dark";
    const hasError = errorMessages.length > 0;
    const web = Platform.OS === "web";
    const editable = rest.editable !== false;
    const showClear =
        clearable && editable && Boolean(rest.value) && !!rest.onChangeText;
    const name = rest.accessibilityLabel ?? label ?? rest.placeholder;
    useEffect(() => {
        if (__DEV__ && !name)
            logger.warn(
                "HgTextField without an accessible name: give it a label, accessibilityLabel or placeholder"
            );
    }, [name]);
    const id = useId();
    const errorsId = `${id}-errors`;
    const counterId = `${id}-counter`;
    const describedBy =
        [hasError ? errorsId : "", counter ? counterId : ""]
            .filter(Boolean)
            .join(" ") || undefined;
    const errorHint = hasError ? errorMessages.join(". ") : undefined;
    return (
        <View
            ref={webClassRef({ "v-input--error": hasError })}
            className={cn("mb-2", className)}
            testID={web ? testID : undefined}
        >
            {label && !floatingLabel ? (
                <AppText variant="label" className="mb-1">
                    {label}
                </AppText>
            ) : null}
            <View
                // v-field variant solo: elevated, with an outline only for an error
                style={hasError ? undefined : elevation1}
                className={cn(
                    "flex-row items-center bg-surface rounded",
                    compact ? "min-h-[40px]" : "min-h-[48px]",
                    hasError
                        ? "border border-error"
                        : dark && "border border-bc-grey"
                )}
            >
                {prependInner ? (
                    <View className="ml-3">{prependInner}</View>
                ) : null}
                <View className="flex-1 min-w-0">
                    {label && floatingLabel ? (
                        <AppText className="px-3 pt-1.5 text-[12px] leading-4 tracking-normal text-bc-grey">
                            {label}
                        </AppText>
                    ) : null}
                    <TextInput
                        placeholderTextColor={colors["bc-grey"]}
                        {...rest}
                        accessibilityLabel={name}
                        // native: the error text follows the name; web: the messages describe the input
                        accessibilityHint={
                            errorHint
                                ? [rest.accessibilityHint, errorHint]
                                      .filter(Boolean)
                                      .join(". ")
                                : rest.accessibilityHint
                        }
                        autoComplete={autoComplete}
                        {...(web
                            ? ({
                                  "aria-invalid": hasError || undefined,
                                  "aria-describedby": describedBy,
                              } as object)
                            : {})}
                        testID={web ? undefined : testID}
                        className={cn(
                            "flex-1 min-w-0 font-sans text-body-large text-bc-black px-3",
                            floatingLabel
                                ? "min-h-[28px]"
                                : compact
                                  ? "min-h-[38px]"
                                  : "min-h-[46px]",
                            rest.multiline && "py-3"
                        )}
                        textAlignVertical={rest.multiline ? "top" : undefined}
                    />
                </View>
                {appendInner ? (
                    <View className="mr-2">{appendInner}</View>
                ) : null}
                {showClear ? (
                    <Pressable
                        accessibilityRole="button"
                        accessibilityLabel={`Clear ${name ?? ""}`.trim()}
                        hitSlop={8}
                        onPress={() => rest.onChangeText?.("")}
                        // a 24px box around the 18px glyph: the web has no hitSlop (WCAG 2.5.8)
                        className="mr-3 min-w-6 min-h-6 items-center justify-center"
                    >
                        <Icon
                            name="circle-xmark"
                            size={18}
                            color={colors["bc-grey"]}
                        />
                    </Pressable>
                ) : null}
            </View>
            {errorMessages.length > 0 || counter ? (
                <View className="flex-row items-start">
                    {/* a new message is announced as it appears; the region stays so a later one is too */}
                    <LiveRegion
                        politeness="polite"
                        nativeID={errorsId}
                        className="flex-1"
                    >
                        {errorMessages.map((m) => (
                            <View
                                ref={webClassRef({
                                    "v-messages__message": true,
                                })}
                                key={m}
                            >
                                <AppText
                                    variant="body-medium"
                                    className="text-error mt-1"
                                >
                                    {m}
                                </AppText>
                            </View>
                        ))}
                    </LiveRegion>
                    {counter ? (
                        <AppText
                            nativeID={counterId}
                            variant="body-medium"
                            className="text-bc-grey mt-1 ml-2"
                        >
                            {`${String(rest.value ?? "").length}${rest.maxLength ? `/${rest.maxLength}` : ""}`}
                        </AppText>
                    ) : null}
                </View>
            ) : null}
        </View>
    );
}
