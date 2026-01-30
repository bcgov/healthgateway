<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    Action,
    Destination,
    EventData,
    Origin,
    Type,
} from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";

type DialogBodyLine =
    | string
    | {
          prefix?: string;
          text: string;
          href: string;
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

function cancel(text: string): void {
    emit("cancel");
    close();
    trackExternalLinkDialogAction(text);
}

function confirm(text: string): void {
    emit("confirm");
    close();
    trackExternalLinkDialogAction(text);
}

function trackExternalLinkDialogAction(text: string, url?: string): void {
    const data: EventData = {
        action: Action.ButtonClick,
        text,
        destination: Destination.AccessMyHealth,
        origin: props.origin,
        type: Type.RecordSourceTile,
    };

    if (url) {
        data.url = url;
    }

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
                                            line.text,
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
                        @click="cancel(props.cancelLabel)"
                    />
                    <HgButtonComponent
                        variant="primary"
                        :uppercase="false"
                        :text="props.confirmLabel"
                        data-testid="external-link-confirmation-dialog-proceed-button"
                        @click="confirm(props.confirmLabel)"
                    />
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
