<script setup lang="ts">
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import DelegationContactStepComponent from "@/components/private/sharing/DelegationContactStepComponent.vue";
import DelegationExpiryStepComponent from "@/components/private/sharing/DelegationExpiryStepComponent.vue";
import DelegationRecordTypesStepComponent from "@/components/private/sharing/DelegationRecordTypesStepComponent.vue";
import DelegationReviewStepComponent from "@/components/private/sharing/DelegationReviewStepComponent.vue";
import DelegationSharingCodeStepComponent from "@/components/private/sharing/DelegationSharingCodeStepComponent.vue";
import { DelegationWizardStep, useDelegationStore } from "@/stores/delegation";

const delegateStore = useDelegationStore();

const delegationContactStepComponent =
    ref<InstanceType<typeof DelegationContactStepComponent>>();
const delegationRecordTypesStepComponent =
    ref<InstanceType<typeof DelegationRecordTypesStepComponent>>();
const delegationExpiryStepComponent =
    ref<InstanceType<typeof DelegationExpiryStepComponent>>();
const delegationReviewStepComponent =
    ref<InstanceType<typeof DelegationReviewStepComponent>>();

const showDialog = computed(
    () => delegateStore.delegationWizardState !== undefined
);
const currentStep = computed(() => delegateStore.delegationWizardState?.step);
const isLastStep = computed(() => delegateStore.wizardNextStep === undefined);
const isFirstStep = computed(
    () => delegateStore.wizardPreviousStep === undefined
);
const currentTitle = computed(() => {
    switch (delegateStore.delegationWizardState?.step) {
        case DelegationWizardStep.contact:
            return "Invite Someone";
        case DelegationWizardStep.dataSources:
            return "Select Record Types";
        case DelegationWizardStep.expiryDate:
            return "Select Access Expiry";
        case DelegationWizardStep.review:
            return "Review Invitation";
        case DelegationWizardStep.sharingCode:
            return "Sharing Code";
        default:
            return "Invite Someone";
    }
});
const nextStepButtonTitle = computed(() => {
    switch (delegateStore.wizardNextStep) {
        case DelegationWizardStep.dataSources:
            return "Next: Select Records";
        case DelegationWizardStep.expiryDate:
            return "Next: Expiry Date";
        case DelegationWizardStep.review:
            return "Next: Review";
        case DelegationWizardStep.sharingCode:
            return "Next: Sharing Code";
        default:
            return "Done";
    }
});

function handleCancel(): void {
    delegateStore.clearDelegationWizardState();
}

function handleBack(): void {
    delegateStore.moveWizardToPreviousStep();
}

function handleSaveCurrentStep(): void {
    switch (currentStep.value) {
        case DelegationWizardStep.contact:
            delegationContactStepComponent.value?.saveStep();
            break;
        case DelegationWizardStep.dataSources:
            delegationRecordTypesStepComponent.value?.saveStep();
            break;
        case DelegationWizardStep.expiryDate:
            delegationExpiryStepComponent.value?.saveStep();
            break;
        case DelegationWizardStep.review:
            delegationReviewStepComponent.value?.saveStep();
            break;
        case DelegationWizardStep.sharingCode:
            delegateStore.clearDelegationWizardState();
            break;
        default:
            break;
    }
}
</script>

<template>
    <v-row justify="center">
        <v-dialog v-model="showDialog" max-width="800px">
            <v-card data-testid="delegation-wizard-dialog">
                <v-card-title class="bg-primary px-0">
                    <v-toolbar
                        :title="currentTitle"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            data-testid="delegation-dialog-close-button"
                            icon="close"
                            aria-label="Close"
                            @click="handleCancel"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text v-if="currentStep !== undefined">
                    <v-window v-model="currentStep">
                        <v-window-item
                            :value="DelegationWizardStep.contact"
                            class="px-1"
                        >
                            <DelegationContactStepComponent
                                ref="delegationContactStepComponent"
                            />
                        </v-window-item>
                        <v-window-item
                            :value="DelegationWizardStep.dataSources"
                            class="px-1"
                        >
                            <DelegationRecordTypesStepComponent
                                ref="delegationRecordTypesStepComponent"
                            />
                        </v-window-item>
                        <v-window-item
                            :value="DelegationWizardStep.expiryDate"
                            class="px-1"
                        >
                            <DelegationExpiryStepComponent
                                ref="delegationExpiryStepComponent"
                            />
                        </v-window-item>
                        <v-window-item
                            :value="DelegationWizardStep.review"
                            class="px-1"
                        >
                            <DelegationReviewStepComponent
                                ref="delegationReviewStepComponent"
                            />
                        </v-window-item>
                        <v-window-item
                            :value="DelegationWizardStep.sharingCode"
                            class="px-1"
                        >
                            <DelegationSharingCodeStepComponent />
                        </v-window-item>
                    </v-window>
                </v-card-text>
                <v-card-actions class="mt-4 border-t-sm pa-4">
                    <v-spacer />
                    <HgButtonComponent
                        v-if="isFirstStep"
                        variant="secondary"
                        text="Cancel"
                        data-testid="delegation-dialog-cancel-button"
                        size="large"
                        @click="handleCancel"
                    />
                    <HgButtonComponent
                        v-else-if="!isLastStep"
                        variant="secondary"
                        text="Back"
                        data-testid="delegation-dialog-back-button"
                        size="large"
                        class="px-4"
                        @click="handleBack"
                    />
                    <HgButtonComponent
                        variant="primary"
                        :text="nextStepButtonTitle"
                        size="large"
                        data-testid="delegation-dialog-confirm-button"
                        class="px-4"
                        @click="handleSaveCurrentStep"
                    />
                    <v-spacer v-if="isLastStep" />
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-row>
</template>
