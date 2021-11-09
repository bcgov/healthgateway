<script lang="ts">
import { Component, Emit, Vue, Watch } from "vue-property-decorator";
import { DataTableHeader } from "vuetify";

import BannerModal from "@/components/core/modals/BannerModal.vue";
import EmailModal from "@/components/core/modals/EmailModal.vue";
import { ResultType } from "@/constants/resulttype";
import Communication, {
    CommunicationStatus,
    CommunicationType,
} from "@/models/adminCommunication";
import BannerFeedback from "@/models/bannerFeedback";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ICommunicationService } from "@/services/interfaces";

@Component({
    components: {
        BannerModal,
        EmailModal,
    },
})
export default class CommunicationTable extends Vue {
    private communicationList: Communication[] = [];
    private bannerList: Communication[] = [];
    private inAppList: Communication[] = [];
    private emailList: Communication[] = [];
    private communicationService!: ICommunicationService;
    private isLoading = false;
    private showFeedback = false;
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: "",
    };
    // 0: Banners, 1: In-App, 2: Emails
    private tab = 0;
    private isNewCommunication = true;
    private headers: DataTableHeader[] = [];
    private editedBanner: Communication = {
        id: "-1",
        text: "",
        subject: "",
        communicationTypeCode: CommunicationType.Banner,
        communicationStatusCode: CommunicationStatus.Draft,
        priority: 10,
        version: 0,
        scheduledDateTime: new DateWrapper().toISO(true),
        effectiveDateTime: new DateWrapper().toISO(true),
        expiryDateTime: new DateWrapper().add({ days: 1 }).toISO(true),
    };

    private editedInApp: Communication = {
        id: "-1",
        text: "",
        subject: "",
        communicationTypeCode: CommunicationType.InApp,
        communicationStatusCode: CommunicationStatus.Draft,
        priority: 10,
        version: 0,
        scheduledDateTime: new DateWrapper().toISO(true),
        effectiveDateTime: new DateWrapper().toISO(true),
        expiryDateTime: new DateWrapper().add({ days: 1 }).toISO(true),
    };

    private editedEmail: Communication = {
        id: "-1",
        subject: "",
        communicationTypeCode: CommunicationType.Email,
        communicationStatusCode: CommunicationStatus.Draft,
        text: "<p></p>",
        priority: 10,
        scheduledDateTime: new DateWrapper().toISO(true),
        effectiveDateTime: new DateWrapper().toISO(true),
        expiryDateTime: new DateWrapper().toISO(true),
        version: 0,
    };

    private defaultBanner: Communication = {
        id: "-1",
        text: "",
        subject: "",
        communicationTypeCode: CommunicationType.Banner,
        communicationStatusCode: CommunicationStatus.Draft,
        version: 0,
        priority: 10,
        scheduledDateTime: new DateWrapper().toISO(true),
        effectiveDateTime: new DateWrapper().toISO(true),
        expiryDateTime: new DateWrapper().add({ days: 1 }).toISO(true),
    };

    private defaultInApp: Communication = {
        id: "-1",
        text: "",
        subject: "",
        communicationTypeCode: CommunicationType.InApp,
        communicationStatusCode: CommunicationStatus.Draft,
        version: 0,
        priority: 10,
        scheduledDateTime: new DateWrapper().toISO(true),
        effectiveDateTime: new DateWrapper().toISO(true),
        expiryDateTime: new DateWrapper().add({ days: 1 }).toISO(true),
    };

    private defaultEmail: Communication = {
        id: "-1",
        subject: "",
        communicationTypeCode: CommunicationType.Email,
        communicationStatusCode: CommunicationStatus.Draft,
        text: "<p></p>",
        priority: 10,
        scheduledDateTime: new DateWrapper().toISO(true),
        effectiveDateTime: new DateWrapper().toISO(true),
        expiryDateTime: new DateWrapper().toISO(true),
        version: 0,
    };

    private mounted() {
        this.communicationService = container.get(
            SERVICE_IDENTIFIER.CommunicationService
        );
        this.headers = this.bannerHeaders;
        this.loadCommunicationList();
    }

    @Watch("tab")
    private onTabChange(tabIndex: number) {
        if (tabIndex === 0) {
            // Banners
            this.communicationList = this.bannerList;
            this.headers = this.bannerHeaders;
        } else if (tabIndex === 1) {
            // In-App
            this.communicationList = this.inAppList;
            this.headers = this.bannerHeaders;
        } else {
            // Emails
            this.communicationList = this.emailList;
            this.headers = this.emailHeaders;
        }
    }

    private bannerHeaders: DataTableHeader[] = [
        {
            text: "Subject",
            value: "subject",
            align: "start",
            width: "20%",
            sortable: false,
        },
        {
            text: "Status",
            value: "communicationStatusCode",
            width: "130px",
            sortable: false,
        },
        {
            text: "Effective On",
            value: "effectiveDateTime",
        },
        {
            text: "Expires On",
            value: "expiryDateTime",
        },
        {
            text: "Text",
            value: "text",
            sortable: false,
        },
        {
            text: "Actions",
            value: "actions",
            sortable: false,
        },
    ];

    private emailHeaders: DataTableHeader[] = [
        {
            text: "Subject",
            value: "subject",
            align: "start",
            width: "20%",
            sortable: false,
        },
        {
            text: "Scheduled For",
            value: "scheduledDateTime",
        },
        {
            text: "Priority",
            value: "priority",
        },
        {
            text: "Status",
            value: "communicationStatusCode",
        },
        {
            text: "Actions",
            value: "actions",
            sortable: false,
        },
    ];

    private formatDateTime(date: StringISODateTime): string {
        if (!date) {
            return "";
        }
        return new DateWrapper(date, { isUtc: true }).format(
            DateWrapper.defaultDateTimeFormat
        );
    }

    private formatPriority(priority: number) {
        if (priority === 1) {
            return "Low";
        } else if (priority === 10) {
            return "Standard";
        } else if (priority === 100) {
            return "High";
        } else {
            return "Urgent";
        }
    }

    private edit(item: Communication) {
        this.isNewCommunication = false;
        if (item.communicationTypeCode === CommunicationType.Email) {
            this.editedEmail = item;
        } else if (item.communicationTypeCode === CommunicationType.InApp) {
            this.editedInApp = item;
        } else {
            this.editedBanner = item;
        }
    }

    private sortCommunicationsByDate(
        isDescending: boolean,
        columnName: string
    ) {
        this.communicationList.sort((a, b) => {
            let first!: DateWrapper;
            let second!: DateWrapper;
            if (columnName === "effectiveDateTime") {
                first = new DateWrapper(a.effectiveDateTime, { isUtc: true });
                second = new DateWrapper(b.effectiveDateTime, { isUtc: true });
            } else if (columnName === "expiryDateTime") {
                first = new DateWrapper(a.expiryDateTime, { isUtc: true });
                second = new DateWrapper(b.expiryDateTime, { isUtc: true });
            } else {
                return 0;
            }

            if (first.isAfter(second)) {
                return isDescending ? -1 : 1;
            } else if (first.isBefore(second)) {
                return isDescending ? 1 : -1;
            }
            return 0;
        });
    }

    private customSort(
        items: Communication[],
        index: string[],
        isDescending: boolean[]
    ) {
        // items: 'Communication' items
        // index: Enabled sort headers value. (black arrow status).
        // isDescending: Whether enabled sort headers is desc
        if (index === undefined || index.length === 0) {
            index = ["effectiveDateTime"];
            isDescending = [true];
        }
        this.sortCommunicationsByDate(isDescending[0], index[0]);

        return this.communicationList;
    }

    private checkDisabled(item: Communication) {
        const now = new Date();
        if (
            item.communicationTypeCode === CommunicationType.Banner ||
            item.communicationTypeCode === CommunicationType.InApp
        ) {
            const expiryDateTime = new Date(item.expiryDateTime);
            if (
                item.communicationStatusCode != CommunicationStatus.Draft &&
                expiryDateTime < now
            ) {
                return true;
            }
        } else if (item.communicationTypeCode === CommunicationType.Email) {
            const scheduledDateTime = new Date(item.scheduledDateTime);
            if (
                item.communicationStatusCode != CommunicationStatus.Draft &&
                scheduledDateTime < now
            ) {
                return true;
            }
        }
        return false;
    }

    private loadCommunicationList() {
        this.communicationService
            .getAll()
            .then((banners: Communication[]) => {
                this.parseComms(banners);
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error loading banners",
                };
                console.log(err);
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private parseComms(communication: Communication[]) {
        this.bannerList = communication.filter(
            (comm: Communication) =>
                comm.communicationTypeCode === CommunicationType.Banner
        );
        this.inAppList = communication.filter(
            (comm: Communication) =>
                comm.communicationTypeCode === CommunicationType.InApp
        );
        this.emailList = communication.filter(
            (comm: Communication) =>
                comm.communicationTypeCode === CommunicationType.Email
        );
        if (this.tab === 0) {
            this.communicationList = this.bannerList;
        } else if (this.tab === 1) {
            this.communicationList = this.inAppList;
        } else {
            this.communicationList = this.emailList;
        }
    }

    private add(comm: Communication): void {
        this.isLoading = true;
        this.isFinishedLoading();
        this.communicationService
            .add({
                subject: comm.subject,
                text: comm.text,
                communicationTypeCode: comm.communicationTypeCode,
                communicationStatusCode: comm.communicationStatusCode,
                priority: comm.priority,
                version: 0,
                scheduledDateTime: comm.scheduledDateTime,
                effectiveDateTime: comm.effectiveDateTime,
                expiryDateTime: comm.expiryDateTime,
            })
            .then(() => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Success,
                    title: "Success",
                    message: "Communication Added.",
                };
                this.loadCommunicationList();
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: err.resultMessage,
                };
                this.loadCommunicationList();
                console.log(err);
            })
            .finally(() => {
                this.isLoading = false;
                this.emitResult();
            });
    }

    private update(comm: Communication): void {
        this.isLoading = true;
        this.isFinishedLoading();
        this.communicationService
            .update({
                id: comm.id,
                subject: comm.subject,
                text: comm.text,
                communicationTypeCode: comm.communicationTypeCode,
                communicationStatusCode: comm.communicationStatusCode,
                priority: comm.priority,
                version: comm.version,
                scheduledDateTime: comm.scheduledDateTime,
                effectiveDateTime: comm.effectiveDateTime,
                expiryDateTime: comm.expiryDateTime,
            })
            .then(() => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Success,
                    title: "Success",
                    message: "Communication Updated.",
                };
                this.loadCommunicationList();
            })
            .catch((error) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: error.resultMessage,
                };
                this.loadCommunicationList();
                console.log(error);
            })
            .finally(() => {
                this.isLoading = false;
                this.emitResult();
            });
    }

    private deleteComm(comm: Communication): void {
        this.isLoading = true;
        this.isFinishedLoading();
        this.communicationService
            .delete(comm)
            .then(() => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Success,
                    title: "Success",
                    message: "Communication Deleted.",
                };
                this.loadCommunicationList();
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error deleting communication. Please try again.",
                };
                console.log(err);
            })
            .finally(() => {
                this.isLoading = false;
                this.emitResult();
            });
    }

    private close() {
        this.editedBanner = Object.assign({}, this.defaultBanner);
        this.editedInApp = Object.assign({}, this.defaultInApp);
        this.editedEmail = Object.assign({}, this.defaultEmail);
        this.isNewCommunication = true;
    }

    private emitResult() {
        this.isFinishedLoading();
        this.bannerFeedbackInfo();
        this.shouldShowFeedback();
    }

    @Emit()
    private shouldShowFeedback() {
        return this.showFeedback;
    }

    @Emit()
    private bannerFeedbackInfo() {
        return this.bannerFeedback;
    }

    @Emit()
    private isFinishedLoading() {
        return this.isLoading;
    }
}
</script>

<template>
    <v-data-table
        :headers="headers"
        :items="communicationList"
        :custom-sort="customSort"
        class="elevation-1"
    >
        <template #item.effectiveDateTime="{ item }">
            <span>{{ formatDateTime(item.effectiveDateTime) }}</span>
        </template>
        <template #item.expiryDateTime="{ item }">
            <span>{{ formatDateTime(item.expiryDateTime) }}</span>
        </template>
        <template #item.scheduledDateTime="{ item }">
            <span>{{ formatDateTime(item.scheduledDateTime) }}</span>
        </template>
        <template #top>
            <v-toolbar dark>
                <v-tabs v-model="tab" dark>
                    <v-tab> Banner Posts </v-tab>
                    <v-tab> In-App </v-tab>
                    <v-tab> Emails </v-tab>
                </v-tabs>
                <v-spacer></v-spacer>
                <BannerModal
                    v-if="tab == 0"
                    :edited-item="editedBanner"
                    :is-new="isNewCommunication"
                    :is-in-app="false"
                    @emit-add="add"
                    @emit-update="update"
                    @emit-close="close"
                />
                <BannerModal
                    v-if="tab == 1"
                    :edited-item="editedInApp"
                    :is-new="isNewCommunication"
                    :is-in-app="true"
                    @emit-add="add"
                    @emit-update="update"
                    @emit-close="close"
                />
                <EmailModal
                    v-if="tab == 2"
                    :edited-item="editedEmail"
                    :is-new="isNewCommunication"
                    @emit-send="add"
                    @emit-update="update"
                    @emit-close="close"
                />
            </v-toolbar>
        </template>
        <template #item.actions="{ item }">
            <v-btn
                class="mr-2"
                :disabled="checkDisabled(item)"
                @click="edit(item)"
            >
                <font-awesome-icon icon="edit" size="1x"> </font-awesome-icon>
            </v-btn>

            <v-btn :disabled="checkDisabled(item)" @click="deleteComm(item)">
                <font-awesome-icon icon="trash" size="1x"> </font-awesome-icon>
            </v-btn>
        </template>
        <template #item.priority="{ item }">
            <span>{{ formatPriority(item.priority) }}</span>
        </template>
        <template #no-data>
            <span>Nothing to show here.</span>
        </template>
    </v-data-table>
</template>

<style scoped lang="scss">
.error-message {
    color: #ff5252 !important;
}
</style>
