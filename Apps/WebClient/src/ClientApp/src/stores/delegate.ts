import { defineStore } from "pinia";
import { ref } from "vue";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DelegateInvitation } from "@/models/delegateInvitation";
import { ResultError } from "@/models/errors";
import { IDelegateService, ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";

const cleanInvitation: DelegateInvitation = {
    id: "",
    nickname: "",
    status: "",
    email: "",
    expiryDate: "",
    dataSources: [],
};

export const useDelegateStore = defineStore("delegate", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const delegateService = container.get<IDelegateService>(
        SERVICE_IDENTIFIER.DelegateService
    );

    const errorStore = useErrorStore();

    const invitations = ref<DelegateInvitation[]>([]);
    const newInvitation = ref<DelegateInvitation>({ ...cleanInvitation });
    const invitationsAreLoading = ref(false);
    const error = ref<ResultError>();
    const statusMessage = ref("");

    function setInvitationsError(incomingError: ResultError) {
        error.value = incomingError;
        statusMessage.value = incomingError.resultMessage;
        invitationsAreLoading.value = false;
    }

    function createInvitation(
        invitation: DelegateInvitation
    ): Promise<DelegateInvitation | void> {
        logger.debug("Create invitation");
        invitationsAreLoading.value = true;
        return delegateService
            .createInvitation(invitation)
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Create);
            })
            .then((result) => {
                if (result) {
                    // TODO retrieve all invitations, once endpoint is created
                    invitations.value.push(result);
                }
                invitationsAreLoading.value = false;
                resetNewInvitation();
                return result;
            });
    }

    function resetNewInvitation() {
        newInvitation.value = { ...cleanInvitation };
    }

    function handleError(incomingError: ResultError, errorType: ErrorType) {
        logger.error(`Error: ${JSON.stringify(incomingError)}`);
        setInvitationsError(incomingError);
        if (incomingError.statusCode === 429) {
            const errorKey = "page";
            if (errorType === ErrorType.Retrieve) {
                errorStore.setTooManyRequestsWarning(errorKey);
            } else {
                errorStore.setTooManyRequestsError(errorKey);
            }
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.Dependent,
                incomingError.traceId
            );
        }
    }

    return {
        createInvitation,
        resetInvitation: resetNewInvitation,
        invitations,
        invitationsAreLoading,
    };
});
