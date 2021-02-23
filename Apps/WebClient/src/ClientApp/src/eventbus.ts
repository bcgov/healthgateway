import Vue from "vue";

const EventBus = new Vue();
export default EventBus;

export class EventMessageName {
    public static readonly CreateNote = "note-create";
    public static readonly EditNote = "note-edit";
    public static readonly TimelineCovidCard = "timeline-covid-card";
}
