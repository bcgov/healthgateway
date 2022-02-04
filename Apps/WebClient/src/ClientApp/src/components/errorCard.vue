<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCopy as farCopy } from "@fortawesome/free-regular-svg-icons";
import { faChevronDown, faChevronUp } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import VueClipboard from "vue-clipboard2";
import { Component, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/genericMessage.vue";
import BannerError from "@/models/bannerError";
import { DateWrapper } from "@/models/dateWrapper";
import User from "@/models/user";

library.add(faChevronDown, faChevronUp, farCopy);
Vue.use(VueClipboard);

@Component({
    components: {
        MessageModalComponent,
    },
})
export default class ErrorCardComponent extends Vue {
    @Action("dismiss", {
        namespace: "errorBanner",
    })
    dismissBanner!: () => void;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isShowing", { namespace: "errorBanner" })
    isShowing!: boolean;

    @Getter("errors", { namespace: "errorBanner" })
    errors!: BannerError[];

    @Ref("copyToClipBoardModal")
    readonly copyToClipBoardModal!: MessageModalComponent;

    public get haveError(): boolean {
        if (this.errors !== undefined && this.errors.length > 0) {
            return true;
        }
        return false;
    }
    private get haveMultipleErrors(): boolean {
        if (this.haveError) {
            return this.errorDetails.length > 1;
        }
        return false;
    }

    private get errorTitle(): string {
        let title = "";
        if (this.errorDetails !== undefined && this.errorDetails.length === 1) {
            title = this.errors[0].title;
        }
        return title;
    }

    private get errorDetails(): string[] {
        let result: string[] = [];
        let isoNow = new DateWrapper().format("yyyy-MMM-dd");
        let hdid = this.user.hdid !== undefined ? this.user.hdid : "";
        for (var error of this.errors.filter((c) => c.traceId !== undefined)) {
            let source = error.source !== undefined ? error.source.trim() : "";
            let traceId =
                error.traceId !== undefined ? error.traceId.trim() : "";
            result.push(`${source}/${isoNow}/${hdid}:${traceId}`);
        }
        return result;
    }

    private get errorDetailsCopyToClipboard(): string {
        var errorDetails = "";
        if (this.haveError) {
            errorDetails = this.errorDetails.join("\n");
        }
        return errorDetails;
    }

    private onCopy() {
        this.copyToClipBoardModal.showModal();
    }
}
</script>

<template>
    <b-alert
        data-testid="errorBanner"
        variant="danger"
        dismissible
        class="no-print m-3 m-md-4"
        :show="isShowing"
        @dismissed="dismissBanner"
    >
        <div>
            <div v-if="haveMultipleErrors" data-testid="multipleErrorsHeader">
                <h4>Multiple errors have occurred</h4>
            </div>
            <div v-if="!haveMultipleErrors" data-testid="singleErrorHeader">
                <h4>{{ errorTitle }}</h4>
            </div>
            <hg-button
                v-b-toggle.errorDetails
                data-testid="errorDetailsBtn"
                variant="link"
                class="detailsButton"
            >
                <hg-icon
                    icon="chevron-down"
                    size="medium"
                    aria-hidden="true"
                    class="when-closed mr-2"
                    data-testid="viewDetailsIcon"
                />
                <span class="when-closed">View Details</span>
                <hg-icon
                    icon="chevron-up"
                    size="medium"
                    aria-hidden="true"
                    class="when-opened mr-2"
                    data-testid="hideDetailsIcon"
                />
                <span class="when-opened">Hide Details</span>
            </hg-button>
            <b-collapse id="errorDetails">
                <div class="py-2">
                    <p>
                        Try refreshing the page. If this issue persists, contact
                        HealthGateway@gov.bc.ca and provide
                        <hg-button
                            v-clipboard:copy="errorDetailsCopyToClipboard"
                            v-clipboard:success="onCopy"
                            variant="secondary"
                            data-testid="copyToClipBoardBtn"
                        >
                            <hg-icon :icon="['far', 'copy']" size="small" />
                            Copy
                        </hg-button>
                    </p>
                    <p
                        v-for="(error, index) in errorDetails"
                        :key="index"
                        class="break-word"
                        :data-testid="'error-details-span-' + (index + 1)"
                    >
                        {{ error }}
                    </p>
                </div>
            </b-collapse>
            <MessageModalComponent
                ref="copyToClipBoardModal"
                title="Copy to Clipboard"
                message="Copied Successfully"
                :ok-only="true"
            />
        </div>
    </b-alert>
</template>

<style lang="scss" scoped>
.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}
.break-word {
    word-wrap: break-word;
}
</style>
