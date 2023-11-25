import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { EntryType } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import { CreateDelegationRequest } from "@/models/sharing/createDelegationRequest";
import { Delegation } from "@/models/sharing/delegation";
import { IDelegationService, ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";
import DataSourceUtil from "@/utility/dataSourceUtil";

export enum DelegationWizardStep {
    contact = 0,
    dataSources = 1,
    expiryDate = 2,
    review = 3,
    sharingCode = 4,
}

export interface DelegationWizardState {
    mode?: "Create" | "Modify";
    step?: DelegationWizardStep;
    nickname?: string;
    email?: string;
    expiryDateLabel?: string;
    expiryDate?: Date;
    recordTypes?: EntryType[];
    sharingCode?: string;
}

type WizardConfiguration = {
    [key in DelegationWizardStep]: {
        next?: DelegationWizardStep;
        previous?: DelegationWizardStep;
    };
};

const wizardConfiguration: WizardConfiguration = {
    [DelegationWizardStep.contact]: {
        next: DelegationWizardStep.dataSources,
        previous: undefined,
    },
    [DelegationWizardStep.dataSources]: {
        next: DelegationWizardStep.expiryDate,
        previous: DelegationWizardStep.contact,
    },
    [DelegationWizardStep.expiryDate]: {
        next: DelegationWizardStep.review,
        previous: DelegationWizardStep.dataSources,
    },
    [DelegationWizardStep.review]: {
        next: DelegationWizardStep.sharingCode,
        previous: DelegationWizardStep.expiryDate,
    },
    [DelegationWizardStep.sharingCode]: {
        next: undefined,
        previous: DelegationWizardStep.review,
    },
};

export const useDelegationStore = defineStore("delegation", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    const delegateService = container.get<IDelegationService>(
        SERVICE_IDENTIFIER.DelegateService
    );

    const userStore = useUserStore();
    const errorStore = useErrorStore();

    const delegations = ref<Delegation[]>([]);
    const delegationWizardState = ref<DelegationWizardState>();
    const delegationsAreLoading = ref(false);
    const error = ref<ResultError>();
    const statusMessage = ref("");

    const wizardNextStep = computed(() => {
        const currentStep = delegationWizardState.value?.step;
        if (currentStep === undefined) {
            return DelegationWizardStep.contact;
        }
        return wizardConfiguration[currentStep].next;
    });

    const wizardPreviousStep = computed(() => {
        const currentStep = delegationWizardState.value?.step;
        if (currentStep === undefined) {
            return undefined;
        }
        return wizardConfiguration[currentStep].previous;
    });

    const isDelegationDialogVisible = computed(
        () => delegationWizardState.value != undefined
    );

    const expiryDateDisplay = computed(() => {
        if (delegationWizardState.value?.expiryDate === undefined)
            return "Never";

        return DateWrapper.fromIsoDate(
            delegationWizardState.value?.expiryDate.toISOString()
        ).format("dd MMM yyyy");
    });

    const expiryDateOnly = computed(() => {
        if (delegationWizardState.value?.expiryDate === undefined)
            return undefined;

        return DateWrapper.fromIsoDate(
            delegationWizardState.value?.expiryDate.toISOString()
        ).format("yyyy-MM-dd");
    });

    function setDelegationsError(incomingError: ResultError) {
        error.value = incomingError;
        statusMessage.value = incomingError.resultMessage;
        delegationsAreLoading.value = false;
    }

    function startNewDelegation(): void {
        delegationWizardState.value = {
            mode: "Create",
            step: wizardNextStep.value,
        };
    }

    function moveWizardToPreviousStep(): void {
        if (wizardPreviousStep.value === undefined) {
            return;
        }
        delegationWizardState.value = {
            ...delegationWizardState.value,
            step: wizardPreviousStep.value,
        };
    }

    function moveWiazrdToNextStep(): void {
        if (wizardNextStep.value === undefined) {
            return;
        }
        delegationWizardState.value = {
            ...delegationWizardState.value,
            step: wizardNextStep.value,
        };
    }

    function clearDelegationWizardState() {
        delegationWizardState.value = undefined;
    }

    function captureDelegationContact(email: string, nickname: string) {
        delegationWizardState.value = {
            ...delegationWizardState.value,
            email: email,
            nickname: nickname,
        };
        moveWiazrdToNextStep();
    }

    function captureDelegationRecordTypes(dataSources: EntryType[]) {
        delegationWizardState.value = {
            ...delegationWizardState.value,
            recordTypes: dataSources,
        };
        moveWiazrdToNextStep();
    }

    function captureDelegationExpiry(
        expiryDateLabel: string,
        expiryDate?: Date
    ) {
        delegationWizardState.value = {
            ...delegationWizardState.value,
            expiryDate: expiryDate,
            expiryDateLabel: expiryDateLabel,
        };
        moveWiazrdToNextStep();
    }

    function setWizardSharingCode(sharingCode: string) {
        delegationWizardState.value = {
            ...delegationWizardState.value,
            step: wizardNextStep.value,
            sharingCode: sharingCode,
        };
    }

    function submitDelegationDialog(): Promise<string | void> {
        if (delegationWizardState.value?.mode === "Create") {
            return createDelegation({
                nickname: delegationWizardState.value.nickname,
                email: delegationWizardState.value.email,
                expiryDate: expiryDateOnly.value,
                dataSources: delegationWizardState.value.recordTypes?.map(
                    (entryType) => DataSourceUtil.getDataSource(entryType)
                ),
            }).then((sharingCode) => {
                if (sharingCode != undefined) {
                    setWizardSharingCode(sharingCode!);
                }
            });
        }
        throw new Error("Invalid invitation dialog state");
    }

    function createDelegation(
        invitation: CreateDelegationRequest
    ): Promise<string | void> {
        logger.debug("Create invitation");
        delegationsAreLoading.value = true;
        return delegateService
            .createDelegation(userStore.hdid, invitation)
            .catch((error: ResultError) => {
                handleError(error, ErrorType.Create, "sharing-dialog");
            })
            .finally(() => (delegationsAreLoading.value = false));
    }

    function handleError(
        incomingError: ResultError,
        errorType: ErrorType,
        errorKey?: string
    ) {
        logger.error(`Error: ${JSON.stringify(incomingError)}`);
        setDelegationsError(incomingError);
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
        delegations,
        delegationsAreLoading,
        isDelegationDialogVisible,
        expiryDateDisplay,
        wizardPreviousStep,
        wizardNextStep,
        delegationWizardState,
        startNewDelegation,
        captureDelegationContact,
        captureDelegationRecordTypes,
        captureDelegationExpiry,
        submitDelegationDialog,
        moveWizardToPreviousStep,
        clearDelegationWizardState,
    };
});
