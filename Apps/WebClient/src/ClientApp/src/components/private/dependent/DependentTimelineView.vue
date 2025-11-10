<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import TimelineComponent from "@/components/private/timeline/TimelineComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { Dependent } from "@/models/dependent";
import { useDependentStore } from "@/stores/dependent";
import { useUserStore } from "@/stores/user";
import ConfigUtil from "@/utility/configUtil";
import DependentUtil from "@/utility/dependentUtil";

interface Props {
    id: string;
}
const props = defineProps<Props>();

const dependentStore = useDependentStore();
const userStore = useUserStore();
const router = useRouter();

const dependents = computed<Dependent[]>(() => dependentStore.dependents);
const dependent = computed<Dependent | undefined>(() => {
    return dependents.value.find((d: Dependent) => d.ownerId === props.id);
});
const formattedName = computed<string>(() =>
    DependentUtil.formatName(dependent.value?.dependentInformation)
);
const breadcrumbItems = computed<BreadcrumbItem[]>(() => {
    return [
        {
            text: "Dependents",
            to: "/dependents",
            active: false,
            dataTestId: "breadcrumb-dependents",
        },
        {
            text: formattedName.value,
            active: true,
            dataTestId: "breadcrumb-dependent-name",
        },
    ];
});
const entryTypes = computed<EntryType[]>(() => {
    return [...entryTypeMap.values()]
        .filter((d) => ConfigUtil.isDependentDatasetEnabled(d.type))
        .map((d) => d.type);
});
const title = computed<string>(
    () => `Health Records for ${formattedName.value}`
);

function retrieveDependents(hdid: string, bypassCache: boolean): Promise<void> {
    return dependentStore.retrieveDependents(hdid, bypassCache);
}

retrieveDependents(userStore.hdid, false).then(() => {
    if (dependent.value === undefined) {
        router.push({ path: "/unauthorized" });
    }
});
</script>

<template>
    <div>
        <LoadingComponent :is-loading="dependentStore.dependentsAreLoading" />
        <div
            v-if="
                !dependentStore.dependentsAreLoading && dependent !== undefined
            "
        >
            <BreadcrumbComponent :items="breadcrumbItems" />
            <PageTitleComponent :title="title" data-testid="page-title">
                <template #prepend>
                    <HgIconButtonComponent
                        to="/dependents"
                        icon="arrow-left"
                        size="small"
                        class="text-medium-emphasis"
                        data-testid="backBtn"
                    />
                </template>
            </PageTitleComponent>
            <TimelineComponent :hdid="id" :entry-types="entryTypes" />
        </div>
    </div>
</template>
