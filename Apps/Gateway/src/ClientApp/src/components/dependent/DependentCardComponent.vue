<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEllipsisV } from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import DependentDashboardTabComponent from "@/components/dependent/tabs/DependentDashboardTabComponent.vue";
import DependentProfileTabComponent from "@/components/dependent/tabs/DependentProfileTabComponent.vue";
import DependentReportsTabComponent from "@/components/dependent/tabs/DependentReportsTabComponent.vue";
import DeleteModalComponent from "@/components/modal/DeleteModalComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import User from "@/models/user";
import DependentUtil from "@/utility/dependentUtil";

library.add(faEllipsisV);

interface Props {
    dependent: Dependent;
}
const props = defineProps<Props>();

const store = useStore();

const removeConfirmationModal =
    ref<InstanceType<typeof DeleteModalComponent>>();

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const user = computed<User>(() => store.getters["user/user"]);

const formattedName = computed(() => {
    return DependentUtil.formatName(props.dependent.dependentInformation);
});
const isExpired = computed(() => {
    const birthDate = new DateWrapper(
        props.dependent.dependentInformation.dateOfBirth
    );
    const now = new DateWrapper();
    return now.diff(birthDate, "year").years > config.value.maxDependentAge;
});

function removeDependent(hdid: string, dependent: Dependent): Promise<void> {
    return store.dispatch("dependent/removeDependent", { hdid, dependent });
}

function remove(): void {
    removeDependent(user.value.hdid, props.dependent);
}
</script>

<template>
    <div>
        <b-card no-body :data-testid="`dependent-card-${dependent.ownerId}`">
            <b-tabs card content-class="p-3">
                <template #tabs-start>
                    <b-row no-gutters class="w-100">
                        <b-col>
                            <div class="card-title" data-testid="dependentName">
                                {{ formattedName }}
                            </div>
                        </b-col>
                        <b-col cols="auto">
                            <b-nav-item-dropdown right :no-caret="true">
                                <template slot="button-content">
                                    <hg-icon
                                        icon="ellipsis-v"
                                        size="medium"
                                        data-testid="dependentMenuBtn"
                                        class="dependent-menu"
                                    />
                                </template>
                                <b-dropdown-item
                                    data-testid="deleteDependentMenuBtn"
                                    class="menuItem"
                                    @click="
                                        removeConfirmationModal?.showModal()
                                    "
                                >
                                    Delete
                                </b-dropdown-item>
                            </b-nav-item-dropdown>
                        </b-col>
                    </b-row>
                </template>
                <b-tab
                    :button-id="`dashboard-tab-button-${dependent.ownerId}`"
                    no-body
                    active
                    title="Dashboard"
                    data-testid="dashboard-tab"
                >
                    <div
                        v-if="isExpired"
                        class="text-center"
                        :data-testid="`dependent-is-expired-div-${dependent.ownerId}`"
                    >
                        <h5>Your access has expired</h5>
                        <p>
                            You no longer have access to this dependent as they
                            have turned {{ config.maxDependentAge }}
                        </p>
                        <hg-button
                            variant="secondary"
                            :data-testid="`remove-dependent-btn-${dependent.ownerId}`"
                            @click="remove"
                        >
                            Remove Dependent
                        </hg-button>
                    </div>
                    <DependentDashboardTabComponent
                        v-else
                        :dependent="dependent"
                    />
                </b-tab>
                <b-tab
                    :button-id="`report-tab-button-${dependent.ownerId}`"
                    no-body
                    :disabled="isExpired"
                    title="Export Records"
                    data-testid="report-tab"
                >
                    <DependentReportsTabComponent :dependent="dependent" />
                </b-tab>
                <b-tab
                    :button-id="`profile-tab-button-${dependent.ownerId}`"
                    no-body
                    :disabled="isExpired"
                    title="Profile"
                    data-testid="profile-tab"
                >
                    <DependentProfileTabComponent :dependent="dependent" />
                </b-tab>
            </b-tabs>
        </b-card>
        <DeleteModalComponent
            ref="removeConfirmationModal"
            title="Remove Dependent"
            confirm="Remove Dependent"
            message="Are you sure you want to remove this dependent?"
            @submit="remove"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.dependent-menu {
    color: $soft_text;
}

.card-title {
    padding-left: 14px;
    font-size: 1.2em;
}
</style>
