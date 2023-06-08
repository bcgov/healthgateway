<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faUserPlus } from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import DependentCardComponent from "@/components/DependentCardComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import NewDependentComponent from "@/components/modal/NewDependentComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import type { Dependent } from "@/models/dependent";
import User from "@/models/user";

library.add(faUserPlus);

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Dependents",
        to: "/dependents",
        active: true,
        dataTestId: "breadcrumb-dependents",
    },
];

const store = useStore();

const newDependentModal = ref<InstanceType<typeof NewDependentComponent>>();

const webClientConfig = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const dependents = computed<Dependent[]>(
    () => store.getters["dependent/dependents"]
);
const dependentsAreLoading = computed<boolean>(
    () => store.getters["dependent/dependentsAreLoading"]
);
const user = computed<User>(() => store.getters["user/user"]);

function retrieveDependents(hdid: string, bypassCache: boolean): Promise<void> {
    return store.dispatch("dependent/retrieveDependents", {
        hdid,
        bypassCache,
    });
}

function refreshDependents(): void {
    retrieveDependents(user.value.hdid, true);
}

retrieveDependents(user.value.hdid, false);
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="dependentsAreLoading" />
        <page-title title="Dependents">
            <hg-button
                id="add-dependent-button"
                data-testid="addNewDependentBtn"
                class="float-right"
                variant="secondary"
                @click="newDependentModal?.showModal()"
            >
                <hg-icon icon="user-plus" size="medium" class="mr-2" />
                <span>Add</span>
            </hg-button>
        </page-title>
        <h5 class="my-3">
            You can add your dependents under the age of
            {{ webClientConfig.maxDependentAge }} to view their health records.
            Make sure you include all given names exactly as shown on their BC
            Services Card.
        </h5>
        <DependentCardComponent
            v-for="dependent in dependents"
            :key="dependent.ownerId"
            :dependent="dependent"
            class="mt-2"
            @needs-update="refreshDependents"
        />
        <NewDependentComponent
            ref="newDependentModal"
            @handle-submit="refreshDependents"
        />
    </div>
</template>
