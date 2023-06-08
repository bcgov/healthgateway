<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faAngleDoubleRight, faXmark } from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useRouter, useStore } from "vue-composition-wrapper";

import { EntryType } from "@/constants/entryType";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import Notification, { NotificationActionType } from "@/models/notification";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import User from "@/models/user";

const bctOdrCategory = "BctOdr";
const clinicalDocumentCategory = "ClinicalDocument";
const covid19LaboratoryCategory = "COVID19Laboratory";
const healthVisitCategory = "HealthVisit";
const immunizationCategory = "Immunization";
const laboratoryCategory = "Laboratory";
const medicationCategory = "Medications";
const noteCategory = "MyNote";
const specialAuthorityCategory = "SpecialAuthority";

library.add(faAngleDoubleRight, faXmark);

const router = useRouter();
const store = useStore();

const sidebarId = ref("notification-centre-sidebar");

const notifications = computed<Notification[]>(
    () => store.getters["notification/notifications"]
);
const user = computed<User>(() => store.getters["user/user"]);
const newNotifications = computed<Notification[]>(() => {
    if (!user.value.lastLoginDateTime) {
        return notifications.value;
    }

    const lastLoginDateTime = new DateWrapper(user.value.lastLoginDateTime);
    return notifications.value.filter((n) =>
        new DateWrapper(n.scheduledDateTimeUtc).isAfter(lastLoginDateTime)
    );
});

function dismissAllNotifications(): void {
    store.dispatch("notification/dismissAllNotifications");
}

function dismissNotification(notificationId: string): void {
    store.dispatch("notification/dismissNotification", { notificationId });
}

function setFilter(filterBuilder: TimelineFilterBuilder): void {
    store.dispatch("timeline/setFilter", filterBuilder);
}

function handleClickNotificationAction(notification: Notification): void {
    const entryType = getEntryType(notification.categoryName);
    if (entryType) {
        const builder = TimelineFilterBuilder.create().withEntryType(entryType);
        setFilter(builder);
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
    <div class="flex-grow-1 d-flex flex-column p-3">
        <b-row no-gutters class="align-items-center">
            <b-col cols="auto">
                <hg-button
                    v-b-toggle="sidebarId"
                    data-testid="notification-centre-close-button"
                    variant="icon"
                    class="text-dark px-2 py-1"
                >
                    <hg-icon icon="angle-double-right" size="medium" />
                </hg-button>
            </b-col>
            <b-col class="px-2">
                <h5 class="mb-0">Notification Centre</h5>
            </b-col>
            <b-col v-if="notifications.length > 0" cols="auto">
                <hg-button
                    data-testid="notification-centre-dismiss-all-button"
                    variant="link"
                    class="text-muted px-2 py-1"
                    @click="dismissAllNotifications"
                >
                    <small>Clear All</small>
                </hg-button>
            </b-col>
        </b-row>
        <div
            v-if="notifications.length === 0"
            class="flex-grow-1 d-flex align-items-center"
        >
            <div class="text-center flex-grow-1">
                <b-row>
                    <b-col>
                        <img
                            class="img-fluid my-3"
                            src="@/assets/images/home/empty-state-notifications.svg"
                            width="167"
                            alt="No Notifications"
                        />
                    </b-col>
                </b-row>
                <b-row>
                    <b-col> No Notifications </b-col>
                </b-row>
            </div>
        </div>
        <div
            v-for="notification in notifications"
            :key="notification.id"
            class="mt-3"
            data-testid="notifications-div"
        >
            <small :class="{ 'text-muted': !isNew(notification) }">
                {{ formatDate(notification.scheduledDateTimeUtc) }}
            </small>
            <b-card no-body class="mt-2" border-variant="white">
                <div class="text-right">
                    <hg-button
                        :data-testid="`notification-${notification.id}-dismiss-button`"
                        variant="icon"
                        class="text-muted py-0 px-2 m-2"
                        @click="dismissNotification(notification.id)"
                    >
                        <hg-icon icon="xmark" size="small" />
                    </hg-button>
                </div>
                <div class="px-3 pb-3">
                    <div :class="{ 'text-muted': !isNew(notification) }">
                        {{ notification.displayText }}
                    </div>
                    <div
                        v-if="showActionButton(notification)"
                        class="text-right mt-2"
                    >
                        <hg-button
                            :data-testid="`notification-${notification.id}-action-button`"
                            variant="link"
                            class="card-link"
                            @click="handleClickNotificationAction(notification)"
                        >
                            {{ formatActionText(notification.actionType) }}
                        </hg-button>
                    </div>
                </div>
            </b-card>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
