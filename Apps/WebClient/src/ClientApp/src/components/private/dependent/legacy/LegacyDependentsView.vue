<script setup lang="ts">
import { computed } from "vue";

import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import AddDependentComponent from "@/components/private/dependent/AddDependentComponent.vue";
import LegacyDependentCardComponent from "@/components/private/dependent/legacy/LegacyDependentCardComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { useConfigStore } from "@/stores/config";
import { useDependentStore } from "@/stores/dependent";
import { useUserStore } from "@/stores/user";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Dependents",
        to: "/dependents",
        active: true,
        dataTestId: "breadcrumb-dependents",
    },
];

const configStore = useConfigStore();
const dependentStore = useDependentStore();
const userStore = useUserStore();

const webClientConfig = computed(() => configStore.webConfig);
const dependents = computed(() => dependentStore.dependents);
const dependentsAreLoading = computed(
    () => dependentStore.dependentsAreLoading
);

function retrieveDependents(hdid: string, bypassCache: boolean): Promise<void> {
    return dependentStore.retrieveDependents(hdid, bypassCache);
}

function refreshDependents(): void {
    retrieveDependents(userStore.hdid, true);
}

retrieveDependents(userStore.hdid, false);
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <LoadingComponent :is-loading="dependentsAreLoading" />
    <PageTitleComponent title="Dependents">
        <template #append>
            <AddDependentComponent @handle-submit="refreshDependents" />
        </template>
    </PageTitleComponent>
    <h5 class="text-subtitle-1 font-weight-bold">
        You can add your dependents under the age of
        {{ webClientConfig.maxDependentAge }} to view their health records. Make
        sure you include all given names exactly as shown on their BC Services
        Card.
    </h5>
    <LegacyDependentCardComponent
        v-for="dependent in dependents"
        :key="dependent.ownerId"
        :dependent="dependent"
        class="mt-4"
        @needs-update="refreshDependents"
    />
</template>
