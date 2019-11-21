<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

.entryHeading {
  border-radius: 25px;
}

.entryCard {
  width: 100%;
  padding-left: 50px;
  padding-top: 10px;
  padding-bottom: 10px;
}

.entryTitle {
  background-color: $soft_background;
  color: $primary;
  padding: 13px 15px;
  font-weight: bold;
}

.icon {
  background-color: $primary;
  color: white;
  text-align: center;
  padding: 10px 0;
  border-radius: $radius 0px 0px $radius;
}

.leftPane {
  width: 60px;
}

.detailsButton {
  padding: 0px;
}

.detailSection {
  margin-top: 15px;
}

.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
  display: none;
}
</style>

<template>
  <b-row class="entryCard">
    <b-col>
      <b-row class="entryHeading">
        <b-col class="icon leftPane" cols="0">
          <i :class="'fas fa-2x ' + getEntryIcon(entry)"></i>
        </b-col>
        <b-col class="entryTitle">
          {{ entry.medication.brandName }}
        </b-col>
      </b-row>
      <b-row>
        <b-col class="leftPane" cols="0"> </b-col>
        <b-col>
          <b-row>
            <b-col>
              {{ entry.medication.genericName }}
            </b-col>
          </b-row>
          <b-row>
            <b-col>
              <b-btn
                v-b-toggle="'entryDetails-' + index + '-' + datekey"
                variant="link"
                class="detailsButton"
                @click="toggleDetails(entry)"
              >
                <span class="when-opened">
                  <i class="fa fa-chevron-down" aria-hidden="true"></i
                ></span>
                <span class="when-closed">
                  <i class="fa fa-chevron-up" aria-hidden="true"></i
                ></span>
                <span v-if="detailsVisible">Hide Details</span>
                <span v-else>View Details</span>
              </b-btn>
              <b-collapse :id="'entryDetails-' + index + '-' + datekey">
                <div v-if="detailsLoaded">
                  <div class="detailSection">
                    <div>
                      <strong>Practitioner:</strong>
                      {{ entry.practitionerSurname }}
                    </div>
                    <div>
                      <strong>Prescription #:</strong>
                      {{ entry.prescriptionIdentifier }}
                    </div>
                  </div>
                  <div class="detailSection">
                    <div>
                      <strong>Quantity:</strong>
                      {{ entry.medication.quantity }}
                    </div>
                    <div>
                      <strong>Strength:</strong>
                      {{ entry.medication.strength }}
                      {{ entry.medication.strengthUnit }}
                    </div>
                    <div>
                      <strong>Form:</strong>
                      {{ entry.medication.form }}
                    </div>
                    <div>
                      <strong>Manufacturer:</strong>
                      {{ entry.medication.manufacturer }}
                    </div>
                  </div>
                  <div class="detailSection">
                    <strong
                      >{{ entry.medication.isPin ? "PIN" : "DIN" }}:</strong
                    >
                    {{ entry.medication.din }}
                  </div>
                  <div class="detailSection">
                    <div>
                      <strong>Filled At:</strong>
                    </div>
                    <div>
                      {{ entry.pharmacy.name }}
                    </div>
                    <div>
                      {{ entry.pharmacy.address }}
                    </div>
                    <div v-if="entry.pharmacy.phoneType != faxPhoneType">
                      {{ formatPhoneNumber(entry.pharmacy.phoneNumber) }}
                    </div>
                    <div class="detailSection border border-dark p-2 small">
                      <div>
                        <strong>Directions for Use:</strong>
                      </div>
                      <div class="pt-2">
                        {{ entry.directions }}
                      </div>
                    </div>
                  </div>
                </div>
                <div v-else-if="isLoading">
                  <div class="d-flex align-items-center">
                    <strong>Loading...</strong>
                    <b-spinner class="ml-5"></b-spinner>
                  </div>
                </div>
                <div v-else-if="hasErrors" class="pt-1">
                  <b-alert :show="hasErrors" variant="danger">
                    <h5>Error</h5>
                    <span
                      >An unexpected error occured while processing the
                      request.</span
                    >
                  </b-alert>
                </div>
              </b-collapse>
            </b-col>
          </b-row>
        </b-col>
      </b-row>
    </b-col>
  </b-row>
</template>

<script lang="ts">
import Vue from "vue";
import { PhoneType } from "@/models/pharmacy";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import { Prop, Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";

@Component
export default class MedicationTimelineComponent extends Vue {
  @Prop() entry!: MedicationTimelineEntry;
  @Prop() index!: number;
  @Prop() datekey!: string;
  @Action("getMedication", { namespace: "medication" }) getMedication;
  @Action("getPharmacy", { namespace: "pharmacy" }) getPharmacy;
  private faxPhoneType: PhoneType = PhoneType.Fax;
  private isLoading: boolean = false;
  private hasErrors: boolean = false;

  private medicationLoaded: boolean = false;
  private pharmacyLoaded: boolean = false;

  private detailsVisible = false;

  get detailsLoaded(): boolean {
    return this.medicationLoaded && this.pharmacyLoaded;
  }

  private getEntryIcon(entry: any): string {
    return "fa-pills";
  }

  private toggleDetails(medicationEntry: MedicationTimelineEntry): void {
    this.detailsVisible = !this.detailsVisible;
    this.hasErrors = false;

    if (!this.detailsVisible) {
      return;
    }

    // If the medication or pharmacy details are loaded dont fetch again.
    if (this.medicationLoaded && this.pharmacyLoaded) {
      return;
    }

    console.log("Loading details");

    this.isLoading = true;
    var medicationPromise = this.getMedication({
      din: medicationEntry.medication.din
    });

    var pharmacyPromise = this.getPharmacy({
      pharmacyId: medicationEntry.pharmacy.id
    });

    Promise.all([medicationPromise, pharmacyPromise])
      .then(results => {
        // Load medication details
        if (results[0]) {
          medicationEntry.medication.populateFromModel(results[0]);
        }
        this.medicationLoaded = true;

        if (results[1]) {
          // Load pharmacy details
          medicationEntry.pharmacy.populateFromModel(results[1]);
        }
        this.pharmacyLoaded = true;

        this.isLoading = false;
      })
      .catch(err => {
        console.log("Error loading details");
        console.log(err);
        this.hasErrors = true;
        this.isLoading = false;
      });
  }

  private formatPhoneNumber(phoneNumber: string): string {
    phoneNumber = phoneNumber || "";
    return phoneNumber
      .replace(/[^0-9]/g, "")
      .replace(/(\d{3})(\d{3})(\d{4})/, "($1) $2-$3");
  }
}
</script>
