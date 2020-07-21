<template>
    <v-dialog v-model="dialog" persistent max-width="1000px">
        <template v-slot:activator="{ on, attrs }">
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
                        <v-col cols="9">
                            <v-text-field
                                v-model="editedItem.subject"
                                label="Subject"
                                :rules="[v => !!v || 'Subject is required']"
                                validate-on-blur
                                required
                            ></v-text-field>
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
                    @click="emitSend()"
                    >Send</v-btn
                >
                <v-btn v-else color="blue darken-1" text @click="emitUpdate()"
                    >Update</v-btn
                >
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script lang="ts">
import { Component, Vue, Watch, Emit, Prop } from "vue-property-decorator";
import container from "@/plugins/inversify.config";
import Communication from "@/models/adminCommunication";
import { ResultType } from "@/constants/resulttype";
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
    private dialog: boolean = false;
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

    @Prop() editedItem!: Communication;
    @Prop() isNew!: number;

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

    @Emit()
    private emitSend() {
        if (
            (this.$refs.form as Vue & { validate: () => boolean }).validate() &&
            this.contentValid()
        ) {
            this.close();
            (this.$refs.form as Vue & {
                resetValidation: () => any;
            }).resetValidation();
            return this.editedItem;
        }
    }

    @Emit()
    private emitUpdate() {
        if (
            (this.$refs.form as Vue & { validate: () => boolean }).validate() &&
            this.contentValid()
        ) {
            this.close();
            return this.editedItem;
        }
    }

    @Emit()
    private emitClose() {
        return;
    }
}
</script>
