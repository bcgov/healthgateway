<script lang="ts">
import {
    Blockquote,
    Bold,
    BulletList,
    Code,
    HardBreak,
    Heading,
    History,
    Italic,
    Link,
    ListItem,
    OrderedList,
    Paragraph,
    Strike,
    TiptapVuetify,
    Underline,
} from "tiptap-vuetify";
import { extend, ValidationProvider } from "vee-validate";
import { Component, Emit, Prop, Vue, Watch } from "vue-property-decorator";

import type Communication from "@/models/adminCommunication";
import { CommunicationStatus } from "@/models/adminCommunication";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";

extend("dateValid", {
    validate(
        value: unknown,
        args: unknown[] | Record<string, StringISODateTime>
    ) {
        // We know is a record
        args = args as Record<string, StringISODateTime>;
        let effectiveTime = new DateWrapper(args.effective, { isUtc: true });
        let expiryTime = new DateWrapper(args.expiry, { isUtc: true });
        if (effectiveTime.isBefore(expiryTime)) {
            return true;
        }
        return "Effective date must occur before expiry date.";
    },
    params: ["effective", "expiry"],
});

@Component({
    components: {
        ValidationProvider,
        TiptapVuetify,
    },
})
export default class BannerModal extends Vue {
    private dialog = false;
    private extensions = [
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
                    levels: [1, 2, 3, 4],
                },
            },
        ],
        Bold,
        Code,
        Paragraph,
        HardBreak,
    ];

    @Prop() editedItem!: Communication;
    @Prop() isNew!: number;
    @Prop() isInApp!: boolean;

    private get isDraft(): boolean {
        return (
            this.editedItem.communicationStatusCode ===
            CommunicationStatus.Draft
        );
    }

    private get publishingStatus(): string {
        return this.isDraft ? "Publish" : "Draft";
    }

    @Watch("editedItem")
    private onPropChange() {
        if (!this.isNew) {
            this.dialog = true;
        }
    }

    private get formTitle(): string {
        if (this.isInApp) {
            return this.isNew ? "New In-App Post" : "Edit In-App Post";
        }
        return this.isNew ? "New Banner Post" : "Edit Banner Post";
    }

    private get effectiveDateTime(): Date {
        return new DateWrapper(this.editedItem.effectiveDateTime, {
            isUtc: true,
        }).toJSDate();
    }

    private set effectiveDateTime(date: Date) {
        if (date) {
            this.editedItem.effectiveDateTime = new DateWrapper(
                date.toISOString(),
                { isUtc: true }
            ).toISO(true);
        }
    }

    private get expiryDateTime(): Date {
        return new DateWrapper(this.editedItem.expiryDateTime, {
            isUtc: true,
        }).toJSDate();
    }

    private set expiryDateTime(date: Date) {
        if (date) {
            this.editedItem.expiryDateTime = new DateWrapper(
                date.toISOString(),
                { isUtc: true }
            ).toISO(true);
        }
    }

    private dateTimeRules(
        effective: StringISODateTime,
        expiry: StringISODateTime
    ) {
        return "dateValid:" + effective + "," + expiry;
    }

    private dateTimeValid(): boolean {
        let effectiveTime = new DateWrapper(this.editedItem.effectiveDateTime, {
            isUtc: true,
        });
        let expiryTime = new DateWrapper(this.editedItem.expiryDateTime, {
            isUtc: true,
        });
        return effectiveTime.isBefore(expiryTime);
    }

    @Watch("dialog")
    private onDialogChange(dialogIsOpen: boolean) {
        dialogIsOpen || this.close();
    }

    private save() {
        this.editedItem.scheduledDateTime = new DateWrapper().toISO(true);
        if (
            (this.$refs.form as Vue & { validate: () => boolean }).validate() &&
            this.dateTimeValid()
        ) {
            if (!this.isNew) {
                this.emitUpdate(this.editedItem);
            } else {
                this.emitAdd(this.editedItem);
            }
            this.close();
        }
    }

    private togglePublish() {
        if (this.isDraft) {
            this.editedItem.communicationStatusCode = CommunicationStatus.New;
        } else {
            this.editedItem.communicationStatusCode = CommunicationStatus.Draft;
        }
        this.save();
    }

    private close() {
        this.$nextTick(() => {
            (
                this.$refs.form as Vue & {
                    resetValidation: () => void;
                }
            ).resetValidation();
            this.dialog = false;
            this.emitClose();
        });
    }

    @Emit()
    private emitAdd(communication: Communication) {
        return communication;
    }

    @Emit()
    private emitUpdate(communication: Communication) {
        return communication;
    }

    @Emit()
    private emitClose() {
        return;
    }
}
</script>

<template>
    <v-dialog v-model="dialog" persistent max-width="1000px">
        <template #activator="{ on, attrs }">
            <v-btn color="primary" dark v-bind="attrs" v-on="on">
                {{
                    isInApp
                        ? "New In-App Communication"
                        : "New Banner Communication"
                }}
            </v-btn>
        </template>
        <v-card dark>
            <v-card-title>
                <span class="headline">{{ formTitle }}</span>
            </v-card-title>
            <v-card-text>
                <v-form ref="form" lazy-validation>
                    <v-row>
                        <v-col>
                            <ValidationProvider
                                v-slot="{ errors }"
                                :rules="
                                    dateTimeRules(
                                        editedItem.effectiveDateTime,
                                        editedItem.expiryDateTime
                                    )
                                "
                            >
                                <v-datetime-picker
                                    v-model="effectiveDateTime"
                                    label="Effective On"
                                />
                                <div class="error-message">
                                    {{ errors[0] }}
                                </div>
                            </ValidationProvider>
                        </v-col>
                        <v-col>
                            <ValidationProvider
                                v-slot="{ errors }"
                                :rules="
                                    dateTimeRules(
                                        editedItem.effectiveDateTime,
                                        editedItem.expiryDateTime
                                    )
                                "
                            >
                                <v-datetime-picker
                                    v-model="expiryDateTime"
                                    label="Expires On"
                                />
                                <div class="error-message">
                                    {{ errors[0] }}
                                </div>
                            </ValidationProvider>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col>
                            <v-text-field
                                v-model="editedItem.subject"
                                label="Subject"
                                maxlength="100"
                                :rules="[(v) => !!v || 'Subject is required']"
                                validate-on-blur
                                required
                            />
                        </v-col>
                        <v-col>
                            <v-text-field
                                v-model="editedItem.communicationStatusCode"
                                label="Status"
                                disabled
                            />
                        </v-col>
                        <v-col>
                            <v-btn
                                color="blue darken-1"
                                text
                                @click="togglePublish()"
                            >
                                {{ publishingStatus }}
                            </v-btn>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col>
                            <TiptapVuetify
                                v-model="editedItem.text"
                                :toolbar-attributes="{ color: 'gray' }"
                                placeholder="Write the post content here..."
                                :extensions="extensions"
                            />
                        </v-col>
                    </v-row>
                </v-form>
            </v-card-text>
            <v-card-actions>
                <v-spacer />
                <v-btn color="blue darken-1" text @click="close()">
                    Cancel
                </v-btn>
                <v-btn color="blue darken-1" text @click="save()"> Save </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<style scoped lang="scss">
.error-message {
    color: #ff5252 !important;
}
</style>
