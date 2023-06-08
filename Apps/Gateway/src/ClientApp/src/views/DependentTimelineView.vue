<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faArrowLeft,
    faCheckCircle,
    faEdit,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
    faMicroscope,
    faPills,
    faQuestion,
    faSearch,
    faStethoscope,
    faSyringe,
    faVial,
    faXRay,
} from "@fortawesome/free-solid-svg-icons";
import { computed } from "vue";
import { useRouter, useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import TimelineComponent from "@/components/timeline/TimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { Dependent } from "@/models/dependent";
import User from "@/models/user";
import ConfigUtil from "@/utility/configUtil";
import DependentUtil from "@/utility/dependentUtil";

library.add(
    faArrowLeft,
    faCheckCircle,
    faEdit,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
    faMicroscope,
    faPills,
    faQuestion,
    faSearch,
    faStethoscope,
    faSyringe,
    faVial,
    faXRay
);

interface Props {
    id: string;
}
const props = defineProps<Props>();

const store = useStore();
const router = useRouter();

const dependents = computed<Dependent[]>(
    () => store.getters["dependent/dependents"]
);

const dependentsAreLoading = computed<boolean>(
    () => store.getters["dependent/dependentsAreLoading"]
);

const user = computed<User>(() => store.getters["user/user"]);

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

const title = computed<string>(() => `Timeline for ${formattedName.value}`);

function retrieveDependents(hdid: string, bypassCache: boolean): Promise<void> {
    return store.dispatch("dependent/retrieveDependents", {
        hdid,
        bypassCache,
    });
}

retrieveDependents(user.value.hdid, false).then(() => {
    if (dependent.value === undefined) {
        router.push({ path: "/unauthorized" });
    }
});
</script>

<template>
    <div>
        <LoadingComponent :is-loading="dependentsAreLoading" />
        <div v-if="!dependentsAreLoading && dependent !== undefined">
            <BreadcrumbComponent :items="breadcrumbItems" />
            <page-title :title="title" data-testid="page-title">
                <template #prepend>
                    <b-button
                        to="/dependents"
                        data-testid="backBtn"
                        variant="link"
                        size="sm"
                        class="back-button-icon align-baseline p-0 mr-2"
                    >
                        <hg-icon icon="arrow-left" size="large" square />
                    </b-button>
                </template>
            </page-title>
            <TimelineComponent :hdid="id" :entry-types="entryTypes" />
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.back-button-icon {
    color: grey;
}
</style>
