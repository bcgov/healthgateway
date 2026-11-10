import { useState } from "react";
import { Platform, View } from "react-native";

import {
    AppText,
    Dialog,
    ExternalLink,
    HgButton,
    HgTextField,
} from "@/components/common";
import { describeError, logger } from "@/services/logger";
import {
    selectMedicationsAreLoading,
    selectMedicationsAreProtected,
    selectProtectiveWordAttempts,
    useMedicationStore,
} from "@/stores/medication.store";

const RULES_URL =
    "https://www2.gov.bc.ca/gov/content/health/health-drug-coverage/pharmacare-for-bc-residents/pharmanet/protective-word-for-a-pharmanet-record";

/**
 * Port of ProtectiveWordComponent.vue: when PharmaNet answers that the medications are protected, asks for the
 * protective word and retries with it. Closing it leaves the medications out of the timeline.
 */
export function ProtectiveWordDialog({
    hdid,
    onAbort,
}: {
    hdid: string;
    onAbort?: () => void;
}) {
    const isLoading = useMedicationStore(selectMedicationsAreLoading(hdid));
    const isProtected = useMedicationStore(selectMedicationsAreProtected(hdid));
    const attempts = useMedicationStore(selectProtectiveWordAttempts(hdid));
    const retrieveMedications = useMedicationStore(
        (s) => s.retrieveMedications
    );
    const [protectiveWord, setProtectiveWord] = useState("");
    const [isClosed, setIsClosed] = useState(false);

    const isVisible = !isClosed && isProtected && !isLoading;
    const errorMessages =
        attempts > 1 ? ["Invalid protective word. Try again."] : [];

    function handleOk() {
        retrieveMedications(hdid, protectiveWord).catch((error: unknown) =>
            logger.error(
                `Error retrieving medications: ${describeError(error)}`
            )
        );
    }

    function handleClose() {
        setIsClosed(true);
        onAbort?.();
    }

    return (
        <Dialog
            visible={isVisible}
            title="Restricted PharmaNet Records"
            tone="primary"
            persistent
            maxWidth={700}
            testID="protectiveWordModal"
            closeTestID="protectiveWordCloseButton"
            onRequestClose={handleClose}
            actionsDivider={false}
            actions={
                <HgButton
                    testID="protectiveWordContinueBtn"
                    text="Continue"
                    disabled={!protectiveWord}
                    onPress={handleOk}
                />
            }
        >
            <View nativeID="protective-word-modal">
                <AppText
                    variant="body-medium"
                    testID="protectiveWordModalText"
                    className="mb-4"
                >
                    Please enter the protective word required to access these
                    restricted PharmaNet records. For more information visit{" "}
                    <ExternalLink
                        testID="protectiveWordModalRulesHREF"
                        href={RULES_URL}
                    >
                        protective-word-for-a-pharmanet-record
                    </ExternalLink>
                </AppText>
                <AppText variant="body-medium" className="mb-1">
                    <AppText className="text-error" aria-hidden>
                        *
                    </AppText>{" "}
                    Protective word
                </AppText>
                <HgTextField
                    testID="protectiveWordInput"
                    nativeID="protectiveWord-input"
                    accessibilityLabel="Protective word"
                    placeholder="Protective Word"
                    value={protectiveWord}
                    onChangeText={setProtectiveWord}
                    secureTextEntry
                    autoCapitalize="none"
                    autoCorrect={false}
                    errorMessages={errorMessages}
                    // native: the field takes the cursor when the dialog opens, and the keyboard's return key submits,
                    // like Continue (the web client's field has neither)
                    autoFocus={Platform.OS !== "web"}
                    returnKeyType={Platform.OS === "web" ? undefined : "go"}
                    onSubmitEditing={
                        Platform.OS === "web"
                            ? undefined
                            : () => {
                                  if (protectiveWord) handleOk();
                              }
                    }
                />
            </View>
        </Dialog>
    );
}
