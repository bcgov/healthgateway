import Vue from "vue";

const EventBus = new Vue();
export default EventBus;

export class EventMessageName {
    public static readonly CalendarDateEventClick = "calendar-date-eventClick";
    public static readonly CalendarMonthUpdated = "calendar-month-updated";
    public static readonly IdleLogoutWarning = "idle-logout-warning";
    public static readonly TimelineCreateNote = "timeline-create-note";
    public static readonly TimelineEntryAddClose = "timeline-entry-add-close";
    public static readonly TimelineEntryAdded = "timeline-entry-added";
    public static readonly TimelineEntryDeleted = "timeline-entry-deleted";
    public static readonly TimelineEntryEdit = "timeline-entry-edit";
    public static readonly TimelineEntryEditClose = "timeline-entry-edit-close";
    public static readonly TimelineEntryUpdated = "timeline-entry-updated";
    public static readonly TimelinePageUpdate = "timeline-page-update";
    public static readonly TimelinePrintView = "timeline-print-view";
}
