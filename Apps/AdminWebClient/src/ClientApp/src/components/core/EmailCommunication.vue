<style lang="scss" scoped>
.card-title {
    background-color: #272727;
}
.error-message {
    color: #ff5252 !important;
}
</style>
<template>
    <v-card dark>
        <v-card-title class="card-title" dark
            >New Email Communication</v-card-title
        >
        <v-spacer></v-spacer>
        <v-card-text>
            <v-form class="px-5" ref="form" lazy-validation>
                <!-- Subject and priority -->
                <v-row>
                    <v-col class="d-flex pt-0" cols="9">
                        <v-text-field
                            v-model="subject"
                            label="Subject"
                            :rules="[v => !!v || 'Subject is required']"
                            validate-on-blur
                            required
                        ></v-text-field>
                    </v-col>
                    <v-col class="d-flex pt-0" cols="3">
                        <v-select
                            v-model="priority"
                            :items="priorityItems"
                            label="Priority"
                            :rules="[v => !!v || 'Priority is required']"
                            validate-on-blur
                            required
                        ></v-select>
                    </v-col>
                </v-row>
                <v-row>
                    <v-col>
                        <ValidationProvider
                            mode="eager"
                            v-slot="{
                                errors
                            }"
                            :rules="htmlRules(content)"
                        >
                            <TiptapVuetify
                                :toolbar-attributes="{ color: 'gray' }"
                                v-model="content"
                                placeholder="Write the email content here..."
                                :extensions="extensions"
                            />
                            <div class="mt-2">
                                <span class="error-message">{{
                                    errors[0]
                                }}</span>
                            </div>
                        </ValidationProvider>
                    </v-col>
                </v-row>
            </v-form>
        </v-card-text>
        <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="blue darken-1" text @click="cancel()">Cancel</v-btn>
            <v-btn color="blue darken-1" text @click="send()">Send</v-btn>
        </v-card-actions>
    </v-card>
</template>

<script lang="ts">
import { Component, Vue, Watch, Emit } from "vue-property-decorator";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import BannerFeedback from "@/models/bannerFeedback";
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
import { ResultType } from "@/constants/resulttype";
import { ICommunicationService } from "@/services/interfaces";
import { ValidationProvider, extend, validate } from "vee-validate";
import { required } from "vee-validate/dist/rules";
import moment from "moment";

extend("htmlRules", {
    validate(value: any, args: any) {
        if (args.content.replace(/(<([^>]+)>)/gi, "") !== "") {
            return true;
        }
        return "Email body content is required.";
    },
    params: ["content"]
});

@Component({
    components: {
        ValidationProvider,
        TiptapVuetify
    }
})
export default class EmailCommunication extends Vue {
    private isLoading: boolean = false;
    private showFeedback: boolean = false;
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: ""
    };
    private priorityItems = ["Urgent", "Medium", "Low"];

    private subject: string = "";
    private priority: string = "";
    private extensions: any = [
        History,
        Blockquote,
        Link,
        Underline,
        Strike,
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
    private content: string = "<p></p>";

    private htmlRules(htmlBody: string): string {
        return "htmlRules:" + htmlBody;
    }

    private emitResult() {
        this.isFinishedLoading();
        this.bannerFeedbackInfo();
        this.shouldShowFeedback();
    }

    private contentValid(): boolean {
        return this.content.replace(/(<([^>]+)>)/gi) !== "" ? true : false;
    }

    private send() {
        if (
            (this.$refs.form as Vue & { validate: () => boolean }).validate() &&
            this.contentValid()
        ) {
            // Send to backend
            this.showFeedback = true;
            this.bannerFeedback = {
                type: ResultType.Success,
                title: "Success",
                message: "Email sent."
            };
            this.subject = "";
            this.priority = "";
            this.content = "<p></p>";
            (this.$refs.form as Vue & {
                resetValidation: () => any;
            }).resetValidation();
        }
    }

    private cancel() {
        (this.$refs.form as Vue & {
            resetValidation: () => any;
        }).resetValidation();
        this.subject = "";
        this.priority = "";
        this.content = "<p></p>";
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
