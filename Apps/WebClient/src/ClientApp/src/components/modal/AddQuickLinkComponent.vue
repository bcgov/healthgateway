<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { EntryType, entryTypeMap } from "@/constants/entryType";
import type { WebClientConfiguration } from "@/models/configData";
import { QuickLink } from "@/models/quickLink";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

interface QuickLinkFilter {
    name: string;
    module: EntryType;
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
    }) => Promise<void>;

    @Getter("quickLinks", { namespace: "user" }) quickLinks!:
        | QuickLink[]
        | undefined;

    private logger!: ILogger;
    private checkboxComponentKey = 0;
    private isVisible = false;

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
        return (
            this.enabledQuickLinkFilter.length > 0 &&
            this.selectedQuickLinks.length > 0
        );
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

    private forceCheckboxComponentRerender(): void {
        this.logger.debug(
            `Checkbox Component Key before force re-render: ${this.checkboxComponentKey}`
        );
        this.checkboxComponentKey++;
        this.logger.debug(
            `Checkbox Component Key after force re-render: ${this.checkboxComponentKey}`
        );
    }

    private handleCancel(modalEvt: Event) {
        // Prevent modal from closing
        modalEvt.preventDefault();

        // Clear selected quick links
        this.selectedQuickLinks = [];

        // Force checkbox conponent to re-render
        this.forceCheckboxComponentRerender();

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
        const quickLinks: QuickLink[] = [];
        this.selectedQuickLinks.forEach((module) => {
            const details = entryTypeMap.get(module as EntryType);
            if (details) {
                quickLinks.push({
                    name: details.name,
                    filter: { modules: [module] },
                });
            }
        });

        this.addQuickLink(quickLinks);

        // Force checkbox conponent to re-render
        this.forceCheckboxComponentRerender();

        // Clear selected quick links
        this.selectedQuickLinks = [];

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
                        :value="quickLink.module"
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
                        :disabled="!isAddButtonEnabled"
                        @click="handleSubmit"
                        >Add to Home</hg-button
                    >
                </div>
            </b-row>
        </template>
    </b-modal>
</template>
