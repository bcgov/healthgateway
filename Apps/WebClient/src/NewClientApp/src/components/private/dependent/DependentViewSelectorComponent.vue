<script setup lang="ts">
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import AddDependentComponent from "@/components/private/dependent/AddDependentComponent.vue";
import { useDependentStore } from "@/stores/dependent";
import { useUserStore } from "@/stores/user";

const dependentStore = useDependentStore();
const userStore = useUserStore();

function retrieveDependents(hdid: string, bypassCache: boolean): Promise<void> {
    return dependentStore.retrieveDependents(userStore.hdid, bypassCache);
}
retrieveDependents(userStore.hdid, false);
</script>

<template>
    <PageTitleComponent title="Dependents">
        <template #append>
            <AddDependentComponent />
        </template>
    </PageTitleComponent>
</template>
