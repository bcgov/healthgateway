<script lang="ts">
import Vue from "vue";

import TimelineEntry, { EntryType } from "@/models/timelineEntry";

import { Component, Emit, Prop } from "vue-property-decorator";
import MedicationTimelineComponent from "./medication.vue";
import ImmunizationTimelineComponent from "./immunization.vue";
import LaboratoryTimelineComponent from "./laboratory.vue";
import EncounterTimelineComponent from "./encounter.vue";
import NoteTimelineComponent from "./note.vue";

@Component({
    components: {
        MedicationComponent: MedicationTimelineComponent,
        ImmunizationComponent: ImmunizationTimelineComponent,
        LaboratoryComponent: LaboratoryTimelineComponent,
        EncounterComponent: EncounterTimelineComponent,
        NoteComponent: NoteTimelineComponent,
    },
})
export default class EntrycardTimelineComponent extends Vue {
    @Prop() datekey!: string;
    @Prop() entry!: TimelineEntry;
    @Prop() index!: number;

    private get EntryType(): typeof EntryType {
        return EntryType;
    }

    @Emit()
    public onChange(object: any) {
        return object;
    }

    @Emit()
    public onRemove(object: any) {
        return object;
    }

    @Emit()
    public onEdit(object: any) {
        return object;
    }

    @Emit()
    public onClose(object: any) {
        return object;
    }
}
</script>

<template>
    <b-row class="entryCard">
        <MedicationComponent
            v-if="entry.type === EntryType.Medication"
            :key="entry.id"
            :datekey="datekey"
            :entry="entry"
            :index="index"
        />
        <ImmunizationComponent
            v-if="entry.type === EntryType.Immunization"
            :key="entry.id"
            :datekey="datekey"
            :entry="entry"
            :index="index"
        />
        <LaboratoryComponent
            v-if="entry.type === EntryType.Laboratory"
            :key="entry.id"
            :datekey="datekey"
            :entry="entry"
            :index="index"
        />
        <EncounterComponent
            v-if="entry.type === EntryType.Encounter"
            :key="entry.id"
            :datekey="datekey"
            :entry="entry"
            :index="index"
        />
        <NoteComponent
            v-if="entry.type === EntryType.Note"
            :key="entry.id"
            :datekey="datekey"
            :entry="entry"
            :index="index"
        />
    </b-row>
</template>

<style lang="scss" scoped>
.entryCard {
    width: 100%;
    padding-left: 50px;
    padding-top: 10px;
    padding-bottom: 10px;
}
</style>
