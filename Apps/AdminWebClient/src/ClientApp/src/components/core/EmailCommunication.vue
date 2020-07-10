<style lang="scss" scoped>
.card-title {
    background-color: #272727;
}
</style>
<template>
    <v-card dark>
        <v-card-title class="card-title" dark
            >New Email Communication</v-card-title
        >
        <v-spacer></v-spacer>
        <v-card-text>
            <v-form ref="form" class="px-5" lazy-validation>
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
                <!-- WYSIWYG Editor -->
                <v-row>
                    <v-col>
                        <TiptapVuetify
                            v-model="content"
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

@Component({
    components: {
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
    private content: string = "<p></p>";

    private contentValid(): boolean {
        return this.content.replace("<[^>]*>", "") !== "" ? true : false;
    }

    private send() {
        this.isLoading = true;
        this.isFinishedLoading();
        if (
            (this.$refs.form as Vue & { validate: () => boolean }).validate() &&
            this.contentValid()
        ) {
            // NETWORK REQUEST GOES HERE!
            // Temp data below.
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
            this.isLoading = false;
            this.emitResult();
        }
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
