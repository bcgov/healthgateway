import { useId } from "react";
import {
    KeyboardAvoidingView,
    Platform,
    Pressable,
    ScrollView,
    View,
} from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import { cn } from "@/utils/cn";

import { AppModal } from "./AppModal";
import { AppText } from "./AppText";
import { Backdrop } from "./Backdrop";
import { CardActions } from "./HgButton";
import { Icon } from "./Icon";
import { ModalSafeArea } from "./ModalSafeArea";

interface Props {
    visible: boolean;
    title: string;
    onRequestClose?: () => void;
    children: React.ReactNode;
    actions?: React.ReactNode;
    testID?: string;
    /**
     * Header style. "primary": compact v-toolbar in the primary colour with a close button
     * (IdleDialogComponent.vue). "toolbar": default-height transparent v-toolbar with a close button
     * (RecommendationsDialogComponent.vue). Unset: a plain title.
     */
    tone?: "primary" | "toolbar" | "accent";
    /** an icon before the title (the note dialog's "edit") */
    titleIcon?: string;
    /** v-toolbar density="compact" on the "toolbar" tone (FeedbackComponent.vue) */
    denseToolbar?: boolean;
    /** v-dialog fullscreen (the web client passes layoutStore.isMobile) */
    fullscreen?: boolean;
    /** HTML id of the title bar's close button */
    closeNativeID?: string;
    /** v-dialog persistent: tapping outside does not close it */
    persistent?: boolean;
    /** centres a plain title */
    centerTitle?: boolean;
    /** v-card max-width (default 520) */
    maxWidth?: number;
    /** test id of the title bar's close button (tones "primary" and "toolbar") */
    closeTestID?: string;
    /** v-card-actions alignment (default end) */
    actionsAlign?: "end" | "center";
    /** a divider above the actions (`border-t-sm`, default true) */
    actionsDivider?: boolean;
    /**
     * Native: drawn in place over the screen (fill the parent) instead of in its own modal window. For a dialog that
     * must show whatever else is open: iOS presents only one modal window at a time, so a second one opened over
     * another may never appear (Update required). The caller places it and keeps the rest from screen readers.
     */
    inline?: boolean;
}

/**
 * Centred modal dialog (v-dialog + v-card). The card never grows past the space left by the keyboard:
 * title and actions stay fixed, the body scrolls, so Submit / Cancel remain reachable while typing.
 */
export function Dialog({
    visible,
    title,
    onRequestClose,
    children,
    actions,
    testID,
    tone,
    persistent,
    centerTitle = false,
    maxWidth = 520,
    closeTestID,
    closeNativeID,
    actionsAlign = "end",
    actionsDivider = true,
    fullscreen = false,
    titleIcon,
    denseToolbar = false,
    inline = false,
}: Props) {
    const colors = useThemeColors();
    // the dialog is named by its title (WCAG 4.1.2); react-native-web puts the attribute on its role="dialog" element
    const titleId = useId();
    const toolbar =
        tone === "primary" || tone === "toolbar" || tone === "accent";
    const onBar =
        tone === "primary"
            ? colors["on-primary"]
            : tone === "accent"
              ? colors["on-accent"]
              : colors["bc-black"];
    const content = (
        <>
            {/* iOS does not resize the window for the keyboard; Android (adjustResize) and web need no help */}
            <KeyboardAvoidingView
                className="flex-1"
                behavior={Platform.OS === "ios" ? "padding" : undefined}
            >
                <Backdrop
                    className={
                        fullscreen
                            ? "flex-1 bg-black/40"
                            : "flex-1 bg-black/40 items-center justify-center p-4"
                    }
                    onPress={persistent ? undefined : onRequestClose}
                >
                    {/* the card: a plain View, so it is no focus stop, which swallows the press (the responder on
                        native, the click on web) so a press inside is not one on the scrim */}
                    <View
                        testID={testID}
                        className={
                            fullscreen
                                ? "flex-1 w-full bg-surface overflow-hidden"
                                : "w-full max-h-full bg-surface rounded-lg overflow-hidden"
                        }
                        style={fullscreen ? undefined : { maxWidth }}
                        onStartShouldSetResponder={() => true}
                        {...({
                            onClick: (e: Event) => e.stopPropagation(),
                        } as object)}
                    >
                        <SafeAreaIfFullscreen fullscreen={fullscreen}>
                            {toolbar ? (
                                <View
                                    className={
                                        tone === "primary"
                                            ? "flex-row items-center bg-primary pl-4 pr-1 min-h-[48px]"
                                            : tone === "accent"
                                              ? "flex-row items-center bg-accent pl-4 pr-1 min-h-[48px]"
                                              : denseToolbar
                                                ? "flex-row items-center pl-5 pr-2 min-h-[48px]"
                                                : "flex-row items-center pl-5 pr-2 min-h-[64px]"
                                    }
                                >
                                    {titleIcon ? (
                                        <View className="mr-2">
                                            <Icon
                                                name={titleIcon}
                                                size={20}
                                                color={onBar}
                                            />
                                        </View>
                                    ) : null}
                                    <AppText
                                        variant="title-large"
                                        className={
                                            tone === "primary"
                                                ? "flex-1 text-on-primary"
                                                : tone === "accent"
                                                  ? "flex-1 text-on-accent"
                                                  : "flex-1"
                                        }
                                        accessibilityRole="header"
                                        aria-level={2}
                                        nativeID={titleId}
                                    >
                                        {title}
                                    </AppText>
                                    <Pressable
                                        testID={closeTestID}
                                        nativeID={closeNativeID}
                                        accessibilityRole="button"
                                        accessibilityLabel="Close"
                                        onPress={onRequestClose}
                                        className="w-11 h-11 items-center justify-center"
                                    >
                                        <Icon
                                            name="xmark"
                                            size={20}
                                            color={onBar}
                                        />
                                    </Pressable>
                                </View>
                            ) : (
                                <View className="px-6 pt-5 pb-3">
                                    <AppText
                                        variant="h3"
                                        accessibilityRole="header"
                                        aria-level={2}
                                        nativeID={titleId}
                                        className={
                                            centerTitle
                                                ? "text-center"
                                                : undefined
                                        }
                                    >
                                        {title}
                                    </AppText>
                                </View>
                            )}
                            <ScrollView
                                // v-card-text: pa-4 under the toolbar variant; the plain variant keeps its 24px sides
                                className={cn(
                                    toolbar ? "shrink px-4" : "shrink px-6",
                                    fullscreen && "flex-1"
                                )}
                                contentContainerClassName={
                                    toolbar ? "py-4" : "pb-4"
                                }
                                keyboardShouldPersistTaps="handled"
                            >
                                {children}
                            </ScrollView>
                            {actions ? (
                                <View
                                    className={`flex-row ${actionsAlign === "center" ? "justify-center" : "justify-end"} gap-2 py-4 ${actionsDivider ? "border-t border-border-divider" : ""} ${
                                        toolbar ? "px-4" : "px-6"
                                    }`}
                                >
                                    <CardActions>{actions}</CardActions>
                                </View>
                            ) : null}
                        </SafeAreaIfFullscreen>
                    </View>
                </Backdrop>
            </KeyboardAvoidingView>
        </>
    );
    if (inline)
        return visible ? (
            <View
                className="absolute inset-0"
                // VoiceOver stays inside, as in a modal window
                accessibilityViewIsModal
            >
                {content}
            </View>
        ) : null;
    return (
        <AppModal
            // iOS: a modal supports portrait only unless told otherwise, which would rotate a landscape screen
            supportedOrientations={["portrait", "landscape"]}
            transparent
            visible={visible}
            animationType="fade"
            onRequestClose={onRequestClose}
            {...(Platform.OS === "web" ? { "aria-labelledby": titleId } : {})}
        >
            {content}
        </AppModal>
    );
}

function SafeAreaIfFullscreen({
    fullscreen,
    children,
}: {
    fullscreen: boolean;
    children: React.ReactNode;
}) {
    return fullscreen ? <ModalSafeArea>{children}</ModalSafeArea> : children;
}
