<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import UserPreferenceType from "@/constants/userPreferenceType";
import type { WebClientConfiguration } from "@/models/configData";
import { BannerError } from "@/models/errors";
import { QuickLink } from "@/models/quickLink";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";
import PromiseUtility from "@/utility/promiseUtility";

interface QuickLinkFilter {
    name: string;
    module: EntryType;
}

@Component({
    components: {},
})
export default class AddQuickLinkComponent extends Vue {
    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Action("updateQuickLinks", { namespace: "user" })
    updateQuickLinks!: (params: {
        hdid: string;
        quickLinks: QuickLink[];
    }) => Promise<void>;

    @Action("updateUserPreference", { namespace: "user" })
    updateUserPreference!: (params: {
        userPreference: UserPreference;
    }) => Promise<void>;

    @Getter("quickLinks", { namespace: "user" })
    quickLinks!: QuickLink[] | undefined;

    private logger!: ILogger;
    private checkboxComponentKey = 0;
    private isVisible = false;
    private bannerError: BannerError | null = null;
    private isLoading = false;

    private selectedQuickLinks: string[] = [];
    private quickLinkFilter: QuickLinkFilter[] = [...entryTypeMap.values()].map(
        (details) => ({
            name: details.name,
            module: details.type,
        })
    );

    private get enabledQuickLinkFilter(): QuickLinkFilter[] {
        return this.quickLinkFilter.filter(
            (filter) =>
                this.webClientConfig.modules[filter.module] &&
                this.quickLinks?.find(
                    (existingLink) =>
                        existingLink.filter.modules.length === 1 &&
                        existingLink.filter.modules[0] === filter.module
                ) === undefined
        );
    }

    private get isAddButtonEnabled(): boolean {
        return this.selectedQuickLinks.length > 0 && !this.isLoading;
    }

    private get showVaccineCard(): boolean {
        const preference =
            this.user.preferences[UserPreferenceType.HideVaccineCardQuickLink];
        return (
            preference?.value === "true" &&
            this.webClientConfig.modules["VaccinationStatus"]
        );
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private forceCheckboxComponentRerender(): void {
        this.checkboxComponentKey++;
    }

    private handleCancel(modalEvt: Event): void {
        this.bannerError = null;

        // Prevent modal from closing
        modalEvt.preventDefault();

        // Clear selected quick links
        this.selectedQuickLinks = [];

        // Force checkbox component to re-render
        this.forceCheckboxComponentRerender();

        // Hide the modal manually
        this.$nextTick(() => this.hideModal());
    }

    private async handleSubmit(modalEvt: Event): Promise<void> {
        this.bannerError = null;

        // Prevent modal from closing
        modalEvt.preventDefault();

        this.selectedQuickLinks.forEach((element) =>
            this.logger.debug(`Adding quick link:  ${element}`)
        );

        const quickLinks: QuickLink[] = this.quickLinks ?? [];
        this.selectedQuickLinks.forEach((module) => {
            const details = entryTypeMap.get(module as EntryType);
            if (details) {
                quickLinks.push({
                    name: details.name,
                    filter: { modules: [module] },
                });
            }
        });

        try {
            this.isLoading = true;

            const promises = [
                this.updateQuickLinks({ hdid: this.user.hdid, quickLinks }),
            ];

            if (this.selectedQuickLinks.includes("bc-vaccine-card")) {
                const userPreference = {
                    ...this.user.preferences[
                        UserPreferenceType.HideVaccineCardQuickLink
                    ],
                    value: "false",
                };

                promises.push(
                    this.updateUserPreference({ userPreference }).then(() => {
                        this.selectedQuickLinks =
                            this.selectedQuickLinks.filter(
                                (link) => link !== "bc-vaccine-card"
                            );
                    })
                );
            }

            await PromiseUtility.withMinimumDelay(Promise.all(promises), 1000);

            // Hide the modal manually
            await this.$nextTick();
            this.hideModal();
        } catch (error) {
            this.bannerError = ErrorTranslator.toBannerError(
                ErrorType.Update,
                ErrorSourceType.QuickLinks,
                undefined
            );
        } finally {
            // Force checkbox component to re-render
            this.forceCheckboxComponentRerender();

            this.isLoading = false;
        }
    }

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }
}
</script>

<template>
    <b-modal
        id="add-quick-link-modal"
        v-model="isVisible"
        data-testid="add-quick-link-modal"
        content-class="mt-5"
        title="Add a quick link to a record type"
        size="lg"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
        @close="handleCancel"
    >
        <form data-testid="quick-link-modal-text">
            <b-alert
                v-if="bannerError"
                data-testid="quick-link-modal-error"
                variant="danger"
                dismissible
                show
            >
                <p>{{ bannerError.title }}</p>
                <span>
                    If you continue to have issues, please contact
                    HealthGateway@gov.bc.ca.
                </span>
            </b-alert>
            <b-row
                v-for="quickLink in enabledQuickLinkFilter"
                :key="quickLink.module"
            >
                <b-col cols="8" align-self="start">
                    <b-form-checkbox
                        :id="quickLink.module + '-filter'"
                        :key="checkboxComponentKey"
                        v-model="selectedQuickLinks"
                        :data-testid="`${quickLink.module}-filter`"
                        :name="quickLink.module + '-filter'"
                        :value="quickLink.module"
                    >
                        {{ quickLink.name }}
                    </b-form-checkbox>
                </b-col>
            </b-row>
            <b-row v-if="showVaccineCard">
                <b-col cols="8" align-self="start">
                    <b-form-checkbox
                        id="bc-vaccine-card-filter"
                        :key="checkboxComponentKey"
                        v-model="selectedQuickLinks"
                        data-testid="bc-vaccine-card-filter"
                        name="bc-vaccine-card-filter"
                        value="bc-vaccine-card"
                    >
                        BC Vaccine Card
                    </b-form-checkbox>
                </b-col>
            </b-row>
        </form>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <hg-button
                        data-testid="cancel-add-quick-link-btn"
                        variant="secondary"
                        @click="handleCancel"
                        >Cancel</hg-button
                    >
                </div>
                <div>
                    <hg-button
                        data-testid="add-quick-link-btn"
                        variant="primary"
                        :disabled="!isAddButtonEnabled"
                        @click="handleSubmit"
                    >
                        <b-spinner v-if="isLoading" small class="mr-2" />
                        <span>Add to Home</span>
                    </hg-button>
                </div>
            </b-row>
        </template>
    </b-modal>
</template>
