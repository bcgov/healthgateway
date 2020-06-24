<template>
  <v-container>
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <BannerFeedbackComponent
      :show-feedback.sync="showFeedback"
      :feedback="bannerFeedback"
      class="mt-5"
    ></BannerFeedbackComponent>
    <v-form ref="form" v-model="valid">
      <v-row>
        <v-col md="9">
          <v-row>
            <v-col>
              <v-text-field
                v-model="subject"
                label="Subject"
                maxlength="100"
                :rules="[v => !!v || 'Subject is required']"
                required
              ></v-text-field>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-textarea
                v-model="text"
                label="Communication Text"
                maxlength="1000"
                :rules="[v => !!v || 'Text is required']"
                required
              ></v-textarea>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-datetime-picker
                v-model="effectiveDateTime"
                requried
                label="Effective On"
              ></v-datetime-picker>
            </v-col>
            <v-col>
              <v-datetime-picker
                v-model="expiryDateTime"
                required
                label="Expires On"
              ></v-datetime-picker>
            </v-col>
          </v-row>
          <v-row justify="end" no-gutters>
            <v-btn :disabled="!valid" @click="add()">Add</v-btn>
          </v-row>
        </v-col>
      </v-row>
    </v-form>

    <v-row>
      <v-col md="9">
        <v-row>
          <v-col no-gutters>
            <v-data-table
              :headers="tableHeaders"
              :items="communicationList"
              :custom-sort="customSort"
              :items-per-page="5"
            >
              <template v-slot:item.effectiveDateTime="{ item }">
                <span>{{ formatDate(item.effectiveDateTime) }}</span>
              </template>
              <template v-slot:item.expiryDateTime="{ item }">
                <span>{{ formatDate(item.expiryDateTime) }}</span>
              </template>
            </v-data-table>
          </v-col>
        </v-row>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import LoadingComponent from "@/components/core/Loading.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import BannerFeedback from "@/models/bannerFeedback";
import Communication from "@/models/communication";
import { ResultType } from "@/constants/resulttype";
import { ICommunicationService } from "@/services/interfaces";

@Component({
  components: {
    LoadingComponent,
    BannerFeedbackComponent
  }
})
export default class CommunicationView extends Vue {
  private valid: boolean = false;
  private subject: string = "";
  private text: string = "";
  private effectiveDateTime: Date = new Date();
  private expiryDateTime: Date = new Date();
  private isLoading: boolean = false;
  private showFeedback: boolean = false;
  private bannerFeedback: BannerFeedback = {
    type: ResultType.NONE,
    title: "",
    message: ""
  };

  private tableHeaders: any[] = [
    {
      text: "Subject",
      value: "subject",
      width: "30%"
    },
    {
      text: "Effective On",
      value: "effectiveDateTime",
      width: "20%"
    },
    {
      text: "Expires On",
      value: "expiryDateTime",
      width: "20%"
    },
    {
      text: "Text",
      value: "text",
      width: "40%"
    }
  ];

  private communicationList: Communication[] = [];

  private communicationService!: ICommunicationService;

  mounted() {
    this.communicationService = container.get(
      SERVICE_IDENTIFIER.CommunicationService
    );
    this.clearForm();
    this.loadCommunicationList();
  }

  private sortCommunicationsByDate(isDescending: boolean, columnName: string) {
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
    // Dummy data for testing banner list
    //  This will need to be disabled when we hook this to the real service via vue service.
    this.communicationList = [
      {
        id: "b1001",
        subject: "banner 1",
        text:
          "B1 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
        effectiveDateTime: new Date("2020-08-05"),
        expiryDateTime: new Date("2020-08-07")
      },
      {
        id: "b1002",
        subject: "banner 2",
        text:
          "B2 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
        effectiveDateTime: new Date("2020-06-30"),
        expiryDateTime: new Date("2020-07-01")
      },
      {
        id: "b1003",
        subject: "banner 3",
        text:
          "B3 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
        effectiveDateTime: new Date("2020-05-03"),
        expiryDateTime: new Date("2020-05-28")
      },
      {
        id: "b1004",
        subject: "banner 4",
        text:
          "B4 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
        effectiveDateTime: new Date("2020-07-30"),
        expiryDateTime: new Date("2020-08-01")
      },
      {
        id: "b1005",
        subject: "banner 5",
        text:
          "B5 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
        effectiveDateTime: new Date("2020-06-23"),
        expiryDateTime: new Date("2020-06-25")
      },

      {
        id: "b10012",
        subject: "banner 1",
        text:
          "B1 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
        effectiveDateTime: new Date("2020-06-18"),
        expiryDateTime: new Date("2020-07-20")
      },
      {
        id: "b10052",
        subject: "banner 52",
        text:
          "B5 - Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum s",
        effectiveDateTime: new Date("2020-06-15"),
        expiryDateTime: new Date("2020-06-17")
      }
    ];

    //Todo for 8213: Enable the following when 8213 update vue service is complete.
    //this.communicationService
    //    .getCommunications()
    //    .then(banners => {
    //        this.communicationList = [];
    //        this.communicationList.push(...banners);
    //    })
    //    .catch(err => {
    //        this.showFeedback = true;
    //        this.bannerFeedback = {
    //            type: ResultType.Error,
    //            title: "Error",
    //            message: "Error loading banners"
    //        };
    //    })
    //    .finally(() => {
    //        this.isLoading = false;
    //    });
  }

  private formatDate(date: Date): string {
    return new Date(Date.parse(date + "Z")).toLocaleString();
  }

  private clearForm() {
    this.subject = "";
    this.text = "";
    this.effectiveDateTime = new Date();
    this.expiryDateTime = new Date();
    this.expiryDateTime.setDate(this.effectiveDateTime.getDate() + 1);
  }

  private add(): void {
    this.isLoading = true;

    this.communicationService
      .add({
        subject: this.subject,
        text: this.text,
        effectiveDateTime: this.effectiveDateTime,
        expiryDateTime: this.expiryDateTime
      })
      .then(() => {
        this.clearForm();
        this.showFeedback = true;
        this.bannerFeedback = {
          type: ResultType.Success,
          title: "Success",
          message: "Communication Added."
        };
      })
      .catch(err => {
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
