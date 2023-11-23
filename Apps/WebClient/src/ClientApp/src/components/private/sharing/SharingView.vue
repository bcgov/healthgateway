<script setup lang="ts">
import { computed } from "vue";

import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import DelegateInvitationDialog from "@/components/private/sharing/DelegateInvitationDialog.vue";
import EmptySharingPageComponent from "@/components/private/sharing/EmptySharingPageComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { useDelegateStore } from "@/stores/delegate";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Sharing",
        to: "/sharing",
        active: true,
        dataTestId: "breadcrumb-sharing",
    },
];

const delegateStore = useDelegateStore();

const delegationsAreLoading = computed(
    () => delegateStore.delegationsAreLoading
);
const hasDelegations = computed(() => delegateStore.delegations.length > 0);

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
    <DelegateInvitationDialog />
</template>
