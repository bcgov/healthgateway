<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Getter } from "vuex-class";
import { IconDefinition, faUserMd } from "@fortawesome/free-solid-svg-icons";
import CommentSectionComponent from "@/components/timeline/commentSection.vue";
import User from "@/models/user";
import PhoneUtil from "@/utility/phoneUtil";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";

@Component({
    components: {
        CommentSection: CommentSectionComponent,
    },
})
export default class EncounterTimelineEntryComponent extends Vue {
    @Prop() entry!: EncounterTimelineEntry;
    @Prop() index!: number;
    @Prop() datekey!: string;
    @Getter("user", { namespace: "user" }) user!: User;

    private detailsVisible = false;

    private get entryIcon(): IconDefinition {
        return faUserMd;
    }

    private toggleDetails(): void {
        this.detailsVisible = !this.detailsVisible;
    }

    private formatPhone(phoneNumber: string): string {
        return PhoneUtil.formatPhone(phoneNumber);
    }
}
</script>

<template>
    <b-col class="timelineCard">
        <b-row class="entryHeading">
            <b-col class="icon leftPane">
                <font-awesome-icon
                    :icon="entryIcon"
                    size="2x"
                ></font-awesome-icon>
            </b-col>
            <b-col class="entryTitle">
                <b-row class="justify-content-between">
                    <b-col cols="auto" data-testid="encounterTitle">
                        <strong>{{ entry.practitionerName }}</strong>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <b-row class="my-2">
            <b-col class="leftPane"></b-col>
            <b-col>
                <b-row>
                    <b-col cols="auto" data-testid="encounterDescription">
                        <strong> Specialty Description: </strong>
                        {{ entry.specialtyDescription }}
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <div class="d-flex flex-row-reverse">
                            <b-btn
                                v-b-toggle="
                                    'entryDetails-' + index + '-' + datekey
                                "
                                variant="link"
                                class="detailsButton"
                                @click="toggleDetails()"
                            >
                                <span class="when-opened">
                                    <font-awesome-icon
                                        icon="chevron-up"
                                        aria-hidden="true"
                                    ></font-awesome-icon
                                ></span>
                                <span class="when-closed">
                                    <font-awesome-icon
                                        icon="chevron-down"
                                        aria-hidden="true"
                                    ></font-awesome-icon
                                ></span>
                                <span v-if="detailsVisible">Hide Details</span>
                                <span v-else>View Details</span>
                            </b-btn>
                        </div>
                        <b-collapse
                            :id="'entryDetails-' + index + '-' + datekey"
                            v-model="detailsVisible"
                        >
                            <div>
                                <div class="detailSection">
                                    <div>
                                        <strong>Location:</strong>
                                    </div>
                                    <div>
                                        {{ entry.clinic.name }}
                                    </div>
                                    <div>
                                        {{ entry.clinic.address }}
                                    </div>
                                    <div>
                                        {{
                                            formatPhone(
                                                entry.clinic.phoneNumber
                                            )
                                        }}
                                    </div>
                                </div>
                            </div>
                        </b-collapse>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <CommentSection :parent-entry="entry"></CommentSection>
    </b-col>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

.timelineCard {
    border-radius: $radius $radius $radius $radius;
    border-color: $soft_background;
    border-style: solid;
    border-width: 2px;
}

.entryTitle {
    background-color: $soft_background;
    color: $primary;
    padding: 13px 15px;
    margin-right: -1px;
    border-radius: 0px $radius 0px 0px;
}

.icon {
    background-color: $primary;
    color: white;
    text-align: center;
    padding: 10px 0;
    border-radius: $radius 0px 0px 0px;
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

.commentButton {
    border-radius: $radius;
}

.newComment {
    border-radius: $radius;
}

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}
</style>
