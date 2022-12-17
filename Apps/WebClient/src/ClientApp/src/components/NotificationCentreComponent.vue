<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faAngleDoubleRight, faXmark } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import Notification, { NotificationActionType } from "@/models/notification";
import User from "@/models/user";

library.add(faAngleDoubleRight, faXmark);

@Component
export default class NotificationCentreComponent extends Vue {
    @Action("dismissAllNotifications", { namespace: "notification" })
    dismissAllNotifications!: () => void;

    @Action("dismissNotification", { namespace: "notification" })
    dismissNotification!: (params: { notificationId: string }) => void;

    @Getter("notifications", { namespace: "notification" })
    notifications!: Notification[];

    @Getter("user", { namespace: "user" })
    user!: User;

    readonly sidebarId = "notification-centre-sidebar";
    readonly internalLink = NotificationActionType.InternalLink;

    public get newNotifications(): Notification[] {
        if (this.user.lastLoginDateTime) {
            const lastLoginDateTime = new DateWrapper(
                this.user.lastLoginDateTime
            );
            return this.notifications.filter((n) =>
                new DateWrapper(n.scheduledDateTimeUtc).isAfter(
                    lastLoginDateTime
                )
            );
        }
        return this.notifications;
    }

    formatDate(date: StringISODateTime): string {
        return new DateWrapper(date, {
            hasTime: true,
            isUtc: true,
        }).format("MMMM d, yyyy");
    }

    formatActionText(actionType: NotificationActionType): string | undefined {
        switch (actionType) {
            case NotificationActionType.InternalLink:
                return "View";
            case NotificationActionType.ExternalLink:
                return "More Info";
            default:
                return undefined;
        }
    }

    showActionButton(notification: Notification): boolean {
        return (
            notification.actionType === NotificationActionType.InternalLink ||
            notification.actionType === NotificationActionType.ExternalLink
        );
    }

    isNew(notification: Notification): boolean {
        return this.newNotifications.some((n) => n.id === notification.id);
    }
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
                        @click="
                            dismissNotification({
                                notificationId: notification.id,
                            })
                        "
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
                        <b-link
                            :data-testid="`notification-${notification.id}-action-button`"
                            :href="notification.actionUrl"
                            class="card-link"
                            >{{
                                formatActionText(notification.actionType)
                            }}</b-link
                        >
                    </div>
                </div>
            </b-card>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
