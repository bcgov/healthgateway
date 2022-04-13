<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faComment as farComment } from "@fortawesome/free-regular-svg-icons";
import { faPaperclip } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";

import EventBus, { EventMessageName } from "@/eventbus";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";

import CommentSectionComponent from "./CommentSectionComponent.vue";

library.add(farComment, faPaperclip);

@Component({
    components: {
        CommentSection: CommentSectionComponent,
    },
})
export default class EntrycardTimelineComponent extends Vue {
    @Prop() entry!: TimelineEntry;
    @Prop() cardId!: string;
    @Prop() title!: string;
    @Prop() subtitle!: string;
    @Prop() entryIcon: string | undefined;
    @Prop() iconClass!: string;
    @Prop({ default: true }) allowComment!: boolean;
    @Prop({ default: true }) canShowDetails!: boolean;
    @Prop({ default: false }) isMobileDetails!: boolean;
    @Prop({ default: false }) hasAttachment!: boolean;
    @Getter("isMobile") isMobileWidth!: boolean;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    private eventBus = EventBus;
    private detailsVisible = false;

    private get icon(): string {
        return this.entryIcon ?? "question";
    }

    private get isInteractive() {
        return (
            (!this.isMobileWidth && this.canShowDetails) ||
            (this.isMobileWidth && !this.isMobileDetails)
        );
    }

    private get displayTitle(): string {
        if (this.title === "") {
            return this.entry.type.toString();
        } else {
            return this.title;
        }
    }

    private get dateString(): string {
        const today = new DateWrapper();
        if (this.entry.date.isSame(today, "day")) {
            return "Today";
        } else if (this.entry.date.year() === today.year()) {
            return this.entry.date.format("MMM d");
        } else {
            return this.entry.date.format();
        }
    }

    private get isCommentEnabled(): boolean {
        return this.config.modules["Comment"];
    }

    private get commentCount(): number {
        return this.entry.comments !== null ? this.entry.comments.length : 0;
    }

    @Watch("isMobileWidth")
    private onMobileWidthChanged() {
        if (this.isMobileWidth && !this.isMobileDetails) {
            this.detailsVisible = false;
        }
    }

    private mounted() {
        if (this.isMobileDetails) {
            this.detailsVisible = true;
        }
    }

    private handleCardClick() {
        if (this.isMobileWidth) {
            if (!this.isMobileDetails) {
                this.eventBus.$emit(
                    EventMessageName.ViewEntryDetails,
                    this.entry
                );
            }
        } else {
            if (this.canShowDetails) {
                this.detailsVisible = !this.detailsVisible;
            }
        }
    }
}
</script>

<template>
    <b-row class="cardWrapper mb-1">
        <b-col class="timelineCard ml-0 ml-md-2">
            <b-row
                class="entryHeading px-3 py-2"
                :class="{
                    mobileDetail: isMobileDetails,
                    interactive: isInteractive,
                }"
                @click="handleCardClick()"
            >
                <b-col class="leftPane">
                    <div class="icon" :class="iconClass">
                        <hg-icon :icon="icon" size="large" fixed-width />
                    </div>
                </b-col>
                <b-col class="entryTitleWrapper">
                    <b-row>
                        <b-col
                            :data-testid="`${entry.type.toLowerCase()}Title`"
                        >
                            <span
                                data-testid="entryCardDetailsTitle"
                                class="entry-title"
                            >
                                <strong>{{ displayTitle }}</strong>
                            </span>
                        </b-col>
                        <b-col cols="4" class="text-right">
                            <span
                                class="text-muted"
                                data-testid="entryCardDate"
                                >{{ dateString }}</span
                            >
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col class="py-1 text-muted align-self-center">
                            <slot name="header-description">
                                {{ subtitle }}
                            </slot>
                        </b-col>
                        <b-col cols="4" class="text-right align-self-center">
                            <span
                                v-if="commentCount > 1"
                                class="pr-2"
                                data-testid="commentCount"
                                >{{ commentCount }}</span
                            >
                            <span v-if="commentCount > 0">
                                <hg-icon
                                    :icon="['far', 'comment']"
                                    size="small"
                                    data-testid="commentIcon"
                                />
                            </span>
                            <span v-if="hasAttachment" class="ml-1">
                                <hg-icon
                                    size="small"
                                    icon="paperclip"
                                    data-testid="attachmentIcon"
                                />
                            </span>
                            <slot name="header-menu" />
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
            <b-collapse :id="'entryDetails-' + cardId" v-model="detailsVisible">
                <b-row>
                    <b-col class="leftPane d-none d-md-block" />
                    <b-col class="pb-1 pt-1 px-3">
                        <slot name="details-body" />
                    </b-col>
                </b-row>
                <b-row v-if="allowComment && isCommentEnabled">
                    <b-col class="leftPane d-none d-md-block" />
                    <b-col class="pb-1 pt-1 px-3">
                        <CommentSection
                            :parent-entry="entry"
                            :is-mobile-details="isMobileDetails"
                        />
                    </b-col>
                </b-row>
            </b-collapse>
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
$left-pane-width: 80px;
$right-pane-width: 90px;

div[class^="col"],
div[class*=" col"] {
    padding: 0px;
    margin: 0px;
}

div[class^="row"],
div[class*=" row"] {
    padding: 0px;
    margin: 0px;
}

.cardWrapper {
    width: 100%;
    min-width: 100%;
}

.timelineCard {
    border-color: $soft_background;
    border-style: solid;
    border-width: 2px;
}

.leftPane {
    width: $left-pane-width;
    max-width: $left-pane-width;
}

.rightPane {
    text-align: center;
    width: $right-pane-width;
    max-width: $right-pane-width;
}

.entryHeading {
    background-color: $soft_background;

    &.interactive {
        &:hover,
        &:active,
        &:focus,
        &:focus-visible {
            background-color: #ebebeb;
            cursor: pointer;

            .entry-title {
                text-decoration: underline;
            }
        }
    }
}

.mobileDetail {
    background-color: white;
}

.entryTitleWrapper {
    color: $primary;
    text-align: left;
}

.icon {
    background-color: $primary;
    color: white;
    color-adjust: exact;
    text-align: center;
    border-radius: 50%;
    height: 60px;
    width: 60px;
    padding-top: 17px;
}

.detailsButton {
    padding: 0px;
    color: $primary;
}
</style>
