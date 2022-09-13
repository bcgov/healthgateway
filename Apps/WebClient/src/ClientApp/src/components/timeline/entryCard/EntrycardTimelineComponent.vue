<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faComment as farComment } from "@fortawesome/free-regular-svg-icons";
import { faPaperclip } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import UserPreferenceType from "@/constants/userPreferenceType";
import EventBus, { EventMessageName } from "@/eventbus";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

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

    @Action("setUserPreference", { namespace: "user" })
    setUserPreference!: (params: {
        preference: UserPreference;
    }) => Promise<void>;

    @Action("setSeenTutorialComment", { namespace: "user" })
    setSeenTutorialComment!: (params: { value: boolean }) => void;

    @Getter("isMobile")
    isMobileWidth!: boolean;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("seenTutorialComment", { namespace: "user" })
    seenTutorialComment!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    private logger!: ILogger;
    private eventBus = EventBus;
    private detailsVisible = false;
    private isCommentTutorialHidden = true;

    private get icon(): string {
        return this.entryIcon ?? "question";
    }

    private get isInteractive(): boolean {
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

    private get showCommentTutorial(): boolean {
        const preferenceType = UserPreferenceType.TutorialComment;
        return (
            this.user.preferences[preferenceType]?.value === "true" &&
            !this.isCommentTutorialHidden
        );
    }

    @Watch("isMobileWidth")
    private onMobileWidthChanged(): void {
        if (this.isMobileWidth && !this.isMobileDetails) {
            this.detailsVisible = false;
        }
    }

    @Watch("detailsVisible")
    private onDetailsVisibleChanged(): void {
        if (
            this.detailsVisible &&
            this.allowComment &&
            this.isCommentEnabled &&
            !this.seenTutorialComment
        ) {
            this.isCommentTutorialHidden = false;
            this.setSeenTutorialComment({ value: true });
        }
    }

    private mounted(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        if (this.isMobileDetails) {
            this.detailsVisible = true;
        }
    }

    private handleCardClick(): void {
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

    private dismissCommentTutorial(): void {
        this.logger.debug("Dismissing comment tutorial");
        this.isCommentTutorialHidden = true;

        const preference = {
            ...this.user.preferences[UserPreferenceType.TutorialComment],
            value: "false",
        };
        this.setUserPreference({ preference });
    }
}
</script>

<template>
    <b-row class="cardWrapper mb-1">
        <b-col class="timelineCard">
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
                        <b-popover
                            triggers="manual"
                            :show="showCommentTutorial"
                            :target="'tooltip-' + entry.id"
                            placement="topright"
                            boundary="viewport"
                        >
                            <div>
                                <hg-button
                                    class="float-right text-dark p-0 ml-2"
                                    variant="icon"
                                    @click="dismissCommentTutorial()"
                                    >×</hg-button
                                >
                            </div>
                            <div data-testid="comment-tutorial-popover">
                                You can add comments to help you keep track of
                                important health details. Only you can see them.
                            </div>
                        </b-popover>
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
