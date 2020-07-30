<template>
    <v-dialog v-model="dialog" persistent max-width="500px">
        <template v-slot:activator="{ on, attrs }">
            <v-btn color="primary" dark v-bind="attrs" v-on="on"
                >New Banner Communication</v-btn
            >
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
                                v-slot="{
                                    errors
                                }"
                                :rules="
                                    dateTimeRules(
                                        editedItem.effectiveDateTime,
                                        editedItem.expiryDateTime
                                    )
                                "
                            >
                                <v-datetime-picker
                                    v-model="editedItem.effectiveDateTime"
                                    requried
                                    label="Effective On"
                                ></v-datetime-picker>
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </ValidationProvider>
                        </v-col>
                        <v-col>
                            <ValidationProvider
                                v-slot="{
                                    errors
                                }"
                                :rules="
                                    dateTimeRules(
                                        editedItem.effectiveDateTime,
                                        editedItem.expiryDateTime
                                    )
                                "
                            >
                                <v-datetime-picker
                                    v-model="editedItem.expiryDateTime"
                                    required
                                    label="Expires On"
                                ></v-datetime-picker>
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </ValidationProvider>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col>
                            <v-text-field
                                v-model="editedItem.subject"
                                label="Subject"
                                maxlength="100"
                                :rules="[v => !!v || 'Subject is required']"
                                validate-on-blur
                                required
                            ></v-text-field>
                        </v-col>
                    </v-row>
                    <v-row>
                        <v-col>
                            <v-textarea
                                v-model="editedItem.text"
                                label="Message"
                                maxlength="1000"
                                :rules="[v => !!v || 'Text is required']"
                                validate-on-blur
                                required
                            ></v-textarea>
                        </v-col>
                    </v-row>
                </v-form>
            </v-card-text>
            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="blue darken-1" text @click="close()"
                    >Cancel</v-btn
                >
                <v-btn color="blue darken-1" text @click="save()">Save</v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>
<script lang="ts">
import { Component, Vue, Watch, Emit, Prop } from "vue-property-decorator";
import container from "@/plugins/inversify.config";
import Communication from "@/models/adminCommunication";
import { ResultType } from "@/constants/resulttype";
import { ValidationProvider, extend, validate } from "vee-validate";
import { required, email } from "vee-validate/dist/rules";
import moment from "moment";

extend("dateValid", {
    validate(value: any, args: any) {
        if (moment(args.effective).isBefore(moment(args.expiry))) {
            return true;
        }
        return "Effective date must occur before expiry date.";
    },
    params: ["effective", "expiry"]
});
@Component({
    components: {
        ValidationProvider
    }
})
export default class BannerModal extends Vue {
    private dialog: boolean = false;

    @Prop() editedItem!: Communication;
    @Prop() isNew!: number;

    @Watch("editedItem")
    private onPropChange() {
        if (!this.isNew) {
            this.dialog = true;
        }
    }

    private get formTitle(): string {
        return this.isNew ? "New Banner Post" : "Edit Banner Post";
    }

    private dateTimeRules(effective: Date, expiry: Date) {
        return "dateValid:" + effective.toString() + "," + expiry.toString();
    }

    private dateTimeValid(): boolean {
        return moment(this.editedItem.effectiveDateTime).isBefore(
            moment(this.editedItem.expiryDateTime)
        );
    }

    @Watch("dialog")
    private onDialogChange(val: any) {
        val || this.close();
    }

    private save() {
        this.editedItem.scheduledDateTime = new Date();
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

    private close() {
        this.$nextTick(() => {
            (this.$refs.form as Vue & {
                resetValidation: () => any;
            }).resetValidation();
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
