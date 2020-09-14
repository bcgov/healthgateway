<style scoped lang="scss">
.error-message {
    color: #ff5252 !important;
}
</style>
<template>
    <v-data-table
        :headers="headers"
        :items="communicationList"
        :custom-sort="customSort"
        class="elevation-1"
    >
        <template v-slot:item.effectiveDateTime="{ item }">
            <span>{{ formatDate(item.effectiveDateTime) }}</span>
        </template>
        <template v-slot:item.expiryDateTime="{ item }">
            <span>{{ formatDate(item.expiryDateTime) }}</span>
        </template>
        <template v-slot:item.scheduledDateTime="{ item }">
            <span>{{ formatDate(item.scheduledDateTime) }}</span>
        </template>
        <template v-slot:top>
            <v-toolbar dark>
                <v-tabs v-model="tab" dark>
                    <v-tab>
                        Banner Posts
                    </v-tab>
                    <v-tab>
                        Emails
                    </v-tab>
                </v-tabs>
                <v-spacer></v-spacer>
                <BannerModal
                    v-if="tab == 0"
                    :edited-item="editedBanner"
                    :is-new="isNewCommunication"
                    @emit-add="add"
                    @emit-update="update"
                    @emit-close="close"
                />
                <EmailModal
                    v-if="tab == 1"
                    :edited-item="editedEmail"
                    :is-new="isNewCommunication"
                    @emit-send="add"
                    @emit-update="update"
                    @emit-close="close"
                />
            </v-toolbar>
        </template>
        <template v-slot:item.actions="{ item }">
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
        <template v-slot:item.priority="{ item }">
            <span>{{ formatPriority(item.priority) }}</span>
        </template>
        <template v-slot:no-data>
            <span>Nothing to show here.</span>
        </template>
    </v-data-table>
</template>
<script lang="ts">
import { Component, Vue, Watch, Emit } from "vue-property-decorator";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import BannerFeedback from "@/models/bannerFeedback";
import Communication, {
    CommunicationType,
    CommunicationStatus
} from "@/models/adminCommunication";
import BannerModal from "@/components/core/modals/BannerModal.vue";
import EmailModal from "@/components/core/modals/EmailModal.vue";
import { ResultType } from "@/constants/resulttype";
import { ICommunicationService } from "@/services/interfaces";
import { faWater } from "@fortawesome/free-solid-svg-icons";
import moment from "moment";

@Component({
    components: {
        BannerModal,
        EmailModal
    }
})
export default class CommunicationTable extends Vue {
    private communicationList: Communication[] = [];
    private bannerList: Communication[] = [];
    private emailList: Communication[] = [];
    private communicationService!: ICommunicationService;
    private isLoading: boolean = false;
    private showFeedback: boolean = false;
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: ""
    };
    // 0: Banners, 1: Emails
    private tab: number = 0;
    private isNewCommunication: boolean = true;
    private headers: any[] = [];
    private editedBanner: Communication = {
        id: "-1",
        text: "",
        subject: "",
        communicationTypeCode: CommunicationType.Banner,
        communicationStatusCode: CommunicationStatus.New,
        priority: 10,
        version: 0,
        scheduledDateTime: moment(new Date()).toDate(),
        effectiveDateTime: moment(new Date()).toDate(),
        expiryDateTime: moment(new Date())
            .add(1, "days")
            .toDate()
    };

    private editedEmail: Communication = {
        id: "-1",
        subject: "",
        communicationTypeCode: CommunicationType.Email,
        communicationStatusCode: CommunicationStatus.New,
        text: "<p></p>",
        priority: 10,
        scheduledDateTime: moment(new Date()).toDate(),
        effectiveDateTime: moment(new Date()).toDate(),
        expiryDateTime: moment(new Date()).toDate(),
        version: 0
    };

    private defaultBanner: Communication = {
        id: "-1",
        text: "",
        subject: "",
        communicationTypeCode: CommunicationType.Banner,
        communicationStatusCode: CommunicationStatus.New,
        version: 0,
        priority: 10,
        scheduledDateTime: moment(new Date()).toDate(),
        effectiveDateTime: moment(new Date()).toDate(),
        expiryDateTime: moment(new Date())
            .add(1, "days")
            .toDate()
    };

    private defaultEmail: Communication = {
        id: "-1",
        subject: "",
        communicationTypeCode: CommunicationType.Email,
        communicationStatusCode: CommunicationStatus.New,
        text: "<p></p>",
        priority: 10,
        scheduledDateTime: moment(new Date()).toDate(),
        effectiveDateTime: moment(new Date()).toDate(),
        expiryDateTime: moment(new Date()).toDate(),
        version: 0
    };

    private mounted() {
        this.communicationService = container.get(
            SERVICE_IDENTIFIER.CommunicationService
        );
        this.headers = this.bannerHeaders;
        this.loadCommunicationList();
    }

    @Watch("tab")
    private onTabChange(val: any) {
        if (!val) {
            // Banners
            this.communicationList = this.bannerList;
            this.headers = this.bannerHeaders;
        } else {
            // Emails
            this.communicationList = this.emailList;
            this.headers = this.emailHeaders;
        }
    }

    private bannerHeaders: any[] = [
        {
            text: "Subject",
            value: "subject",
            align: "start",
            width: "20%",
            sortable: false
        },
        {
            text: "Effective On",
            value: "effectiveDateTime"
        },
        {
            text: "Expires On",
            value: "expiryDateTime"
        },
        {
            text: "Text",
            value: "text",
            sortable: false
        },
        {
            text: "Actions",
            value: "actions",
            sortable: false
        }
    ];

    private emailHeaders: any[] = [
        {
            text: "Subject",
            value: "subject",
            align: "start",
            width: "20%",
            sortable: false
        },
        {
            text: "Scheduled For",
            value: "scheduledDateTime"
        },
        {
            text: "Priority",
            value: "priority"
        },
        {
            text: "Status",
            value: "communicationStatusCode"
        },
        {
            text: "Actions",
            value: "actions",
            sortable: false
        }
    ];

    private formatDate(date: Date): string {
        return new Date(Date.parse(date + "Z")).toLocaleString();
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
            this.editedEmail.scheduledDateTime = moment
                .utc(item.scheduledDateTime)
                .toDate();
        } else {
            this.editedBanner = item;
            this.editedBanner.effectiveDateTime = moment
                .utc(item.effectiveDateTime)
                .toDate();
            this.editedBanner.expiryDateTime = moment
                .utc(item.expiryDateTime)
                .toDate();
        }
    }

    private sortCommunicationsByDate(
        isDescending: boolean,
        columnName: string
    ) {
        this.communicationList.sort((a, b) => {
            let first!: Date;
            let second!: Date;
            if (columnName === "effectiveDateTime") {
                first = a.effectiveDateTime;
                second = b.effectiveDateTime;
            } else if (columnName === "expiryDateTime") {
                first = a.expiryDateTime;
                second = b.expiryDateTime;
            } else {
                return 0;
            }

            if (first > second) {
                return isDescending ? -1 : 1;
            } else if (first < second) {
                return isDescending ? 1 : -1;
            }
            return 0;
        });
    }

    private customSort(
        items: Communication[],
        index: any[],
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
        if (
            item.communicationTypeCode === CommunicationType.Email &&
            item.communicationStatusCode != CommunicationStatus.New
        ) {
            return true;
        }
        return false;
    }

    private loadCommunicationList() {
        this.communicationService
            .getAll()
            .then((banners: Communication[]) => {
                this.parseComms(banners);
            })
            .catch((err: any) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error loading banners"
                };
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
        this.emailList = communication.filter(
            (comm: Communication) =>
                comm.communicationTypeCode === CommunicationType.Email
        );
        if (this.tab === 0) {
            this.communicationList = this.bannerList;
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
                communicationStatusCode: CommunicationStatus.New,
                priority: comm.priority,
                version: 0,
                scheduledDateTime: comm.scheduledDateTime,
                effectiveDateTime: comm.effectiveDateTime,
                expiryDateTime: comm.expiryDateTime
            })
            .then(() => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Success,
                    title: "Success",
                    message: "Communication Added."
                };
                this.loadCommunicationList();
            })
            .catch((err: any) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Add communication failed, please try again"
                };
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
                communicationStatusCode: CommunicationStatus.New,
                priority: comm.priority,
                version: comm.version,
                scheduledDateTime: comm.scheduledDateTime,
                effectiveDateTime: comm.effectiveDateTime,
                expiryDateTime: comm.expiryDateTime
            })
            .then(() => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Success,
                    title: "Success",
                    message: "Communication Updated."
                };
                this.loadCommunicationList();
            })
            .catch((err: any) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error updating communication. Please try again."
                };
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
                    message: "Communication Deleted."
                };
                this.loadCommunicationList();
            })
            .catch((err: any) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error deleting communication. Please try again."
                };
            })
            .finally(() => {
                this.isLoading = false;
                this.emitResult();
            });
    }

    private close() {
        this.editedBanner = Object.assign({}, this.defaultBanner);
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
