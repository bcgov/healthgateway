<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            class="mt-5"
        ></BannerFeedbackComponent>
        <v-row>
            <v-col>
                <v-row>
                    <v-col no-gutters>
                        <template>
                            <v-data-table
                                :headers="headers"
                                :items="communicationList"
                                class="elevation-1"
                            >
                                <template
                                    v-slot:item.effectiveDateTime="{ item }"
                                >
                                    <span>{{
                                        formatDate(item.effectiveDateTime)
                                    }}</span>
                                </template>
                                <template v-slot:item.expiryDateTime="{ item }">
                                    <span>{{
                                        formatDate(item.expiryDateTime)
                                    }}</span>
                                </template>
                                <template v-slot:top>
                                    <v-toolbar dark>
                                        <v-toolbar-title
                                            >Communications</v-toolbar-title
                                        >
                                        <v-spacer></v-spacer>
                                        <v-dialog
                                            v-model="dialog"
                                            max-width="500px"
                                        >
                                            <template
                                                v-slot:activator="{ on, attrs }"
                                            >
                                                <v-btn
                                                    color="primary"
                                                    dark
                                                    v-bind="attrs"
                                                    v-on="on"
                                                    >New Communication</v-btn
                                                >
                                            </template>
                                            <v-card dark>
                                                <v-card-title>
                                                    <span class="headline">{{
                                                        formTitle
                                                    }}</span>
                                                </v-card-title>
                                                <v-card-text>
                                                    <v-form
                                                        ref="form"
                                                        lazy-validation
                                                    >
                                                        <v-row>
                                                            <v-col>
                                                                <v-datetime-picker
                                                                    v-model="
                                                                        editedItem.effectiveDateTime
                                                                    "
                                                                    requried
                                                                    label="Effective On"
                                                                ></v-datetime-picker>
                                                            </v-col>
                                                            <v-col>
                                                                <v-datetime-picker
                                                                    v-model="
                                                                        editedItem.expiryDateTime
                                                                    "
                                                                    required
                                                                    label="Expires On"
                                                                ></v-datetime-picker>
                                                            </v-col>
                                                        </v-row>
                                                        <v-row>
                                                            <v-col>
                                                                <v-text-field
                                                                    v-model="
                                                                        editedItem.subject
                                                                    "
                                                                    label="Subject"
                                                                    maxlength="100"
                                                                    :rules="[
                                                                        v =>
                                                                            !!v ||
                                                                            'Subject is required'
                                                                    ]"
                                                                    validate-on-blur
                                                                    required
                                                                ></v-text-field>
                                                            </v-col>
                                                        </v-row>
                                                        <v-row>
                                                            <v-col>
                                                                <v-textarea
                                                                    v-model="
                                                                        editedItem.text
                                                                    "
                                                                    label="Message"
                                                                    maxlength="1000"
                                                                    :rules="[
                                                                        v =>
                                                                            !!v ||
                                                                            'Text is required'
                                                                    ]"
                                                                    validate-on-blur
                                                                    required
                                                                ></v-textarea>
                                                            </v-col>
                                                        </v-row>
                                                    </v-form>
                                                </v-card-text>
                                                <v-card-actions>
                                                    <v-spacer></v-spacer>
                                                    <v-btn
                                                        color="blue darken-1"
                                                        text
                                                        @click="close()"
                                                        >Cancel</v-btn
                                                    >
                                                    <v-btn
                                                        color="blue darken-1"
                                                        text
                                                        @click="save()"
                                                        >Save</v-btn
                                                    >
                                                </v-card-actions>
                                            </v-card>
                                        </v-dialog>
                                    </v-toolbar>
                                </template>
                                <template v-slot:item.actions="{ item }">
                                    <v-btn @click="editItem(item)">
                                        <font-awesome-icon
                                            icon="edit"
                                            size="1x"
                                        >
                                        </font-awesome-icon>
                                    </v-btn>
                                </template>
                                <template v-slot:no-data>
                                    <span>Nothing to show here.</span>
                                </template>
                            </v-data-table>
                        </template>
                    </v-col>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>

<script lang="ts">
import { Component, Vue, Watch } from "vue-property-decorator";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import LoadingComponent from "@/components/core/Loading.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import BannerFeedback from "@/models/bannerFeedback";
import Communication from "@/models/communication";
import { ResultType } from "@/constants/resulttype";
import { ICommunicationService } from "@/services/interfaces";
import { faWater } from "@fortawesome/free-solid-svg-icons";

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent
    }
})
export default class CommunicationView extends Vue {
    private isLoading: boolean = false;
    private showFeedback: boolean = false;
    private communicationList: Communication[] = [];
    private communicationService!: ICommunicationService;
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: ""
    };
    private dialog: boolean = false;
    private editedIndex: number = -1;

    @Watch("dialog")
    private onDialogChange(val: any) {
        val || this.close();
    }

    private get formTitle(): string {
        return this.editedIndex === -1 ? "New Item" : "Edit Item";
    }

    private editedItem: Communication = {
        id: "-1",
        text: "",
        subject: "",
        effectiveDateTime: new Date(),
        expiryDateTime: new Date()
    };

    private defaultItem: Communication = {
        id: "-1",
        text: "",
        subject: "",
        effectiveDateTime: new Date(),
        expiryDateTime: new Date()
    };

    private headers: any[] = [
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

    private mounted() {
        this.communicationService = container.get(
            SERVICE_IDENTIFIER.CommunicationService
        );
        this.loadCommunicationList();
    }

    private formatDate(date: Date): string {
        return new Date(Date.parse(date + "Z")).toLocaleString();
    }

    private close() {
        this.dialog = false;
        this.$nextTick(() => {
            this.editedItem = Object.assign({}, this.defaultItem);
            this.editedIndex = -1;
            (this.$refs.form as Vue & {
                resetValidation: () => any;
            }).resetValidation();
        });
    }

    private save() {
        if ((this.$refs.form as Vue & { validate: () => boolean }).validate()) {
            if (this.editedIndex > -1) {
                // Assign (this.editedItem) to item at this.editedIndex
                // this.communicationService.update(this.communicationList[this.editedIndex], this.editedItem);
            } else {
                // Add new item (this.editedItem)
                this.communicationService.add(this.editedItem);
            }
            this.close();
        }
    }

    private editItem(item: Communication) {
        this.editedIndex = this.communicationList.indexOf(item);
        this.editedItem = Object.assign({}, item);
        this.dialog = true;
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
            .then(banners => {
                this.communicationList = banners;
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

    private add(comm: Communication): void {
        this.isLoading = true;
        this.communicationService
            .add({
                subject: comm.subject,
                text: comm.text,
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
            });
    }
}
</script>
