<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

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

    private detailsVisible = false;

    private toggleDetails(): void {
        this.detailsVisible = !this.detailsVisible;
    }
}
</script>

<template>
    <b-row class="cardWrapper mb-1">
        <b-col class="timelineCard ml-0 ml-md-4">
            <b-row class="entryHeading px-2 py-3">
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
                            class="py-1"
                            :data-testid="entry.type.toLowerCase() + 'Title'"
                        >
                            <strong>{{ title }}</strong>
                        </b-col>
                        <b-col cols="auto">
                            <slot name="header-menu"> </slot>
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col class="text-muted">
                            <slot name="header-description">{{
                                subtitle
                            }}</slot>
                        </b-col>
                        <b-col class="rightPane align-self-end">
                            <div v-if="showDetailsButton">
                                <b-btn
                                    variant="link"
                                    data-testid="entryCardDetailsButton"
                                    class="detailsButton"
                                    @click="toggleDetails()"
                                >
                                    <span v-if="detailsVisible"
                                        >Hide Details</span
                                    >
                                    <span v-else>View Details</span>
                                </b-btn>
                            </div>
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
            <b-row>
                <b-col class="leftPane d-none d-md-block"></b-col>
                <b-col class="px-3">
                    <b-collapse
                        :id="'entryDetails-' + cardId"
                        v-model="detailsVisible"
                    >
                        <slot name="details-body"></slot>
                        <CommentSection
                            v-if="allowComment"
                            :parent-entry="entry"
                        />
                    </b-collapse>
                </b-col>
            </b-row>
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
$left-pane-width: 80px;
$right-pane-width: 90px;

.col {
    padding: 0px;
    margin: 0px;
}
.row {
    padding: 0;
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
    text-align: right;
}
</style>
