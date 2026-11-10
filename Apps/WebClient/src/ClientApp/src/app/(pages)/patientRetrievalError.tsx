import { useRouter } from "expo-router";
import {} from "react-native";

import { AppText, HgButton } from "@/components/common";
import { AppErrorType } from "@/constants/errorType";
import { Path } from "@/constants/path";
import { useErrorStore } from "@/stores/error.store";
import { openExternal } from "@/utils/openExternal";
import { PageError } from "@/views/errors/PageError";

/** Port of PatientRetrievalErrorView.vue */
export default function PatientRetrievalError() {
    const router = useRouter();
    const tooBusy = useErrorStore(
        (s) => s.appError === AppErrorType.TooManyRequests
    );
    return (
        <PageError
            title="Error retrieving user information"
            testID="patient-retrieval-error"
        >
            {tooBusy ? (
                <AppText className="text-center" testID="too-busy">
                    We are unable to retrieve the patient details from our
                    Client Registry as the site is too busy. Please try again
                    later.
                </AppText>
            ) : (
                <AppText className="text-center">
                    There may be an issue in our Client Registry. Please contact{" "}
                    <AppText
                        className="text-link font-bold"
                        onPress={() =>
                            void openExternal("mailto:HealthGateway@gov.bc.ca")
                        }
                    >
                        HealthGateway@gov.bc.ca
                    </AppText>
                </AppText>
            )}
            <HgButton
                testID="patient-retrieval-error-logout-button"
                variant="secondary"
                icon="right-from-bracket"
                text="Log Out"
                className="mt-4"
                onPress={() => router.push(Path.Logout)}
            />
        </PageError>
    );
}
