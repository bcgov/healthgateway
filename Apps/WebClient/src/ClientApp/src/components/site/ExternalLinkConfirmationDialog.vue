<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    Action,
    createLinkEventData,
    EventData,
    ExternalUrl,
    Origin,
} from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";

type DialogBodyLine =
    | string
    | {
          prefix?: string;
          text: string; // UI-visible text
          trackingText: string; // Snowplow EventData.text
          href: ExternalUrl;
          suffix?: string;
          ariaLabel?: string;
      };

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const props = withDefaults(
    defineProps<{
        modelValue: boolean;
        origin: Origin;
        title?: string;
        body?: DialogBodyLine[];
        confirmLabel?: string;
        cancelLabel?: string;
    }>(),
    {
        title: "",
        body: () => [],
        confirmLabel: "Proceed",
        cancelLabel: "Cancel",
    }
);

const emit = defineEmits<{
    (e: "update:modelValue", value: boolean): void;
    (e: "confirm"): void;
    (e: "cancel"): void;
}>();

function close(): void {
    emit("update:modelValue", false);
}

function cancel(): void {
    emit("cancel");
    close();
    // Caller-specific logic must be handled in the @cancel handler.
}

function confirm(): void {
    emit("confirm");
    close();
    // Caller-specific logic must be handled in the @confirm handler.
}

function trackExternalLinkDialogAction(text: string, url?: ExternalUrl): void {
    if (!url) {
        return;
    }
    const data: EventData = createLinkEventData(
        text,
        props.origin,
        Action.ExternalLink,
        url
    );

    trackingService.trackEvent(data);
}
</script>

<template>
    <v-dialog
        :model-value="modelValue"
        data-testid="external-link-confirmation-dialog"
        persistent
        no-click-animation
        @update:model-value="emit('update:modelValue', $event)"
    >
        <div class="d-flex justify-center">
            <v-card max-width="700">
                <v-card-title class="bg-primary px-0">
                    <v-toolbar
                        :title="props.title"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            icon="close"
                            aria-label="Close"
                            @click="cancel"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="pa-4">
                    <div v-if="props.body?.length">
                        <div
                            v-for="(line, i) in props.body"
                            :key="i"
                            class="text-body-1"
                            :class="{ 'mb-1': i < props.body.length - 1 }"
                            :data-testid="`external-link-confirmation-dialog-body-${i}`"
                        >
                            <template v-if="typeof line === 'string'">
                                {{ line }}
                            </template>
                            <template v-else>
                                <span v-if="line.prefix">{{
                                    line.prefix
                                }}</span>
                                <a
                                    :href="line.href"
                                    target="_blank"
                                    rel="noopener"
                                    class="hg-text-link"
                                    :aria-label="line.ariaLabel ?? line.text"
                                    :data-testid="`external-link-confirmation-dialog-link-${i}`"
                                    @click="
                                        trackExternalLinkDialogAction(
                                            line.trackingText,
                                            line.href
                                        )
                                    "
                                >
                                    {{ line.text }}
                                </a>
                                <span v-if="line.suffix">{{
                                    line.suffix
                                }}</span>
                            </template>
                        </div>
                    </div>
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm px-4 py-3">
                    <HgButtonComponent
                        variant="secondary"
                        :uppercase="false"
                        :text="props.cancelLabel"
                        data-testid="external-link-confirmation-dialog-cancel-button"
                        @click="cancel()"
                    />
                    <HgButtonComponent
                        variant="primary"
                        :uppercase="false"
                        :text="props.confirmLabel"
                        data-testid="external-link-confirmation-dialog-proceed-button"
                        @click="confirm()"
                    />
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
