<script setup lang="ts">
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import DependentDashboardTabComponent from "@/components/private/dependent/tabs/DependentDashboardTabComponent.vue";
import DependentProfileTabComponent from "@/components/private/dependent/tabs/DependentProfileTabComponent.vue";
import ReportsComponent from "@/components/private/reports/ReportsComponent.vue";
import { DateWrapper } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import { useConfigStore } from "@/stores/config";
import { useDependentStore } from "@/stores/dependent";
import { useUserStore } from "@/stores/user";
import DependentUtil from "@/utility/dependentUtil";

interface Props {
    dependent: Dependent;
}
const props = defineProps<Props>();

const configStore = useConfigStore();
const dependentStore = useDependentStore();
const userStore = useUserStore();

const deleteConfirmationModal =
    ref<InstanceType<typeof MessageModalComponent>>();
const selectedTabIndex = ref(0);

const formattedName = computed(() => {
    return DependentUtil.formatName(props.dependent.dependentInformation);
});
const isExpired = computed(
    () =>
        DateWrapper.today().diff(
            DateWrapper.fromIsoDate(
                props.dependent.dependentInformation.dateOfBirth
            ),
            "year"
        ).years > configStore.webConfig.maxDependentAge
);

function removeDependent(): void {
    dependentStore.removeDependent(userStore.hdid, props.dependent);
}
</script>

<template>
    <div>
        <v-card :data-testid="`dependent-card-${dependent.ownerId}`">
            <v-card-title class="bg-grey-lighten-3 pa-4 pb-0">
                <v-row>
                    <v-col>
                        <h5 class="text-h6 mb-4" data-testid="dependentName">
                            {{ formattedName }}
                        </h5>
                    </v-col>
                    <v-col cols="auto">
                        <v-menu>
                            <template
                                #activator="{ props: dependentMenuProps }"
                            >
                                <HgIconButtonComponent
                                    icon="ellipsis-v"
                                    size="small"
                                    class="text-medium-emphasis"
                                    data-testid="dependentMenuBtn"
                                    v-bind="dependentMenuProps"
                                />
                            </template>
                            <v-list>
                                <v-list-item
                                    data-testid="deleteDependentMenuBtn"
                                    class="menuItem"
                                    title="Delete"
                                    @click="
                                        deleteConfirmationModal?.showModal()
                                    "
                                />
                            </v-list>
                        </v-menu>
                    </v-col>
                </v-row>
                <v-tabs
                    v-model="selectedTabIndex"
                    color="primary"
                    selected-class="bg-white"
                >
                    <v-tab
                        :id="`dashboard-tab-button-${dependent.ownerId}`"
                        text="Dashboard"
                    />
                    <v-tab
                        :id="`report-tab-button-${dependent.ownerId}`"
                        :disabled="isExpired"
                        text="Export"
                    />
                    <v-tab
                        :id="`profile-tab-button-${dependent.ownerId}`"
                        :disabled="isExpired"
                        text="Profile"
                    />
                </v-tabs>
            </v-card-title>
            <v-card-text class="pa-4">
                <v-window v-model="selectedTabIndex">
                    <v-window-item data-testid="dashboard-tab" class="pa-1">
                        <div
                            v-if="isExpired"
                            class="text-center"
                            :data-testid="`dependent-is-expired-div-${dependent.ownerId}`"
                        >
                            <v-row>
                                <v-col class="d-flex justify-content-center">
                                    <h5 class="text-h6">
                                        Your access has expired
                                    </h5>
                                </v-col>
                            </v-row>
                            <v-row>
                                <v-col class="d-flex justify-content-center">
                                    <p>
                                        You no longer have access to this
                                        dependent as they have turned
                                        {{
                                            configStore.webConfig
                                                .maxDependentAge
                                        }}
                                    </p>
                                </v-col>
                            </v-row>
                            <v-row>
                                <v-col class="d-flex justify-content-center">
                                    <HgButtonComponent
                                        variant="secondary"
                                        text="Remove Dependent"
                                        :data-testid="`remove-dependent-btn-${dependent.ownerId}`"
                                        @click="removeDependent"
                                    />
                                </v-col>
                            </v-row>
                        </div>
                        <DependentDashboardTabComponent
                            v-else
                            :dependent="dependent"
                        />
                    </v-window-item>
                    <v-window-item data-testid="report-tab" class="pa-1">
                        <ReportsComponent
                            :hdid="dependent.dependentInformation.hdid"
                            :is-dependent="true"
                        />
                    </v-window-item>
                    <v-window-item data-testid="profile-tab" class="pa-1">
                        <DependentProfileTabComponent :dependent="dependent" />
                    </v-window-item>
                </v-window>
            </v-card-text>
        </v-card>
        <MessageModalComponent
            ref="deleteConfirmationModal"
            title="Remove Dependent"
            message="Are you sure you want to remove this dependent?"
            confirm="Remove Dependent"
            @submit="removeDependent"
        />
    </div>
</template>
