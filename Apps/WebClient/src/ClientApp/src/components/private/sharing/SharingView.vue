<script setup lang="ts">
import { computed } from "vue";

import BreadcrumbComponent from "@/components/common/BreadcrumbComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import DelegationWizardDialog from "@/components/private/sharing/DelegationWizardDialog.vue";
import EmptySharingPageComponent from "@/components/private/sharing/EmptySharingPageComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { useDelegationStore } from "@/stores/delegation";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Sharing",
        to: "/sharing",
        active: true,
        dataTestId: "breadcrumb-sharing",
    },
];

const delegationStore = useDelegationStore();

const delegationsAreLoading = computed(
    () => delegationStore.delegationsAreLoading
);
const hasDelegations = computed(() => delegationStore.delegations.length > 0);

// TODO: call for invitations
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <LoadingComponent
        :is-loading="delegationsAreLoading"
        text="Loading invitations"
    />
    <PageTitleComponent title="Sharing" />
    <EmptySharingPageComponent v-if="!hasDelegations" class="mb-4" />
    <DelegationWizardDialog />
</template>
