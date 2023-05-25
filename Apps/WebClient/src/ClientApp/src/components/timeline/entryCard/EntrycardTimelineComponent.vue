<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faComment as farComment } from "@fortawesome/free-regular-svg-icons";
import { faPaperclip } from "@fortawesome/free-solid-svg-icons";
import { computed, onMounted, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import EventBus, { EventMessageName } from "@/eventbus";
import TimelineEntry from "@/models/timelineEntry";

import CommentSectionComponent from "./CommentSectionComponent.vue";

library.add(farComment, faPaperclip);

interface Props {
    cardId: string;
    entry: TimelineEntry;
    entryIcon: string;
    title: string;
    allowComment?: boolean;
    canShowDetails?: boolean;
    hasAttachment?: boolean;
    iconClass?: string;
    isMobileDetails?: boolean;
    subtitle?: string;
}
const props = withDefaults(defineProps<Props>(), {
    allowComment: false,
    canShowDetails: true,
    hasAttachment: false,
    iconClass: "",
    isMobileDetails: false,
    subtitle: "",
});

const store = useStore();

const detailsVisible = ref(false);

const isMobileWidth = computed<boolean>(() => store.getters["isMobile"]);
const isInteractive = computed(
    () =>
        (!isMobileWidth.value && props.canShowDetails) ||
        (isMobileWidth.value && !props.isMobileDetails)
);
const displayTitle = computed(() =>
    props.title === "" ? props.entry.type.toString() : props.title
);
const dateString = computed(() => props.entry.date.format());
const commentCount = computed(() => props.entry.comments?.length ?? 0);

function handleCardClick(): void {
    if (isMobileWidth.value) {
        if (!props.isMobileDetails) {
            EventBus.$emit(EventMessageName.ViewEntryDetails, props.entry);
        }
    } else {
        if (props.canShowDetails) {
            detailsVisible.value = !detailsVisible.value;
        }
    }
}

watch(isMobileWidth, () => {
    if (isMobileWidth.value && !props.isMobileDetails) {
        detailsVisible.value = false;
    }
});

onMounted(() => {
    if (props.isMobileDetails) {
        detailsVisible.value = true;
    }
});
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
                        <hg-icon :icon="entryIcon" size="large" fixed-width />
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
                        <b-col cols="auto" class="text-right text-nowrap ml-3">
                            <span
                                class="text-muted small"
                                data-testid="entryCardDate"
                                >{{ dateString }}</span
                            >
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col
                            class="py-1 text-muted align-self-center"
                            data-testid="entryCardDetailsSubtitle"
                        >
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
                <b-row v-if="allowComment">
                    <b-col class="leftPane d-none d-md-block" />
                    <b-col class="pb-1 pt-1 px-3">
                        <CommentSectionComponent
                            :parent-entry="entry"
                            :is-mobile-details="isMobileDetails"
                            :visible="detailsVisible"
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
    print-color-adjust: exact;
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
