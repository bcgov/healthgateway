<script lang="ts">
import Vue from "vue";
import { Component, Emit } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import type { WebClientConfiguration } from "@/models/configData";
import { QuickLink } from "@/models/quickLink";
import { EntryType } from "@/models/timelineEntry";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ClientModule } from "@/router";
import { ILogger } from "@/services/interfaces";

interface QuickLinkFilter {
    name: string;
    module: string;
    isEnabled: boolean;
}

@Component({
    components: {},
})
export default class AddQuickLinkComponent extends Vue {
    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Action("updateQuickLinks", { namespace: "user" })
    updateQuickLinks!: (params: {
        hdid: string;
        quickLinks: QuickLink[];
    }) => void;

    @Getter("quickLinks", { namespace: "user" }) quickLinks!:
        | QuickLink[]
        | undefined;

    private logger!: ILogger;
    private checkboxComponentKey = 0;
    private isVisible = false;

    private selectedQuickLinks: string[] = [];
    private enabledQuickLinks: QuickLinkFilter[] = [];

    private get quickLinkFilter(): QuickLinkFilter[] {
        return [
            {
                name: "Health Visits",
                module: ClientModule.Encounter,
                isEnabled: this.webClientConfig.modules[EntryType.Encounter],
            },
            {
                name: "COVID-19 Tests",
                module: ClientModule.Laboratory,
                isEnabled:
                    this.webClientConfig.modules[
                        EntryType.Covid19LaboratoryOrder
                    ],
            },
            {
                name: "My Notes",
                module: ClientModule.Note,
                isEnabled: this.webClientConfig.modules[EntryType.Note],
            },
            {
                name: "Special Authority",
                module: ClientModule.MedicationRequest,
                isEnabled:
                    this.webClientConfig.modules[EntryType.MedicationRequest],
            },
        ];
    }

    private get enabledQuickLinkFilter(): QuickLinkFilter[] {
        this.logger.debug(
            `Enabled Quick Links - checkbox component key:  ${this.checkboxComponentKey}`
        );
        if (this.checkboxComponentKey != 0) {
            return this.enabledQuickLinks;
        } else {
            this.logger.debug(
                `Enabled Quick Links - quick links:  ${this.quickLinks}`
            );
            if (this.quickLinks != undefined) {
                this.populateEnabledQuickLinks();
                const existingQuickLinks = this.quickLinks;
                return this.getAvailabeQuickLinks(
                    existingQuickLinks,
                    this.enabledQuickLinks
                );
            }
            this.populateEnabledQuickLinks();
            return this.enabledQuickLinks;
        }
    }

    private get isAddButtonDisabled(): boolean {
        return (
            this.enabledQuickLinkFilter.length == 0 ||
            (this.enabledQuickLinkFilter.length > 0 &&
                this.selectedQuickLinks.length == 0)
        );
    }

    private get isCheckBoxChecked(): boolean {
        return this.enabledQuickLinkFilter.length == 0;
    }

    private addQuickLink(submittedQuickLinks: QuickLink[]): Promise<void> {
        const quickLinks: QuickLink[] = (this.quickLinks ?? []).concat(
            submittedQuickLinks
        );
        return this.updateQuickLinks({
            hdid: this.user.hdid,
            quickLinks: quickLinks,
        });
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private getAvailabeQuickLinks(
        existingQuickLinks: QuickLink[],
        enabledQuickLinks: QuickLinkFilter[]
    ): QuickLinkFilter[] {
        existingQuickLinks.forEach((element) =>
            this.logger.debug(
                `Available Quick Links - Existing store quick links:  ${element}`
            )
        );

        enabledQuickLinks.forEach((element) =>
            this.logger.debug(
                `Available Quick Links - Enabled quick links:  ${element}`
            )
        );

        let result: QuickLinkFilter[] = [];
        result = enabledQuickLinks.filter((enabledQL) => {
            return !existingQuickLinks.find((existingQL) => {
                return existingQL.name === enabledQL.name;
            });
        });
        return result;
    }

    private getCheckedValue(quickLink: QuickLinkFilter): string {
        return (
            '{ "name": "' +
            quickLink.name +
            '", "filter": { "modules": [ "' +
            quickLink.module +
            '"] }}'
        );
    }

    private populateEnabledQuickLinks(): void {
        this.enabledQuickLinks = this.quickLinkFilter.filter(
            (filter: QuickLinkFilter) => filter.isEnabled
        );
        this.logger.debug(`Enabled quick links:  ${this.enabledQuickLinks}`);
    }

    private forceCheckboxComponentRerender(): void {
        this.logger.debug(
            `Checkbox Component Key before force re-render: ${this.checkboxComponentKey}`
        );
        this.checkboxComponentKey++;
        this.logger.debug(
            `Checkbox Component Key after force re-render: ${this.checkboxComponentKey}`
        );
    }

    private removeSelectedQuickLink(quickLinkName: string) {
        this.logger.debug(`Removing quick link: ${quickLinkName}`);
        const index = this.enabledQuickLinks.findIndex((element) => {
            return element.name === quickLinkName;
        });
        this.logger.debug(
            `Removing quick link: ${quickLinkName} at index: ${index}`
        );

        if (index >= 0) {
            this.enabledQuickLinks.splice(index, 1);
            this.logger.debug(
                `Removed quick link: ${quickLinkName} at index: ${index}`
            );
        }
    }

    private handleCancel(modalEvt: Event) {
        // Prevent modal from closing
        modalEvt.preventDefault();

        // Clear selected quick links
        this.selectedQuickLinks = [];

        // Force checkbox conponent to re-render
        this.forceCheckboxComponentRerender();

        // Trigger cancel handler
        this.cancel();

        // Hide the modal manually
        this.$nextTick(() => {
            this.hideModal();
        });
    }

    private handleSubmit(modalEvt: Event) {
        // Prevent modal from closing
        modalEvt.preventDefault();

        this.selectedQuickLinks.forEach((element) =>
            this.logger.debug(`Adding quick link:  ${element}`)
        );

        // Update quick link preferences in store
        const quickLinks: QuickLink[] = this.selectedQuickLinks.map((element) =>
            JSON.parse(element)
        );
        this.addQuickLink(quickLinks);

        // Remove selected quick link from display list
        quickLinks.forEach((element) =>
            this.removeSelectedQuickLink(element.name)
        );

        // Force checkbox conponent to re-render
        this.forceCheckboxComponentRerender();

        // Clear selected quick links
        this.selectedQuickLinks = [];

        // Trigger submit handler
        this.submit();

        // Hide the modal manually
        this.$nextTick(() => {
            this.hideModal();
        });
    }

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    @Emit()
    private submit() {
        return;
    }

    @Emit()
    private cancel() {
        return;
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
            <b-row
                v-for="(quickLink, index) in enabledQuickLinkFilter"
                :key="index"
            >
                <b-col cols="8" align-self="start">
                    <b-form-checkbox
                        :id="quickLink.module + '-filter'"
                        :key="checkboxComponentKey"
                        v-model="selectedQuickLinks"
                        :data-testid="`${quickLink.module}-filter`"
                        :name="quickLink.module + '-filter'"
                        :value="getCheckedValue(quickLink)"
                    >
                        {{ quickLink.name }}
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
                        :disabled="isAddButtonDisabled"
                        @click="handleSubmit"
                        >Add to Home</hg-button
                    >
                </div>
            </b-row>
        </template>
    </b-modal>
</template>
