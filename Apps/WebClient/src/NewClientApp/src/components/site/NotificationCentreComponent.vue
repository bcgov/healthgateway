<script setup lang="ts">
import { computed, ref } from "vue";
import { useRouter } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { EntryType } from "@/constants/entryType";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import Notification, { NotificationActionType } from "@/models/notification";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import { useNotificationStore } from "@/stores/notification";
import { useTimelineStore } from "@/stores/timeline";

const bctOdrCategory = "BctOdr";
const clinicalDocumentCategory = "ClinicalDocument";
const covid19LaboratoryCategory = "COVID19Laboratory";
const healthVisitCategory = "HealthVisit";
const immunizationCategory = "Immunization";
const laboratoryCategory = "Laboratory";
const medicationCategory = "Medications";
const noteCategory = "MyNote";
const specialAuthorityCategory = "SpecialAuthority";

const router = useRouter();
const notificationStore = useNotificationStore();
const timelineStore = useTimelineStore();

const messageLoaders = ref(new Set<string>());

const notifications = computed(() => notificationStore.notifications);
const newNotifications = computed(() => notificationStore.newNotifications);

function dismissAllNotifications(): void {
    messageLoaders.value.add("all");
    notificationStore
        .dismissAllNotifications()
        .finally(() => messageLoaders.value.clear());
}

function dismissNotification(notificationId: string): void {
    messageLoaders.value.add(notificationId);
    notificationStore
        .dismissNotification(notificationId)
        .finally(() => messageLoaders.value.delete(notificationId));
}

function isMessageLoading(notificationId?: string): boolean {
    return (
        messageLoaders.value.has("all") ||
        (notificationId !== undefined &&
            messageLoaders.value.has(notificationId))
    );
}

function handleClickNotificationAction(notification: Notification): void {
    const entryType = getEntryType(notification.categoryName);
    if (entryType) {
        const builder = TimelineFilterBuilder.create().withEntryType(entryType);
        timelineStore.setFilter(builder);
        router.push({ path: "/timeline" });
    } else if (notification.categoryName === bctOdrCategory) {
        router.push({ path: "/services" });
    } else {
        router.push({ path: notification.actionUrl });
    }
}

function getEntryType(categoryName: string): EntryType | undefined {
    switch (categoryName) {
        case clinicalDocumentCategory:
            return EntryType.ClinicalDocument;
        case covid19LaboratoryCategory:
            return EntryType.Covid19TestResult;
        case healthVisitCategory:
            return EntryType.HealthVisit;
        case immunizationCategory:
            return EntryType.Immunization;
        case laboratoryCategory:
            return EntryType.LabResult;
        case medicationCategory:
            return EntryType.Medication;
        case noteCategory:
            return EntryType.Note;
        case specialAuthorityCategory:
            return EntryType.SpecialAuthorityRequest;
        default:
            return undefined;
    }
}

function formatDate(date: StringISODateTime): string {
    return new DateWrapper(date, {
        hasTime: true,
        isUtc: true,
    }).format("MMMM d, yyyy");
}

function formatActionText(
    actionType: NotificationActionType
): string | undefined {
    switch (actionType) {
        case NotificationActionType.InternalLink:
            return "View";
        case NotificationActionType.ExternalLink:
            return "More Info";
        default:
            return undefined;
    }
}

function showActionButton(notification: Notification): boolean {
    return (
        notification.actionType === NotificationActionType.InternalLink ||
        notification.actionType === NotificationActionType.ExternalLink
    );
}

function isNew(notification: Notification): boolean {
    return newNotifications.value.some((n) => n.id === notification.id);
}
</script>

<template>
    <v-navigation-drawer
        v-model="notificationStore.isNotificationCenterOpen"
        temporary
        location="right"
        width="500"
    >
        <v-container class="d-flex flex-column align-center">
            <v-row align="center" class="align-self-start flex-grow-0 w-100">
                <v-col cols="auto" class="text-primary">
                    <HgIconButtonComponent
                        data-testid="notification-centre-close-button"
                        icon="angle-double-right"
                        size="x-small"
                        @click="
                            notificationStore.isNotificationCenterOpen = false
                        "
                    />
                </v-col>
                <v-col>
                    <h5 class="text-body-1 text-primary">
                        Notification Centre
                    </h5>
                </v-col>
                <v-col v-if="notifications.length > 0" cols="auto">
                    <HgButtonComponent
                        data-testid="notification-centre-dismiss-all-button"
                        variant="link"
                        color="grey"
                        text="Clear All"
                        :loading="isMessageLoading()"
                        @click="dismissAllNotifications"
                    />
                </v-col>
            </v-row>
            <v-row
                v-if="notifications.length === 0"
                align="center"
                justify="center"
            >
                <v-col>
                    <v-img
                        src="@/assets/images/home/empty-state-notifications.svg"
                        alt="No Notifications"
                    />
                    <p class="text-center mt-4 text-body-1">No Notifications</p>
                </v-col>
            </v-row>
            <v-row
                v-else
                class="flex-column w-100"
                data-testid="notifications-div"
            >
                <v-col
                    v-for="notification in notifications"
                    :key="notification.id"
                    class="flex-grow-0 py-2 px-0"
                >
                    <p
                        class="text-body-2"
                        :class="{ 'text-grey-lighten-1': !isNew(notification) }"
                    >
                        {{ formatDate(notification.scheduledDateTimeUtc) }}
                    </p>
                    <v-card class="bg-grey-lighten-5">
                        <v-card-title class="text-right pa-0">
                            <HgIconButtonComponent
                                :data-testid="`notification-${notification.id}-dismiss-button`"
                                class="text-muted"
                                icon="xmark"
                                size="small"
                                :loading="isMessageLoading(notification.id)"
                                @click="dismissNotification(notification.id)"
                            />
                        </v-card-title>
                        <v-card-text
                            class="text-body-1"
                            :class="{
                                'text-grey-lighten-1': !isNew(notification),
                            }"
                        >
                            {{ notification.displayText }}
                        </v-card-text>
                        <v-card-actions
                            v-if="showActionButton(notification)"
                            class="pa-0"
                        >
                            <v-spacer />
                            <HgButtonComponent
                                :data-testid="`notification-${notification.id}-action-button`"
                                variant="link"
                                :text="
                                    formatActionText(notification.actionType)
                                "
                                @click="
                                    handleClickNotificationAction(notification)
                                "
                            />
                        </v-card-actions>
                    </v-card>
                </v-col>
            </v-row>
        </v-container>
    </v-navigation-drawer>
</template>
