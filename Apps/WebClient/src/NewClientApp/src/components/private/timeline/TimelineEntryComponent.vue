<script setup lang="ts">
import { computed, ref, watch } from "vue";

import CommentSectionComponent from "@/components/private/timeline/comment/CommentSectionComponent.vue";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { useAppStore } from "@/stores/app";
import { EventName, useEventStore } from "@/stores/event";

interface Props {
    cardId: string;
    entry: TimelineEntry;
    title: string;
    entryIcon?: string;
    allowComment?: boolean;
    canShowDetails?: boolean;
    hasAttachment?: boolean;
    iconClass?: string;
    isMobileDetails?: boolean;
    subtitle?: string;
}
const props = withDefaults(defineProps<Props>(), {
    entryIcon: "question",
    allowComment: false,
    canShowDetails: true,
    hasAttachment: false,
    iconClass: "",
    isMobileDetails: false,
    subtitle: "",
});

const appStore = useAppStore();
const eventStore = useEventStore();

const detailsVisible = ref(props.isMobileDetails);

const isInteractive = computed(
    () =>
        (!appStore.isMobile && props.canShowDetails) ||
        (appStore.isMobile && !props.isMobileDetails)
);
const displayTitle = computed(() =>
    props.title === "" ? props.entry.type.toString() : props.title
);
const dateString = computed(() => props.entry.date.format());
const commentCount = computed(() => props.entry.comments?.length ?? 0);

function handleCardClick(): void {
    if (appStore.isMobile && !props.isMobileDetails) {
        eventStore.emit(EventName.OpenFullscreenTimelineEntry, props.entry);
    } else if (!appStore.isMobile && props.canShowDetails) {
        detailsVisible.value = !detailsVisible.value;
    }
}

watch(
    () => appStore.isMobile,
    (value) => {
        if (value && !props.isMobileDetails) {
            detailsVisible.value = false;
        }
    }
);
</script>

<template>
    <v-expansion-panels
        :model-value="detailsVisible ? 0 : undefined"
        :class="{ 'h-100': isMobileDetails, 'mb-2': !isMobileDetails }"
    >
        <v-expansion-panel
            :elevation="isMobileDetails ? 0 : undefined"
            :rounded="0"
        >
            <v-expansion-panel-title
                :class="{
                    'no-pointer-events': !isInteractive,
                    'no-header-highlight': isMobileDetails,
                }"
                readonly
                hide-actions
                @click="handleCardClick"
            >
                <v-row>
                    <v-col cols="auto">
                        <div
                            class="timeline-entry-header-icon rounded-circle pa-4 d-flex justify-center"
                            :class="iconClass"
                        >
                            <v-icon
                                :icon="entryIcon"
                                color="white"
                                size="x-large"
                            />
                        </div>
                    </v-col>
                    <v-col>
                        <v-row dense>
                            <v-col
                                :data-testid="`${entry.type.toLowerCase()}Title`"
                            >
                                <h2
                                    data-testid="entryCardDetailsTitle"
                                    class="text-subtitle-1 font-weight-bold"
                                >
                                    {{ displayTitle }}
                                </h2>
                            </v-col>
                            <v-col
                                cols="auto"
                                data-testid="entryCardDate"
                                class="text-body-2 text-no-wrap"
                            >
                                {{ dateString }}
                            </v-col>
                        </v-row>
                        <v-row dense justify="end">
                            <v-col
                                class="py-1 align-self-center"
                                data-testid="entryCardDetailsSubtitle"
                            >
                                <slot name="header-description">
                                    {{ subtitle }}
                                </slot>
                            </v-col>
                            <v-col cols="auto" class="text-primary">
                                <span
                                    v-if="commentCount > 1"
                                    class="pr-2"
                                    data-testid="commentCount"
                                >
                                    {{ commentCount }}
                                </span>
                                <v-icon
                                    v-if="commentCount > 0"
                                    icon="far fa-comment"
                                    size="x-small"
                                    data-testid="commentIcon"
                                />
                                <v-icon
                                    v-if="hasAttachment"
                                    class="ml-2"
                                    icon="paperclip"
                                    size="x-small"
                                    data-testid="attachmentIcon"
                                />
                                <span class="all-pointer-events ml-2">
                                    <slot name="header-menu" />
                                </span>
                            </v-col>
                        </v-row>
                    </v-col>
                </v-row>
            </v-expansion-panel-title>
            <v-expansion-panel-text>
                <div class="timeline-entry-content">
                    <slot />
                    <CommentSectionComponent
                        v-if="allowComment"
                        :parent-entry="entry"
                        :is-mobile-details="isMobileDetails"
                    />
                </div>
            </v-expansion-panel-text>
        </v-expansion-panel>
    </v-expansion-panels>
</template>

<style lang="scss" scoped>
.timeline-entry-header-icon {
    height: 4rem;
    width: 4rem;
}

.timeline-entry-content {
    // icon size + icon padding + col padding
    margin-left: calc(4rem + 24px);
}

.no-pointer-events {
    pointer-events: none;
}

.all-pointer-events {
    pointer-events: all;
}
</style>

<style lang="scss">
.no-pointer-events.v-expansion-panel-title:hover
    > .v-expansion-panel-title__overlay {
    opacity: 0;
}

.no-header-highlight.v-expansion-panel-title
    > .v-expansion-panel-title__overlay {
    opacity: 0;
}
</style>
