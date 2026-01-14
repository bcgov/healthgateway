<script setup lang="ts">
import { computed, ref, watch } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import CommentSectionComponent from "@/components/private/timeline/comment/CommentSectionComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { Action, Type } from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { EventName, useEventStore } from "@/stores/event";
import { useLayoutStore } from "@/stores/layout";

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

const emit = defineEmits<{
    (e: "click-attachment-button"): void;
}>();

const layoutStore = useLayoutStore();
const eventStore = useEventStore();
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const detailsVisible = ref(props.isMobileDetails);

const isInteractive = computed(
    () =>
        (!layoutStore.isMobile && props.canShowDetails) ||
        (layoutStore.isMobile && !props.isMobileDetails)
);
const entryTypeTitle = computed(() => {
    const title = entryTypeMap.get(props.entry.type)?.title;
    return title && title.trim() ? title : props.entry.type.toString();
});
const displayTitle = computed(() => props.title?.trim() ?? "");
const dateString = computed(() => props.entry.date.format());
const commentCount = computed(() => props.entry.comments?.length ?? 0);

function handleCardClick(): void {
    if (props.entry.type === EntryType.BcCancerScreening) {
        trackingService.trackEvent({
            action: Action.TimelineCardClick,
            text: props.title,
            type: Type.BcCancerScreening,
        });
    }
    if (layoutStore.isMobile && !props.isMobileDetails) {
        eventStore.emit(EventName.OpenFullscreenTimelineEntry, props.entry);
    } else if (!layoutStore.isMobile && props.canShowDetails) {
        detailsVisible.value = !detailsVisible.value;
    }
}

watch(
    () => layoutStore.isMobile,
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
                                :data-testid="`${String(
                                    entry.type
                                ).toLowerCase()}Title`"
                            >
                                <span
                                    data-testid="entryCardDate"
                                    class="text-subtitle-1 font-weight-bold d-sm-inline d-block text-no-wrap mr-sm-2"
                                >
                                    {{ dateString }}
                                </span>
                                <span
                                    data-testid="entry-card-type-title"
                                    class="text-subtitle-1 font-weight-bold d-sm-inline d-block text-no-wrap"
                                >
                                    {{ entryTypeTitle }}
                                </span>
                                <span
                                    v-if="displayTitle"
                                    class="d-sm-inline d-block"
                                >
                                    <span
                                        data-testid="entry-card-details-pipe"
                                        class="text-subtitle-1 font-weight-bold d-none d-sm-inline"
                                        aria-hidden="true"
                                    >
                                        |
                                    </span>
                                    <span
                                        data-testid="entryCardDetailsTitle"
                                        class="text-subtitle-1"
                                    >
                                        {{ displayTitle }}
                                    </span>
                                </span>
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
                            <v-col
                                cols="auto"
                                class="text-primary d-flex align-center all-pointer-events"
                            >
                                <span
                                    v-if="commentCount > 1"
                                    data-testid="commentCount"
                                >
                                    {{ commentCount }}
                                </span>
                                <v-icon
                                    v-if="commentCount > 0"
                                    class="pa-1"
                                    icon="far fa-comment"
                                    size="x-small"
                                    data-testid="commentIcon"
                                />
                                <HgIconButtonComponent
                                    v-if="hasAttachment"
                                    icon="paperclip"
                                    size="x-small"
                                    data-testid="attachment-button"
                                    @click.stop="
                                        emit('click-attachment-button')
                                    "
                                />
                                <slot name="header-menu" />
                            </v-col>
                        </v-row>
                    </v-col>
                </v-row>
            </v-expansion-panel-title>
            <v-expansion-panel-text>
                <div
                    class="my-2"
                    :class="{ 'indent-content': !isMobileDetails }"
                >
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

.indent-content {
    // icon size + icon padding + col padding
    margin-left: calc(4rem + 24px);
}

.no-pointer-events {
    pointer-events: none;
}

.all-pointer-events {
    pointer-events: all;
}

:deep(.v-expansion-panel-title:hover) {
    background-color: rgb(var(--v-theme-surfaceHover)) !important;
}
:deep(.v-expansion-panel-title:hover > .v-expansion-panel-title__overlay) {
    opacity: 0 !important;
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
