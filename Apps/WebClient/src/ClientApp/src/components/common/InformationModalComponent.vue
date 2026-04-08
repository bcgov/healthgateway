<script setup lang="ts">
import { ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { InformationModalContent } from "@/models/informationModal";
import {
    Action,
    Destination,
    ExternalUrl,
    InternalUrl,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";

interface Props {
    content: InformationModalContent;
    isLoading?: boolean;
    okOnly?: boolean;
    submitText?: string;
    cancelText?: string;
    okText?: string;
}

withDefaults(defineProps<Props>(), {
    isLoading: false,
    okOnly: false,
    submitText: "Continue",
    cancelText: "Cancel",
    okText: "OK",
});

defineExpose({ showModal, hideModal });

const emit = defineEmits<{
    (e: "submit"): void;
    (e: "cancel"): void;
}>();

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const isVisible = ref(false);

function showModal(): void {
    isVisible.value = true;
}

function hideModal(): void {
    isVisible.value = false;
}

function handleSubmit(): void {
    hideModal();
    emit("submit");
}

function handleCancel(): void {
    hideModal();
    emit("cancel");
}

function handleClick(
    text?: Text,
    origin?: Origin,
    destination?: Destination,
    type?: Type,
    url?: ExternalUrl | InternalUrl
): void {
    if (!text) return;

    trackingService.trackEvent({
        action: Action.ExternalLink,
        text,
        origin,
        destination,
        type,
        url,
    });
}
</script>

<template>
    <v-dialog
        id="informationModal"
        v-model="isVisible"
        persistent
        no-click-animation
    >
        <div class="d-flex justify-center">
            <v-card
                :loading="isLoading"
                max-width="700px"
                data-testid="information-modal"
            >
                <template #loader="{ isActive }">
                    <v-progress-linear
                        :active="isActive"
                        color="accent"
                        indeterminate
                    />
                </template>
                <v-card-title class="bg-primary px-0">
                    <v-toolbar
                        :title="content.title"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            data-testid="message-modal-close-button"
                            icon="close"
                            aria-label="Close"
                            @click="handleCancel"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="text-body-1 pa-4">
                    <p
                        v-for="(
                            paragraph, paragraphIndex
                        ) in content.paragraphs"
                        :key="paragraphIndex"
                        class="mb-4"
                        :class="{
                            'mb-0':
                                paragraphIndex ===
                                content.paragraphs.length - 1,
                        }"
                        :data-testid="`information-modal-paragraph-${paragraphIndex}`"
                    >
                        <template
                            v-for="(
                                segment, segmentIndex
                            ) in paragraph.segments"
                            :key="segmentIndex"
                        >
                            <span v-if="segment.type === 'text'">
                                {{ segment.value }}
                            </span>
                            <strong v-else-if="segment.type === 'bold'">
                                {{ segment.value }}
                            </strong>
                            <a
                                v-else
                                :href="segment.href"
                                target="_blank"
                                rel="noopener noreferrer"
                                :data-testid="`information-modal-link-${paragraphIndex}-${segmentIndex}`"
                                @click="
                                    handleClick(
                                        segment.analyticsText,
                                        segment.analyticsOrigin,
                                        segment.analyticsDestination,
                                        segment.analyticsType,
                                        segment.analysticsUrl
                                    )
                                "
                            >
                                {{ segment.text }}
                            </a>
                        </template>
                    </p>
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm pa-4">
                    <HgButtonComponent
                        data-testid="generic-message-ok-btn"
                        :variant="okOnly ? 'primary' : 'secondary'"
                        :text="okOnly ? okText : cancelText"
                        @click.prevent="handleCancel"
                    />
                    <HgButtonComponent
                        v-if="!okOnly"
                        data-testid="generic-message-submit-btn"
                        class="ml-2"
                        variant="primary"
                        :text="submitText"
                        @click.prevent="handleSubmit"
                    />
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
