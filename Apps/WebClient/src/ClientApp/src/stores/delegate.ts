import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { DataSource } from "@/constants/dataSource";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { CreateDelegateInvitationRequest } from "@/models/sharing/createDelegateInvitationRequest";
import { DelegateInvitation } from "@/models/sharing/delegateInvitation";
import { IDelegateService, ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";

export enum DelegateInvitationWizardStep {
    contact = 0,
    dataSources = 1,
    expiryDate = 2,
    review = 3,
    sharingCode = 4,
}

export interface DelegateInvitationWizardState {
    mode?: "Create" | "Modify";
    step?: DelegateInvitationWizardStep;
    nickname?: string;
    email?: string;
    expiryDateRange?: string;
    expiryDate?: string;
    dataSources?: DataSource[];
    sharingCode?: string;
}

const wizardConfiguration = {
    [DelegateInvitationWizardStep.contact]: {
        next: DelegateInvitationWizardStep.dataSources,
        previous: undefined,
    },
    [DelegateInvitationWizardStep.dataSources]: {
        next: DelegateInvitationWizardStep.expiryDate,
        previous: DelegateInvitationWizardStep.contact,
    },
    [DelegateInvitationWizardStep.expiryDate]: {
        next: DelegateInvitationWizardStep.review,
        previous: DelegateInvitationWizardStep.dataSources,
    },
    [DelegateInvitationWizardStep.review]: {
        next: DelegateInvitationWizardStep.sharingCode,
        previous: DelegateInvitationWizardStep.expiryDate,
    },
    [DelegateInvitationWizardStep.sharingCode]: {
        next: undefined,
        previous: DelegateInvitationWizardStep.review,
    },
};

export const useDelegateStore = defineStore("delegate", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const delegateService = container.get<IDelegateService>(
        SERVICE_IDENTIFIER.DelegateService
    );

    const errorStore = useErrorStore();

    const invitations = ref<DelegateInvitation[]>([]);
    const invitationWizardState = ref<DelegateInvitationWizardState>();
    const invitationsAreLoading = ref(false);
    const error = ref<ResultError>();
    const statusMessage = ref("");

    const wizardNextStep = computed(() => {
        const currentStep = invitationWizardState.value?.step;
        if (currentStep === undefined) {
            return DelegateInvitationWizardStep.contact;
        }
        return wizardConfiguration[currentStep].next;
    });

    const wizardPreviousStep = computed(() => {
        const currentStep = invitationWizardState.value?.step;
        if (currentStep === undefined) {
            return undefined;
        }
        return wizardConfiguration[currentStep].previous;
    });

    const isInvitationDialogVisible = computed(
        () => invitationWizardState.value != undefined
    );

    function setInvitationsError(incomingError: ResultError) {
        error.value = incomingError;
        statusMessage.value = incomingError.resultMessage;
        invitationsAreLoading.value = false;
    }

    function startNewInvitation(): void {
        invitationWizardState.value = {
            mode: "Create",
            step: wizardNextStep.value,
        };
    }

    function wizardToPreviousStep(): void {
        if (wizardPreviousStep.value === undefined) {
            return;
        }
        invitationWizardState.value = {
            ...invitationWizardState.value,
            step: wizardPreviousStep.value,
        };
    }

    function wiazrdToNextStep(): void {
        if (wizardNextStep.value === undefined) {
            return;
        }
        invitationWizardState.value = {
            ...invitationWizardState.value,
            step: wizardNextStep.value,
        };
    }

    function clearInvitationDialogState() {
        invitationWizardState.value = undefined;
    }

    function captureInvitationContact(email: string, nickname: string) {
        invitationWizardState.value = {
            ...invitationWizardState.value,
            email: email,
            nickname: nickname,
        };
        wiazrdToNextStep();
    }

    function captureInvitationDataSources(dataSources: DataSource[]) {
        invitationWizardState.value = {
            ...invitationWizardState.value,
            dataSources: dataSources,
        };
        wiazrdToNextStep();
    }

    function captureExpiryDate(expiryDateRange: string, expiryDate?: string) {
        invitationWizardState.value = {
            ...invitationWizardState.value,
            expiryDate: expiryDate,
            expiryDateRange: expiryDateRange,
        };
        wiazrdToNextStep();
    }

    function setWizardSharingCode(sharingCode: string) {
        invitationWizardState.value = {
            ...invitationWizardState.value,
            step: wizardNextStep.value,
            sharingCode: sharingCode,
        };
    }

    function submitInvitationDialog(): Promise<string | void> {
        if (invitationWizardState.value?.mode === "Create") {
            return createInvitation({
                nickname: invitationWizardState.value.nickname,
                email: invitationWizardState.value.email,
                expiryDate: invitationWizardState.value.expiryDate,
                dataSources: invitationWizardState.value.dataSources,
            }).then((sharingCode) => {
                if (sharingCode != undefined) {
                    setWizardSharingCode(sharingCode!);
                }
            });
        }
        throw new Error("Invalid invitation dialog state");
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
            });
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
        invitations,
        invitationsAreLoading,
        isInvitationDialogVisible,
        wizardPreviousStep,
        wizardNextStep,
        invitationWizardState,
        startNewInvitation,
        captureInvitationContact,
        captureInvitationDataSources,
        captureExpiryDate,
        submitInvitationDialog,
        wizardToPreviousStep,
        clearInvitationDialogState,
    };
});
