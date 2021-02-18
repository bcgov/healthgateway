<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";

import CommentSectionComponent from "./commentSection.vue";

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
    @Prop() entryIcon!: string;
    @Prop() iconClass!: string;
    @Prop({ default: true }) allowComment!: boolean;
    @Prop({ default: true }) showDetailsButton!: boolean;

    private get displayTitle(): string {
        if (this.title === "") {
            return this.entry.type.toString();
        } else {
            return this.title;
        }
    }

    private detailsVisible = false;

    private get dateString(): string {
        const today = new DateWrapper();
        if (this.entry.date.isSame(today, "day")) {
            return "Today";
        } else if (this.entry.date.year() === today.year()) {
            return this.entry.date.format("MMM d");
        } else {
            return this.entry.date.format("yyyy-MM-dd");
        }
    }

    private toggleDetails(): void {
        this.detailsVisible = !this.detailsVisible;
    }
}
</script>

<template>
    <b-row class="cardWrapper mb-1">
        <b-col class="timelineCard ml-0 ml-md-2">
            <b-row class="entryHeading px-3 py-2">
                <b-col class="leftPane">
                    <div class="icon" :class="iconClass">
                        <font-awesome-icon
                            :icon="entryIcon"
                            size="lg"
                        ></font-awesome-icon>
                    </div>
                </b-col>
                <b-col class="entryTitleWrapper">
                    <b-row>
                        <b-col
                            :data-testid="entry.type.toLowerCase() + 'Title'"
                        >
                            <b-btn
                                v-if="showDetailsButton"
                                variant="link"
                                data-testid="entryCardDetailsButton"
                                class="detailsButton text-left"
                                @click="toggleDetails()"
                            >
                                <strong>{{ displayTitle }}</strong>
                            </b-btn>
                            <strong v-else>{{ displayTitle }}</strong>
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
                            <slot name="header-description">{{
                                subtitle
                            }}</slot>
                        </b-col>
                        <b-col cols="4" class="text-right align-self-center">
                            <slot name="header-menu"> </slot>
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
            <b-collapse :id="'entryDetails-' + cardId" v-model="detailsVisible">
                <b-row>
                    <b-col class="leftPane d-none d-md-block"></b-col>
                    <b-col class="pb-3 pt-1 px-3">
                        <slot name="details-body"></slot>
                        <CommentSection
                            v-if="allowComment"
                            :parent-entry="entry"
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
}

.entryTitleWrapper {
    color: $primary;
    text-align: left;
}

.icon {
    background-color: $primary;
    color: white;
    text-align: center;
    border-radius: 50%;
    height: 60px;
    width: 60px;
    padding-top: 17px;
    font-size: 1.2em;
}

.detailsButton {
    padding: 0px;
    color: $primary;
}
</style>
