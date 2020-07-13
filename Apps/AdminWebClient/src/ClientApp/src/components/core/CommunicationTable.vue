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
                    :edited-item="editedItem"
                    :edited-index="editedIndex"
                    @emit-add="add"
                    @emit-update="update"
                    @emit-close="close"
                />
                <EmailModal
                    v-if="tab == 1"
                    :edited-item="editedItem"
                    :edited-index="editedIndex"
                    @emit-add="add"
                    @emit-update="update"
                    @emit-close="close"
                />
            </v-toolbar>
        </template>
        <template v-slot:item.actions="{ item }">
            <v-btn @click="editItem(item)">
                <font-awesome-icon icon="edit" size="1x"> </font-awesome-icon>
            </v-btn>
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
import Communication from "@/models/communication";
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
    private editedIndex: number = -1;
    private editedItem: Communication = {
        id: "-1",
        text: "",
        subject: "",
        version: 0,
        effectiveDateTime: moment(new Date()).toDate(),
        expiryDateTime: moment(new Date())
            .add(1, "days")
            .toDate()
    };

    private defaultItem: Communication = {
        id: "-1",
        text: "",
        subject: "",
        version: 0,
        effectiveDateTime: new Date(),
        expiryDateTime: moment(new Date())
            .add(1, "days")
            .toDate()
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

    private headers: any[] = [];

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
            text: "Email Content",
            width: "20%",
            value: "text",
            sortable: false
        },
        {
            text: "Scheduled For",
            value: "effectiveDateTime"
        },
        {
            text: "Priority",
            value: "priority"
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

    private editItem(item: Communication) {
        this.editedIndex = this.communicationList.indexOf(item);
        this.editedItem = item;
        this.editedItem.effectiveDateTime = new Date(item.effectiveDateTime);
        this.editedItem.expiryDateTime = new Date(item.expiryDateTime);
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

    private loadCommunicationList() {
        console.log("retrieving communications...");
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
        // this.bannerList = communication.filter((comm: Communication) => comm.type === "banner");
        // this.emailList = communication.filter((comm: Communication) => comm.type === "email");
        this.bannerList = communication;
        this.communicationList = this.bannerList;
    }

    private add(comm: Communication): void {
        this.isLoading = true;
        this.isFinishedLoading();
        this.communicationService
            .add({
                subject: comm.subject,
                text: comm.text,
                version: 0,
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
                version: comm.version,
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

    private close() {
        this.editedItem = Object.assign({}, this.defaultItem);
        this.editedIndex = -1;
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
