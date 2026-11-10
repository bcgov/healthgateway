import { useLocalSearchParams } from "expo-router";
import { useEffect, useState } from "react";
import { Image, type ImageSourcePropType, View } from "react-native";

import { AppText, Dialog, HgButton } from "@/components/common";
import { Path } from "@/constants/path";
import { useBreakpoint } from "@/hooks/useBreakpoint";

const RegistrationImage =
    require("@/assets/images/tour/at-registration-web.png") as ImageSourcePropType;

/**
 * Port of AppTourComponent.vue, web only: the "Thank you for registering!" slide shown after registration, the only slide
 * the web client still has (AB#16942). Native has its first-launch tutorial instead. HeaderComponent.vue opens it from
 * `?registration=success` and clears the query, as this does.
 */
export function AppTourDialog() {
    const { mdAndUp } = useBreakpoint();
    const { registration } = useLocalSearchParams<{ registration?: string }>();
    const requested = registration === "success";
    const [wasRequested, setWasRequested] = useState(false);
    const [open, setOpen] = useState(false);
    if (requested !== wasRequested) {
        setWasRequested(requested);
        if (requested) setOpen(true);
    }

    // Clears the query from the address bar so a reload doesn't reopen it. Not through the router: replacing the route
    // would remount Home and this dialog with it, and setParams leaves the query in place. history.state is kept because
    // the router tracks its position in it.
    useEffect(() => {
        if (requested)
            globalThis.history.replaceState(
                globalThis.history.state,
                "",
                Path.Home
            );
    }, [requested]);

    return (
        <Dialog
            visible={open}
            title="Thank you for registering!"
            centerTitle
            testID="app-tour-modal"
            persistent
            onRequestClose={() => setOpen(false)}
            maxWidth={675}
            actionsAlign="center"
            actionsDivider={false}
            actions={
                <HgButton
                    testID="app-tour-done"
                    text="Got it!"
                    uppercase={false}
                    onPress={() => setOpen(false)}
                />
            }
        >
            <View className="-mx-6">
                <Image
                    source={RegistrationImage}
                    accessibilityLabel="Thank you registration splash"
                    resizeMode="cover"
                    style={{ width: "100%", height: mdAndUp ? 440 : 250 }}
                />
            </View>
            <AppText className="text-center mt-4">
                It may take up to four business days for all of your available
                health records to appear.
            </AppText>
        </Dialog>
    );
}
