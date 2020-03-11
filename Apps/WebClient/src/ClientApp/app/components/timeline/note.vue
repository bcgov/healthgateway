<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

.entryHeading {
  border-radius: 25px;
}

.entryTitle {
  background-color: $soft_background;
  color: $primary;
  padding: 15px;
  font-weight: bold;
  word-wrap: break-word;
  width: 100%;
}

.editableEntryTitle {
  background-color: $soft_background;
  padding: 9px 15px 9px 15px;
  width: 100%;
  margin: 0px;
}

.entryDetails {
  word-wrap: break-word;
  padding-left: 15px;
}

.icon {
  background-color: $bcgold;
  color: white;
  text-align: center;
  padding: 10px 15px;
  border-radius: $radius 0px 0px $radius;
}

.leftPane {
  width: 60px;
  max-width: 60px;
}

.detailsButton {
  padding: 0px;
}

.detailSection {
  margin-top: 15px;
}
</style>

<template>
  <b-col>
    <b-form @submit="onSubmit" @reset="onReset">
      <b-row class="entryHeading">
        <b-col class="d-flex px-0">
          <div class="icon leftPane">
            <font-awesome-icon :icon="entryIcon" size="2x"></font-awesome-icon>
          </div>
          <div v-if="!editing" class="entryTitle">{{ entry.title }}</div>
          <b-row v-else class="editableEntryTitle">
            <b-form-input
              v-model="title"
              class="col-lg-7 col-md-7 col-6"
              type="text"
              placeholder="Title"
              maxlength="100"
            />
            <b-form-input
              v-model="date"
              class="col-lg-5 col-md-5 col-6"
              required
              type="date"
            ></b-form-input>
          </b-row>
        </b-col>
      </b-row>
      <b-row>
        <b-col class="leftPane"></b-col>
        <b-col>
          <b-row>
            <b-col v-if="!editing" class="entryDetails">
              {{ !detailsVisible ? entry.textSummary : entry.text }}
              <b-btn
                v-b-toggle="'entryDetails-' + entry.id"
                variant="link"
                class="detailsButton"
                @click="toggleDetails()"
              >
                <span v-if="detailsVisible && entry.textSummary != entry.text">Hide Details</span>
                <span v-else-if="entry.textSummary != entry.text">Read More</span>
              </b-btn>
            </b-col>
            <b-col v-if="editing">
              <b-form-textarea
                id="text"
                v-model="text"
                placeholder="Your note here"
                rows="3"
                max-rows="6"
                maxlength="1000"
              ></b-form-textarea>
            </b-col>
          </b-row>
          <b-row v-if="editing" class="py-2">
            <b-col class="d-flex flex-row-reverse">
              <div>
                <b-btn variant="light" type="reset">
                  <span>CANCEL</span>
                </b-btn>
                <b-btn variant="primary" type="submit">
                  <span>SAVE</span>
                </b-btn>
              </div>
            </b-col>
          </b-row>
        </b-col>
      </b-row>
    </b-form>
    <div v-if="hasErrors" class="pt-1">
      <b-alert :show="hasErrors" variant="danger">
        <h5>Error</h5>
        <span>An unexpected error occured while processing the request.</span>
      </b-alert>
    </div>
  </b-col>
</template>

<script lang="ts">
import Vue from "vue";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import { Prop, Component, Emit } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import { faEdit, IconDefinition } from "@fortawesome/free-solid-svg-icons";
import { IUserNoteService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import UserNote from "@/models/userNote";

@Component
export default class NoteTimelineComponent extends Vue {
  @Prop() editing!: boolean;
  @Prop() entry!: NoteTimelineEntry;

  private noteService: IUserNoteService;
  private text: string = "";
  private title: string = "";
  private date: string = new Date().toISOString().slice(0, 10);
  private detailsVisible = false;
  private hasErrors: boolean = false;

  mounted() {
    this.noteService = container.get<IUserNoteService>(
      SERVICE_IDENTIFIER.UserNoteService
    );
  }
  private get entryIcon(): IconDefinition {
    return faEdit;
  }

  private toggleDetails(): void {
    this.detailsVisible = !this.detailsVisible;
  }

  private onSubmit(evt: Event): void {
    evt.preventDefault();
    this.noteService
      .createNote({
        text: this.text,
        title: this.title,
        journalDateTime: this.date
      })
      .then(result => {
        this.onNoteAdded(result);
      })
      .catch(() => {
        this.hasErrors = true;
      });
  }

  private onReset(): void {
    this.close();
  }

  @Emit()
  public close() {
    return;
  }

  @Emit()
  public onNoteAdded(note: UserNote) {
    return note;
  }
}
</script>
