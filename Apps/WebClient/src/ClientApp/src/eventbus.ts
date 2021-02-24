import Vue from "vue";

const EventBus = new Vue();
export default EventBus;

export class EventMessageName {
    public static readonly CreateNote = "note-create";
    public static readonly EditNote = "note-edit";
    public static readonly AddedNote = "note-added";

    public static readonly CalendarDateEventClick = "calendar-date-eventClick";
    public static readonly CalendarMonthUpdated = "calendar-month-updated";

    public static readonly TimelinePageUpdate = "timeline-page-update";
    public static readonly SelectedFilter = "filter-selected";

    public static readonly TimelineViewUpdated = "timeline-view-updated";
    public static readonly TimelineCovidCard = "timeline-covid-card";

    public static readonly ViewEntryDetails = "view-entry-details";
}
