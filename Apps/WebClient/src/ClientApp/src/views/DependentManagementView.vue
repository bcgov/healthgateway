<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DependentCardComponent from "@/components/dependent/DependentCardComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import NewDependentComponent from "@/components/modal/NewDependentComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import TutorialComponent from "@/components/shared/TutorialComponent.vue";
import UserPreferenceType from "@/constants/userPreferenceType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { Dependent } from "@/models/dependent";
import User from "@/models/user";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BreadcrumbComponent,
        DependentCardComponent,
        LoadingComponent,
        NewDependentComponent,
        TutorialComponent,
    },
};

@Component(options)
export default class DependentManagementView extends Vue {
    @Action("retrieveDependents", { namespace: "dependent" })
    private retrieveDependents!: (params: {
        hdid: string;
        bypassCache: boolean;
    }) => Promise<void>;

    @Getter("dependents", { namespace: "dependent" })
    private dependents!: Dependent[];

    @Getter("dependentsAreLoading", { namespace: "dependent" })
    private dependentsAreLoading!: boolean;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Ref("newDependentModal")
    readonly newDependentModal!: NewDependentComponent;

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Dependents",
            to: "/dependents",
            active: true,
            dataTestId: "breadcrumb-dependents",
        },
    ];

    private get isLoading(): boolean {
        return this.dependentsAreLoading;
    }

    private get addDependentTutorialPreference(): string {
        return UserPreferenceType.TutorialAddDependent;
    }

    private async created(): Promise<void> {
        await this.retrieveDependents({
            hdid: this.user.hdid,
            bypassCache: false,
        });
    }

    private refreshDependents(): void {
        this.retrieveDependents({ hdid: this.user.hdid, bypassCache: true });
    }

    private showModal(): void {
        this.newDependentModal.showModal();
    }
}
</script>
<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <page-title title="Dependents">
            <hg-button
                id="add-dependent-button"
                data-testid="addNewDependentBtn"
                class="float-right"
                variant="secondary"
                @click="showModal()"
            >
                <hg-icon icon="user-plus" size="medium" class="mr-2" />
                <span>Add</span>
            </hg-button>
            <TutorialComponent
                :preference-type="addDependentTutorialPreference"
                target="add-dependent-button"
            >
                <div data-testid="add-dependent-tutorial-popover">
                    Add a dependent under 12 years old to get their health
                    records.
                </div>
            </TutorialComponent>
        </page-title>
        <DependentCardComponent
            v-for="dependent in dependents"
            :key="dependent.ownerId"
            :dependent="dependent"
            class="my-3"
        />
        <NewDependentComponent
            ref="newDependentModal"
            @show="showModal"
            @handle-submit="refreshDependents"
        />
    </div>
</template>
