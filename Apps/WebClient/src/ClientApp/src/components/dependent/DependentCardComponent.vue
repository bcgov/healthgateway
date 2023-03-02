<script lang="ts">
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DependentDashboardTabComponent from "@/components/dependent/tabs/DependentDashboardTabComponent.vue";
import DeleteModalComponent from "@/components/modal/DeleteModalComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";
import User from "@/models/user";
import DependentUtil from "@/utility/dependentUtil";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        DeleteModalComponent,
        DependentDashboardTabComponent,
    },
};

@Component(options)
export default class DependentCardComponent extends Vue {
    @Prop({ required: true })
    private dependent!: Dependent;

    @Action("removeDependent", { namespace: "dependent" })
    private removeDependent!: (params: {
        hdid: string;
        dependent: Dependent;
    }) => Promise<void>;

    @Getter("webClient", { namespace: "config" })
    private config!: WebClientConfiguration;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Ref("removeConfirmationModal")
    private readonly removeConfirmationModal!: DeleteModalComponent;

    private get formattedName(): string {
        return DependentUtil.formatName(this.dependent.dependentInformation);
    }

    private get isExpired(): boolean {
        const birthDate = new DateWrapper(
            this.dependent.dependentInformation.dateOfBirth
        );
        const now = new DateWrapper();
        return now.diff(birthDate, "year").years > this.config.maxDependentAge;
    }

    private remove(): void {
        this.removeDependent({
            hdid: this.user.hdid,
            dependent: this.dependent,
        });
    }
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
                                    @click="removeConfirmationModal.showModal()"
                                >
                                    Delete
                                </b-dropdown-item>
                            </b-nav-item-dropdown>
                        </b-col>
                    </b-row>
                </template>
                <b-tab
                    no-body
                    active
                    title="Dashboard"
                    data-testid="dashboard-tab"
                >
                    <div v-if="isExpired" class="text-center">
                        <h5>Your access has expired</h5>
                        <p>
                            You no longer have access to this dependent as they
                            have turned {{ config.maxDependentAge }}
                        </p>
                        <hg-button variant="secondary" @click="remove">
                            Remove Dependent
                        </hg-button>
                    </div>
                    <DependentDashboardTabComponent
                        v-else
                        :dependent="dependent"
                    />
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
