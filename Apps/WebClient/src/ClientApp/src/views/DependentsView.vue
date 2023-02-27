<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faUserPlus } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DependentCardComponent from "@/components/DependentCardComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import NewDependentComponent from "@/components/modal/NewDependentComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import UserPreferenceType from "@/constants/userPreferenceType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import type { Dependent } from "@/models/dependent";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(faUserPlus);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BreadcrumbComponent,
        LoadingComponent,
        DependentCardComponent,
        NewDependentComponent,
    },
};

@Component(options)
export default class DependentsView extends Vue {
    @Action("retrieveDependents", { namespace: "dependent" })
    retrieveDependents!: (params: {
        hdid: string;
        bypassCache: boolean;
    }) => Promise<void>;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("setTooManyRequestsWarning", { namespace: "errorBanner" })
    setTooManyRequestsWarning!: (params: { key: string }) => void;

    @Action("setUserPreference", { namespace: "user" })
    setUserPreference!: (params: {
        preference: UserPreference;
    }) => Promise<void>;

    @Getter("isMobile")
    isMobileView!: boolean;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Getter("dependents", { namespace: "dependent" })
    dependents!: Dependent[];

    @Getter("dependentsAreLoading", { namespace: "dependent" })
    dependentsAreLoading!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Ref("newDependentModal")
    readonly newDependentModal!: NewDependentComponent;

    private logger!: ILogger;

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Dependents",
            to: "/dependents",
            active: true,
            dataTestId: "breadcrumb-dependents",
        },
    ];

    private isAddDependentTutorialHidden = false;

    private get showAddDependentTutorial(): boolean {
        const preferenceType = UserPreferenceType.TutorialAddDependent;
        return (
            this.user.preferences[preferenceType]?.value === "true" &&
            !this.isAddDependentTutorialHidden
        );
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveDependents({ hdid: this.user.hdid, bypassCache: false });
    }

    private dismissAddDependentTutorial(): void {
        this.logger.debug("Dismissing add dependent tutorial");
        this.isAddDependentTutorialHidden = true;

        const preference = {
            ...this.user.preferences[UserPreferenceType.TutorialAddDependent],
            value: "false",
        };
        this.setUserPreference({ preference });
    }

    private showModal(): void {
        this.newDependentModal.showModal();
    }

    private hideModal(): void {
        this.newDependentModal.hideModal();
    }

    private refreshDependents(): void {
        this.retrieveDependents({ hdid: this.user.hdid, bypassCache: true });
    }
}
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
                @click="showModal()"
            >
                <hg-icon icon="user-plus" size="medium" class="mr-2" />
                <span>Add</span>
            </hg-button>
            <b-popover
                triggers="manual"
                :show="showAddDependentTutorial"
                target="add-dependent-button"
                :placement="isMobileView ? 'bottom' : 'left'"
                boundary="viewport"
            >
                <div>
                    <hg-button
                        class="float-right text-dark p-0 ml-2"
                        variant="icon"
                        @click="dismissAddDependentTutorial()"
                        >×</hg-button
                    >
                </div>
                <div data-testid="add-dependent-tutorial-popover">
                    Add a dependent under 12 years old to get their health
                    records.
                </div>
            </b-popover>
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
            @show="showModal"
            @handle-submit="refreshDependents"
        />
    </div>
</template>
