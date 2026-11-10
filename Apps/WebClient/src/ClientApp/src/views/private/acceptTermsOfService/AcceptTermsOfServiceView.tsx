import { useRouter } from "expo-router";
import { useEffect, useState } from "react";
import { View } from "react-native";

import { Loading } from "@/components/site/Loading";
import { HgButton, HgCheckbox, PageTitle } from "@/components/ui";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Path } from "@/constants/path";
import { isSessionEnded, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { TermsOfServiceBox } from "@/views/registration/TermsOfServiceBox";

/** Port of AcceptTermsOfServiceView.vue */
export function AcceptTermsOfServiceView() {
    const router = useRouter();
    const updateAcceptedTerms = useUserStore((s) => s.updateAcceptedTerms);
    const errorStore = useErrorStore();
    const [isLoading, setIsLoading] = useState(true);
    const [termsOfServiceId, setTermsOfServiceId] = useState("");
    const [termsOfService, setTermsOfService] = useState("");
    const [accepted, setAccepted] = useState(false);

    useEffect(() => {
        services.userProfile
            .getTermsOfService()
            .then((result) => {
                setTermsOfServiceId(result.id);
                setTermsOfService(result.content);
            })
            .catch((error) => {
                if (isSessionEnded(error)) return;
                logger.error(describeError(error));
                if (error instanceof ResultError && error.statusCode === 429)
                    errorStore.setTooManyRequestsWarning();
                else
                    errorStore.addError(
                        ErrorType.Retrieve,
                        ErrorSourceType.TermsOfService
                    );
            })
            .finally(() => setIsLoading(false));
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    async function onSubmit() {
        if (!accepted || isLoading) return;
        setIsLoading(true);
        try {
            await updateAcceptedTerms(termsOfServiceId);
            logger.debug("Redirecting to /home");
            router.replace(Path.Home);
        } catch (error) {
            if (isSessionEnded(error)) return;
            logger.error(
                `Error updating accepted terms of service: ${describeError(error)}`
            );
            if (error instanceof ResultError && error.statusCode === 429)
                errorStore.setTooManyRequestsError();
            else
                errorStore.addError(
                    ErrorType.Update,
                    ErrorSourceType.Profile,
                    error instanceof ResultError ? error.traceId : undefined
                );
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <View className="min-h-[320px]">
            <Loading isLoading={isLoading} />
            <PageTitle
                testID="tos-page-title"
                title="Update to our Terms of Service"
            />
            {!isLoading && termsOfService !== "" ? (
                <>
                    <TermsOfServiceBox
                        testID="tos-text-area-component"
                        html={termsOfService}
                        height={460}
                    />
                    <HgCheckbox
                        testID="accept-tos-checkbox"
                        label="I agree to the terms of service above"
                        checked={accepted}
                        onChange={setAccepted}
                        errorMessages={
                            accepted
                                ? []
                                : ["You must accept the terms of service."]
                        }
                        className="mb-4"
                    />
                    <View className="items-end">
                        <HgButton
                            testID="continue-btn"
                            text="Continue"
                            className="self-end"
                            disabled={!accepted || isLoading}
                            onPress={() => void onSubmit()}
                        />
                    </View>
                </>
            ) : null}
        </View>
    );
}
