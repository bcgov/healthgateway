import { defineStore } from "pinia";
import { ref } from "vue";

import { DataSource } from "@/constants/dataSource";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { CreateDelegateInvitationRequest } from "@/models/sharing/createDelegateInvitationRequest";
import { DelegateInvitation } from "@/models/sharing/delegateInvitation";
import { IDelegateService, ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";

export enum DelegateInvitationDialogStep {
    contact = 0,
    nickname = 1,
    dataSources = 2,
    expiryDate = 3,
    review = 4,
    sharingCode = 5,
}

export interface DelegateInvitationDialogState {
    mode: "Create" | "Modify";
    step: DelegateInvitationDialogStep;
    nickname?: string;
    email?: string;
    expiryDate?: string;
    dataSources?: DataSource[];
}

export const useDelegateStore = defineStore("delegate", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const delegateService = container.get<IDelegateService>(
        SERVICE_IDENTIFIER.DelegateService
    );

    const errorStore = useErrorStore();

    const invitations = ref<DelegateInvitation[]>([]);
    const invitationDialogState = ref<DelegateInvitationDialogState>();
    const invitationsAreLoading = ref(false);
    const error = ref<ResultError>();
    const statusMessage = ref("");

    function setInvitationsError(incomingError: ResultError) {
        error.value = incomingError;
        statusMessage.value = incomingError.resultMessage;
        invitationsAreLoading.value = false;
    }

    function submitInvitationDialog(): Promise<string | void> {
        if (
            invitationDialogState.value != null &&
            invitationDialogState.value.mode === "Create"
        ) {
            return createInvitation({
                nickname: invitationDialogState.value.nickname,
                email: invitationDialogState.value.email,
                expiryDate: invitationDialogState.value.expiryDate,
                dataSources: invitationDialogState.value.dataSources,
            });
        }
        // TODO: Remove this once all paths are covered
        return Promise.resolve();
    }

    function createInvitation(
        invitation: CreateDelegateInvitationRequest
    ): Promise<string | void> {
        logger.debug("Create invitation");
        invitationsAreLoading.value = true;
        return delegateService
            .createInvitation(invitation)
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Create, "sharing-dialog");
            })
            .then((result) => {
                // TODO: retrieve all invitations, when invitation is successfully created
                invitationsAreLoading.value = false;
                resetInvitationDialogState();
                return result;
            });
    }

    function resetInvitationDialogState() {
        invitationDialogState.value = undefined;
    }

    function handleError(
        incomingError: ResultError,
        errorType: ErrorType,
        errorKey?: string
    ) {
        logger.error(`Error: ${JSON.stringify(incomingError)}`);
        setInvitationsError(incomingError);
        if (incomingError.statusCode === 429) {
            errorKey = errorKey ?? "page";
            if (errorType === ErrorType.Retrieve) {
                errorStore.setTooManyRequestsWarning(errorKey);
            } else {
                errorStore.setTooManyRequestsError(errorKey);
            }
        } else {
            errorStore.addError(
                errorType,
                ErrorSourceType.Delegate,
                incomingError.traceId
            );
        }
    }

    return {
        submitInvitationDialog,
        resetInvitationDialogState,
        invitations,
        invitationsAreLoading,
    };
});
