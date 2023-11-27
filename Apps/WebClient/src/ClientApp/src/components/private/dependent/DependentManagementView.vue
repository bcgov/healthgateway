<script setup lang="ts">
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import AddDependentComponent from "@/components/private/dependent/AddDependentComponent.vue";
import DependentCardComponent from "@/components/private/dependent/DependentCardComponent.vue";
import BreadcrumbComponent from "@/components/common/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
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

const dependentStore = useDependentStore();
const userStore = useUserStore();

function refreshDependents(): void {
    dependentStore.retrieveDependents(userStore.hdid, true);
}

refreshDependents();
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="dependentStore.dependentsAreLoading" />
        <PageTitleComponent title="Dependents">
            <template #append>
                <AddDependentComponent @handle-submit="refreshDependents" />
            </template>
        </PageTitleComponent>
        <DependentCardComponent
            v-for="dependent in dependentStore.dependents"
            :key="dependent.ownerId"
            :dependent="dependent"
            class="my-3"
        />
    </div>
</template>
