<script setup lang="ts">
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import InvitationContactStepComponent from "@/components/private/sharing/InvitationContactStepComponent.vue";
import InvitationExpiryStepComponent from "@/components/private/sharing/InvitationExpiryStepComponent.vue";
import InvitationRecordTypesStepComponent from "@/components/private/sharing/InvitationRecordTypesStepComponent.vue";
import InvitationReviewStepComponent from "@/components/private/sharing/InvitationReviewStepComponent.vue";
import InvitationSharingCodeStepComponent from "@/components/private/sharing/InvitationSharingCodeStepComponent.vue";
import {
    DelegateInvitationWizardStep,
    useDelegateStore,
} from "@/stores/delegate";

const delegateStore = useDelegateStore();

const invitationContactStepComponent =
    ref<InstanceType<typeof InvitationContactStepComponent>>();
const invitationRecordTypesStepComponent =
    ref<InstanceType<typeof InvitationRecordTypesStepComponent>>();
const invitationExpiryStepComponent =
    ref<InstanceType<typeof InvitationExpiryStepComponent>>();
const invitationReviewStepComponent =
    ref<InstanceType<typeof InvitationReviewStepComponent>>();

const showDialog = computed(
    () => delegateStore.invitationWizardState !== undefined
);
const currentStep = computed(() => delegateStore.invitationWizardState?.step);
const isLastStep = computed(() => delegateStore.wizardNextStep === undefined);
const currentTitle = computed(() => {
    switch (delegateStore.invitationWizardState?.step) {
        case DelegateInvitationWizardStep.contact:
            return "Invite Someone";
        case DelegateInvitationWizardStep.dataSources:
            return "Select Record Types";
        case DelegateInvitationWizardStep.expiryDate:
            return "Select Access Expiry";
        case DelegateInvitationWizardStep.review:
            return "Review Invitation";
        case DelegateInvitationWizardStep.sharingCode:
            return "Sharing Code";
        default:
            return "Invite Someone";
    }
});
const nextStepButtonTitle = computed(() => {
    switch (delegateStore.wizardNextStep) {
        case DelegateInvitationWizardStep.dataSources:
            return "Next: Select Records";
        case DelegateInvitationWizardStep.expiryDate:
            return "Next: Expiry Date";
        case DelegateInvitationWizardStep.review:
            return "Next: Review";
        case DelegateInvitationWizardStep.sharingCode:
            return "Next: Sharing Code";
        default:
            return "Done";
    }
});

function handleCancel(): void {
    delegateStore.clearInvitationDialogState();
}

function handleBack(): void {
    delegateStore.wizardToPreviousStep();
}

function handleSaveCurrentStep(): void {
    switch (currentStep.value) {
        case DelegateInvitationWizardStep.contact:
            invitationContactStepComponent.value?.saveStep();
            break;
        case DelegateInvitationWizardStep.dataSources:
            invitationRecordTypesStepComponent.value?.saveStep();
            break;
        case DelegateInvitationWizardStep.expiryDate:
            invitationExpiryStepComponent.value?.saveStep();
            break;
        case DelegateInvitationWizardStep.review:
            invitationReviewStepComponent.value?.saveStep();
            break;
        case DelegateInvitationWizardStep.sharingCode:
            delegateStore.clearInvitationDialogState();
            break;
        default:
            break;
    }
}
</script>

<template>
    <v-row justify="center">
        <v-dialog v-model="showDialog" max-width="800px">
            <v-card>
                <v-card-title class="bg-primary px-0">
                    <v-toolbar
                        :title="currentTitle"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            data-testid="invitation-dialog-close-button"
                            icon="close"
                            aria-label="Close"
                            @click="handleCancel"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text v-if="currentStep !== undefined" class="pb-4">
                    <v-window v-model="currentStep">
                        <v-window-item
                            :value="DelegateInvitationWizardStep.contact"
                            class="px-1"
                        >
                            <InvitationContactStepComponent
                                ref="invitationContactStepComponent"
                            />
                        </v-window-item>
                        <v-window-item
                            :value="DelegateInvitationWizardStep.dataSources"
                            class="px-1"
                        >
                            <InvitationRecordTypesStepComponent
                                ref="invitationRecordTypesStepComponent"
                            />
                        </v-window-item>
                        <v-window-item
                            :value="DelegateInvitationWizardStep.expiryDate"
                            class="px-1"
                        >
                            <InvitationExpiryStepComponent
                                ref="invitationExpiryStepComponent"
                            />
                        </v-window-item>
                        <v-window-item
                            :value="DelegateInvitationWizardStep.review"
                            class="px-1"
                        >
                            <InvitationReviewStepComponent
                                ref="invitationReviewStepComponent"
                            />
                        </v-window-item>
                        <v-window-item
                            :value="DelegateInvitationWizardStep.sharingCode"
                            class="px-1"
                        >
                            <InvitationSharingCodeStepComponent />
                        </v-window-item>
                    </v-window>
                </v-card-text>
                <v-card-actions class="mt-4">
                    <v-spacer />
                    <HgButtonComponent
                        v-if="
                            currentStep === DelegateInvitationWizardStep.contact
                        "
                        variant="secondary"
                        text="Cancel"
                        data-testid="invitation-dialog-cancel-button"
                        size="large"
                        @click="handleCancel"
                    />
                    <HgButtonComponent
                        v-else-if="!isLastStep"
                        variant="secondary"
                        text="Back"
                        data-testid="invitation-dialog-cancel-button"
                        size="large"
                        @click="handleBack"
                    />
                    <HgButtonComponent
                        variant="primary"
                        :text="nextStepButtonTitle"
                        size="large"
                        data-testid="invitation-dialog-next-button"
                        @click="handleSaveCurrentStep"
                    />
                    <v-spacer v-if="isLastStep" />
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-row>
</template>
