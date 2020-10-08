<template>
    <v-dialog v-model="dialog" persistent max-width="1000px">
        <template #activator="{ on, attrs }">
            <v-btn color="primary" dark v-bind="attrs" v-on="on"
                >New Email Communication</v-btn
            >
        </template>
        <v-card dark>
            <v-card-title>
                <span class="headline">{{ formTitle }}</span>
            </v-card-title>
            <v-card-text>
                <v-form ref="form" lazy-validation>
                    <!-- Subject and priority -->
                    <v-row>
                        <v-col cols="8">
                            <v-text-field
                                v-model="editedItem.subject"
                                label="Subject"
                                :rules="[v => !!v || 'Subject is required']"
                                validate-on-blur
                                required
                            ></v-text-field>
                        </v-col>
                        <v-col>
                            <v-datetime-picker
                                v-model="editedItem.scheduledDateTime"
                                requried
                                label="Scheduled For"
                            ></v-datetime-picker>
                        </v-col>
                        <v-col>
                            <v-select
                                v-model="editedItem.priority"
                                :items="priorityItems"
                                item-text="text"
                                item-value="number"
                                label="Priority"
                                :rules="[v => !!v || 'Priority is required']"
                                validate-on-blur
                                required
                            ></v-select>
                        </v-col>
                    </v-row>
                    <!-- Email Status & Draft/Publish button row -->
                    <v-row>
                        <v-col>
                            <v-text-field
                                v-model="editedItem.communicationStatusCode"
                                label="Status"
                                disabled
                            ></v-text-field>
                        </v-col>
                        <v-col>
                            <v-btn
                                color="blue darken-1"
                                text
                                @click="togglePublish()"
                                >{{ publishingStatus }}</v-btn
                            >
                        </v-col>
                    </v-row>
                    <!-- WYSIWYG Editor -->
                    <v-row>
                        <v-col>
                            <TiptapVuetify
                                v-model="editedItem.text"
                                :toolbar-attributes="{ color: 'gray' }"
                                placeholder="Write the email content here..."
                                :extensions="extensions"
                            />
                        </v-col>
                    </v-row>
                </v-form>
            </v-card-text>
            <!-- Buttons -->
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="blue darken-1" text @click="close()"
                    >Cancel</v-btn
                >
                <v-btn
                    v-if="isNew"
                    color="blue darken-1"
                    text
                    @click="saveChanges()"
                    >Send</v-btn
                >
                <v-btn v-else color="blue darken-1" text @click="saveChanges()"
                    >Update</v-btn
                >
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script lang="ts">
import { Component, Vue, Watch, Emit, Prop } from "vue-property-decorator";
import container from "@/plugins/inversify.config";
import Communication, {
    CommunicationStatus
} from "@/models/adminCommunication";
import moment from "moment";
import {
    TiptapVuetify,
    Heading,
    Bold,
    Italic,
    Strike,
    Underline,
    Code,
    Paragraph,
    BulletList,
    OrderedList,
    ListItem,
    Link,
    Blockquote,
    HardBreak,
    History
} from "tiptap-vuetify";

@Component({
    components: {
        TiptapVuetify
    }
})
export default class EmailModal extends Vue {
    @Prop() editedItem!: Communication;
    @Prop() isNew!: number;

    private dialog = false;
    private priorityItems = [
        { text: "Urgent", number: 1000 },
        { text: "High", number: 100 },
        { text: "Standard", number: 10 },
        { text: "Low", number: 1 }
    ];
    private extensions: any = [
        History,
        Blockquote,
        Link,
        Underline,
        Strike,
        Bold,
        Italic,
        ListItem,
        BulletList,
        OrderedList,
        [
            Heading,
            {
                options: {
                    levels: [1, 2, 3, 4]
                }
            }
        ],
        Bold,
        Code,
        Paragraph,
        HardBreak
    ];

    @Watch("editedItem")
    private onPropChange() {
        if (!this.isNew) {
            this.dialog = true;
        }
    }

    private get formTitle(): string {
        return this.isNew ? "New Email" : "Edit Email";
    }

    @Watch("dialog")
    private onDialogChange(val: any) {
        val || this.close();
    }

    private close() {
        this.$nextTick(() => {
            (this.$refs.form as Vue & {
                resetValidation: () => any;
            }).resetValidation();
            this.dialog = false;
            this.emitClose();
        });
    }

    private contentValid(): boolean {
        return this.editedItem.text.replace("<[^>]*>", "") !== ""
            ? true
            : false;
    }

    private saveChanges() {
        if (
            (this.$refs.form as Vue & { validate: () => boolean }).validate() &&
            this.contentValid()
        ) {
            if (this.isNew) {
                this.emitSend();
            } else {
                this.emitUpdate();
            }
            this.close();
            (this.$refs.form as Vue & {
                resetValidation: () => any;
            }).resetValidation();
        }
    }

    private get isDraft(): boolean {
        return (
            this.editedItem.communicationStatusCode ===
            CommunicationStatus.Draft
        );
    }

    private get publishingStatus(): string {
        return this.isDraft ? "Publish" : "Draft";
    }

    private togglePublish() {
        if (this.isDraft) {
            this.editedItem.communicationStatusCode = CommunicationStatus.New;
        } else {
            this.editedItem.communicationStatusCode = CommunicationStatus.Draft;
        }
        this.saveChanges();
    }

    @Emit()
    private emitSend() {
        return this.editedItem;
    }

    @Emit()
    private emitUpdate() {
        return this.editedItem;
    }

    @Emit()
    private emitClose() {
        return;
    }
}
</script>
