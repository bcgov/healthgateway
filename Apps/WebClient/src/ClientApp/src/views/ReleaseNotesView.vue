<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import ReleaseNoteComponent from "@/components/ReleaseNoteComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";

@Component({
    components: {
        BreadcrumbComponent,
        "release-note": ReleaseNoteComponent,
    },
})
export default class ReleaseNotesView extends Vue {
    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Release Notes",
            to: "/release-notes",
            active: true,
            dataTestId: "breadcrumb-release-notes",
        },
    ];
}
</script>

<template>
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <BreadcrumbComponent :items="breadcrumbItems" />
        <page-title title="Health Gateway Release Notes" />
        <release-note
            date="May 24, 2022"
            version="v1.5.10"
            title="Profile Page Address"
        >
            <ul>
                <li>
                    We added available addresses to the profile page, so you can
                    confirm that we have the most current information.
                </li>
            </ul>
        </release-note>
        <release-note
            date="April 25, 2022"
            version="v1.5.8"
            title="Landing Page Update"
        >
            <ul>
                <li>
                    We updated the the landing page to provide better
                    description of what to expect in the application, including
                    some screenshots.
                </li>
            </ul>
        </release-note>
        <release-note
            date="April 5, 2022"
            version="v1.5.7"
            title="Timeline Card Update"
        >
            <ul>
                <li>
                    We updated the timeline cards in the desktop view so it is
                    clear that they can be clicked to show more details.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 9, 2022"
            version="v1.5.5"
            title="Home Page Quick Links"
        >
            <ul>
                <li>
                    We added the ability to add quick links to specific data
                    sets to the home page, which will open the Timeline to a
                    pre-filtered view.
                </li>
            </ul>
        </release-note>
        <release-note
            date="February 9, 2022"
            version="v1.5.3"
            title="Error Messages"
        >
            <ul>
                <li>
                    We updated the error messages to provide more details about
                    the issue you might be experiencing and how to get help.
                </li>
            </ul>
        </release-note>
        <release-note
            date="February 2, 2022"
            version="v1.5.2"
            title="Records Loading Indicator"
        >
            <ul>
                <li>
                    We replaced the loading spinner with a loading alert so you
                    can view your records as they are loaded into the Health
                    Gateway timeline.
                </li>
            </ul>
        </release-note>
        <release-note
            date="January 19, 2022"
            version="v1.5.1"
            title="COVID Tests Update, Export Notes"
        >
            <ul>
                <li>
                    We updated the COVID test display to include with more clear
                    messaging depending on your test results.
                </li>
                <li>
                    We added a feature which allows you to export notes that you
                    have added to your profile as PDF or Excel files.
                </li>
            </ul>
        </release-note>
        <release-note
            date="December 15, 2021"
            version="v1.4.17"
            title="Breadcrumb Navigation"
        >
            <ul>
                <li>
                    We added breadcrumb navigation to assist in moving around
                    the application.
                </li>
            </ul>
        </release-note>
        <release-note
            date="December 7, 2021"
            version="v1.4.16"
            title="Login History, Exports Sorting, Dashboard Tiles"
        >
            <ul>
                <li>
                    We added login history in the profile page to show the last
                    five logins.
                </li>
                <li>
                    We updated the sort order on record exports to default to
                    most recent first.
                </li>
                <li>
                    We updated the dashboard tile order so the health records
                    link is first.
                </li>
            </ul>
        </release-note>
        <release-note
            date="October 29, 2021"
            version="v1.4.14"
            title="Canadian Proof of Vaccination"
        >
            <ul>
                <li>
                    We added a downloadable pdf for the Canadian proof of
                    vaccination.
                </li>
            </ul>
        </release-note>
        <release-note
            date="October 13, 2021"
            version="v1.4.13"
            title="COVID Vaccine Page"
        >
            <ul>
                <li>
                    We updated the COVID Vaccine page to provide a separate view
                    for the BC Vaccine Card and COVID vaccine details to make it
                    easier to save and print.
                </li>
            </ul>
        </release-note>
        <release-note
            date="September 13, 2021"
            version="v1.4.11"
            title="Save a copy of BC Vaccine Card"
        >
            <ul>
                <li>
                    We added a feature to allow citizens to save a copy of their
                    BC Vaccine Cards as images.
                </li>
            </ul>
        </release-note>
        <release-note
            date="September 7, 2021"
            version="v1.4.10"
            title="BC Vaccine Card"
        >
            <ul>
                <li>
                    We launched the BC Vaccine Card which provides citizens with
                    a proof of vaccination that they can present to businesses.
                    The BC Vaccine Card shows an indication of the vaccination
                    status of the individual and has a QR code that can be
                    scanned to verify their card.
                </li>
            </ul>
        </release-note>
        <release-note
            date="August 13, 2021"
            version="v1.4.8"
            title="Call Centre Vaccine Record Access"
        >
            <ul>
                <li>
                    We added the ability for government call centre and Service
                    BC agents to access and mail or print out COVID vaccine
                    records to citizens.
                </li>
            </ul>
        </release-note>
        <release-note
            date="July 29, 2021"
            version="v1.4.7"
            title="COVID Card Button, Menu Update"
        >
            <ul>
                <li>
                    We added a button in the Timeline to launch the COVID
                    Vaccine Card.
                </li>
                <li>
                    We updated the menu selection and moved the "add note"
                    button.
                </li>
            </ul>
        </release-note>
        <release-note
            date="July 14, 2021"
            version="v1.4.6"
            title="Export Records, BC Services Card Log In, App Icon"
        >
            <ul>
                <li>
                    We updated the export document for all record types and
                    added CSV and Excel options. These new formats will allow
                    you to quickly sort and filter your health information.
                </li>
                <li>
                    We updated the landing page log in button to provide more
                    clarity around the use of the BC Services Card mobile app
                    for secure login to the Health Gateway.
                </li>
                <li>
                    We updated the Health Gateway app icon so the icon is
                    visually distinct from your other apps and websites if you
                    bookmark the page.
                </li>
            </ul>
        </release-note>
        <release-note
            date="June 23, 2021"
            version="v1.4.4"
            title="In-App Banner"
        >
            <ul>
                <li>
                    We added a feature so we can post notices within the
                    application in the top banner.
                </li>
            </ul>
        </release-note>
        <release-note
            date="June 30, 2021"
            version="v1.4.5"
            title="Export Medication Records"
        >
            <ul>
                <li>
                    We enhanced the document generation service for the
                    medication record export. This will allow for better quality
                    PDFs and other document format types in the future.
                </li>
            </ul>
        </release-note>
        <release-note
            date="June 23, 2021"
            version="v1.4.4"
            title="In-App Banner"
        >
            <ul>
                <li>
                    We added a feature so we can post notices within the
                    application in the top banner.
                </li>
            </ul>
        </release-note>
        <release-note
            date="May 19, 2021"
            version="v1.4.2"
            title="Export Records"
        >
            <ul>
                <li>
                    We updated the tables within record exports so that they are
                    easier to read and columns do not overlap.
                </li>
            </ul>
        </release-note>
        <release-note
            date="May 12, 2021"
            version="v1.4.1"
            title="Resource Centre"
        >
            <ul>
                <li>
                    We added a resource centre which allows you to quickly
                    launch the FAQ, Release Notes, and COVID Immunization Card
                    if you've been vaccinated.
                </li>
            </ul>
        </release-note>
        <release-note
            date="May 5, 2021"
            version="v1.4.0"
            title="Special Authorities Status, Icons and Buttons"
        >
            <ul>
                <li>
                    We launched our Special Authority feature, providing access
                    to the approval status for drugs requiring approval under
                    specific medical circumstances.
                </li>
                <li>
                    We updated our icons and buttons to be consistent with our
                    design system.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 31, 2021"
            version="v1.3.13"
            title="Exclude on Medication Export, Email Verification, Timeline Updates"
        >
            <ul>
                <li>
                    We added the ability to exclude certain medications by brand
                    name on the medication history export.
                </li>
                <li>
                    We updated the email verification flow to indicate when
                    you've already verified your email.
                </li>
                <li>
                    We added an icon to be displayed when pdf attachments are
                    available. The input for comments was moved to the bottom of
                    the expanded timeline card page.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 24, 2021"
            version="v1.3.12"
            title="Update Profile Page, Health Visits, Immunization Text Filter"
        >
            <ul>
                <li>
                    We updated the profile page to add clarity on phone and
                    email verification.
                </li>
                <li>
                    We added Health Visits which show 7 years of health visits,
                    consultations and procedures billed to the Medical Services
                    Program.
                </li>
                <li>
                    We updated the text filter so that immunization details such
                    as product name, lot number and provider/clinic information
                    are filterable.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 17, 2021"
            version="v1.3.11"
            title="Special Authority Request Prep, Immunization Loading"
        >
            <ul>
                <li>
                    We started integration to a new data source to provide
                    access to the status of special drug requests.
                </li>
                <li>
                    We updated the loading of immunizations to automatically add
                    new immunizations to the timeline when they are available.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 10, 2021"
            version="v1.3.10"
            title="COVID-19 Immunization Card, Date Formats"
        >
            <ul>
                <li>
                    We added date of birth to the COVID-19 Immunization Card, to
                    support identity validation.
                </li>
                <li>
                    We updated the date formats in the Health Gateway so that
                    they are consistent and clear.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 3, 2021"
            version="v1.3.9"
            title="FAQ Page, Download COVID Card, Comments"
        >
            <ul>
                <li>
                    We added an FAQ page which publishes the most frequent
                    questions we've had about the application.
                </li>
                <li>
                    We added the ability to download a PDF copy of the COVID
                    Immunization Card.
                </li>
                <li>
                    We added the ability to filter for words in the comments
                    that are uploaded, so you can find them faster.
                </li>
            </ul>
        </release-note>
        <release-note
            date="February 24, 2021"
            version="v1.3.8"
            title="Icon for Comments, Expanded Details Page, Options Menu"
        >
            <ul>
                <li>
                    We added an icon to records which have comments that you've
                    added so you can quickly find them.
                </li>
                <li>
                    We added a details page which opens as a new pane for
                    reviewing your record details.
                </li>
                <li>
                    We updated the filter menu as it has the option of changing
                    timeline view to a calendar view.
                </li>
            </ul>
        </release-note>
        <release-note
            date="February 17, 2021"
            version="v1.3.7"
            title="Email Verification, COVID-19 Immunization Card, Error Messaging"
        >
            <ul>
                <li>
                    We updated the email verification flow to simplify the
                    registration flow and minimize errors when verifying.
                </li>
                <li>
                    We added a COVID-19 Immunization Card to provide a summary
                    card of COVID-19 vaccine doses, similar to the one provided
                    at clinics.
                </li>
                <li>
                    We updated our error messages to include trace ID to better
                    support troubleshooting with users.
                </li>
            </ul>
        </release-note>
        <release-note
            date="February 10, 2021"
            version="v1.3.6"
            title="Date Pickers, Menu Update"
        >
            <ul>
                <li>
                    We moved the date pickers on the export records page to an
                    advanced filter option, to streamline pdf generation.
                </li>
                <li>
                    We updated the navigation menu, moving the profile option to
                    the header with the logout button.
                </li>
            </ul>
        </release-note>
        <release-note
            date="February 3, 2021"
            version="v1.3.5"
            title="Condensed Timeline, BC Gov Logo on Exports, OpenShift Upgrade"
        >
            <ul>
                <li>
                    We updated the timeline view so more records are visible on
                    a small screen, especially important for mobile screens.
                </li>
                <li>
                    We added the BC government logo to as a header for all of
                    the PDFs that are generated in the Health Gateway.
                </li>
                <li>
                    We migrated our application to a newer platform which will
                    provide improved performance.
                </li>
            </ul>
        </release-note>
        <release-note
            date="January 27, 2021"
            version="v1.3.4"
            title="MSP Visits, Forecasted Immunizations"
        >
            <ul>
                <li>
                    We launched our MSP visit feature, which will provide access
                    to your last seven years of health visits billed to the
                    Medical Services Plan, including GP and specialist
                    consultations.
                </li>
                <li>
                    We updated our immunization records to include recommended
                    future immunizations based on our BC immunization schedule
                    and COVID-19 immunization program. If you've received the
                    first dose of your COVID-19 immunization, you will see a
                    recommended date for your next dose in both the health care
                    timeline and export records pages.
                </li>
            </ul>
        </release-note>
        <release-note
            date="January 20, 2021"
            version="v1.3.3"
            title="Filters, Date Pickers, Export Records, Offline Mode"
        >
            <ul>
                <li>
                    We added a colour fill and badge on the filter menu to
                    highlight if filters have been applied. We also updated our
                    date pickers, as the date format placeholder text wasn't
                    visible on some platforms.
                </li>
                <li>
                    We added a pop-over to point users to the Export Records for
                    downloading pdfs for their records. We also added the
                    ability to clear the date picker and record type selected.
                </li>
                <li>
                    We created an offline mode for planned outages so that users
                    know when we are doing maintenance on the application.
                </li>
            </ul>
        </release-note>
        <release-note
            date="January 13, 2021"
            version="v1.3.2"
            title="Phone Verification, Platform Migration Prep, Updated Statistics"
        >
            <ul>
                <li>
                    We improved the phone verification flow from registration,
                    so it is more obvious where to enter the verification code.
                </li>
                <li>
                    We are moving the application to an upgraded platform and
                    have started preparing for the migration.
                </li>
                <li>
                    We enhanced our statistics so we can better understand usage
                    of our application and continue improving the product.
                </li>
            </ul>
        </release-note>
        <release-note
            date="January 5, 2021"
            version="v1.3.1.7"
            title="Export Records Date Picker, Immunization Display"
        >
            <ul>
                <li>
                    We updated the export records date picker and display of
                    immunization records to show additional data.
                </li>
            </ul>
        </release-note>
        <release-note
            date="December 17, 2020"
            version="v1.3.1.4"
            title="Immunization Records"
        >
            <ul>
                <li>
                    We launched the feature which will allow Health Gateway
                    users to view their immunization records, including COVID-19
                    immunizations.
                </li>
            </ul>
        </release-note>
        <release-note
            date="December 9, 2020"
            version="v1.3.1.3"
            title="Fix for Dependent Name Validation, COVID-19 Test Result Export"
        >
            <ul>
                <li>
                    We fixed a bug on dependent name validation and implemented
                    the COVID-19 Test Result record export.
                </li>
            </ul>
        </release-note>
        <release-note
            date="December 8, 2020"
            version="v1.3.1.2"
            title="Updated Add Dependent Form"
        >
            <ul>
                <li>
                    Updated field name to reflect Given Names needed when
                    entering dependent's name.
                </li>
            </ul>
        </release-note>
        <release-note
            date="December 7, 2020"
            version="v1.3.1.1"
            title="Launched Parent Access to Children's COVID-19 Results"
        >
            <ul>
                <li>
                    We launched the feature enabling parent access to their
                    children's COVID-19 results. This will allow parents to view
                    results for their children under the age of 12.
                </li>
            </ul>
        </release-note>
        <release-note
            date="December 3, 2020"
            version="v1.3.0.4"
            title="Code cleanup, Package Updates, Test Coverage"
        >
            <ul>
                <li>
                    Following this team prep sprint, we planned for our upcoming
                    release phase. We also cleaned up code, added test coverage
                    and updated technical packages.
                </li>
            </ul>
        </release-note>
        <release-note
            date="November 25, 2020"
            version="v1.3.0"
            title="Technical Upgrade, Code Cleanup, Non-Photo Card Access over 19 Years"
        >
            <ul>
                <li>
                    During this sprint, in preparation for the next Health
                    Gateway release phase, we upgraded components of our
                    technology platform. We improved. We also performed regular
                    code cleanup, and improved test coverage for our code.
                </li>
                <li>
                    We enabled non-photo BC Services card access for those over
                    19. This will support temporary residents and students of BC
                    who need access to their COVID-19 test results through the
                    Health Gateway.
                </li>
            </ul>
        </release-note>
        <release-note
            date="November 12, 2020"
            version="v1.2.10"
            title="Parent Access to Children's COVID-19 Results, OpenShift Upgrade"
        >
            <ul>
                <li>
                    We continued development to support parent access to their
                    children's COVID-19 results. This update will also enable
                    parents to receive text notifications when the results are
                    ready. Further work to complete this feature will require
                    2-4 weeks.
                </li>
                <li>
                    We started to preparation for a migration to the new
                    upgraded OpenShift Container Platform environment where the
                    Health Gateway application is hosted.
                </li>
            </ul>
        </release-note>
        <release-note
            date="October 28, 2020"
            version="v1.2.9"
            title="Optimized Application Infrastructure, Age Check on Registration, Initial Work for Parent Access to Children's COVID19 results"
        >
            <ul>
                <li>
                    We made some changes in our infrastructure to improve our
                    application's performance under load.
                </li>
                <li>
                    We added an age check on registration. In an upcoming
                    sprint, BC residents will be able to use non-photo BC
                    Services Card to access their health infomration in the
                    Health Gateway.
                </li>
                <li>
                    We started working on a feature which will allow parents to
                    access their children's COVID-19 results.
                </li>
            </ul>
        </release-note>
        <release-note
            date="October 7, 2020"
            version="v1.2.8"
            title="Unsupported Browsers, Filter Counts and Pagination, New Functional Test Tooling, Security Enhancements"
        >
            <ul>
                <li>
                    We added a message for users trying to use unsupported
                    browsers recommending use of modern web browsers.
                </li>
                <li>
                    We added counts to filter types and an ability to change the
                    number of records per page.
                </li>
                <li>
                    We adopted a new functional testing tool which improves our
                    automated testing of all Health Gateway features.
                </li>
                <li>
                    We remediated low severity security issues as identified in
                    a recent vulnerability scan of our admin application.
                </li>
            </ul>
        </release-note>
        <release-note
            date="September 28, 2020"
            version="v1.2.7.3"
            title="Bug Fix for Record Details"
        >
            <ul>
                <li>
                    We fixed a bug which was causing record details to collapse
                    when trying to expand.
                </li>
            </ul>
        </release-note>
        <release-note
            date="September 25, 2020"
            version="v1.2.7.2"
            title="Medication History Report Bug Fix"
        >
            <ul>
                <li>
                    We fixed a bug causing reports with large numbers of
                    medication records to generate blank pdf reports.
                </li>
            </ul>
        </release-note>
        <release-note
            date="September 23, 2020"
            version="v1.2.7"
            title="Advance Filter, Footer Menu, Medication History Report"
        >
            <ul>
                <li>
                    We added a filter menu which expands and collapses, to free
                    up space for content.
                </li>
                <li>
                    The links in the footer menu were updated, making it easier
                    to find more information about the Health Gateway, such as
                    release notes, contact information, terms of service and
                    more.
                </li>
                <li>
                    We implemented the first iteration of our Reports section
                    which will allow users to save pdf reports of their health
                    records, starting with medication history.
                </li>
            </ul>
        </release-note>
        <release-note
            date="September 10, 2020"
            version="v1.2.6"
            title="Menu Enhancements, MSP and Immunization Records"
        >
            <ul>
                <li>
                    We implemented minor enhancements to the menu buttons and
                    mobile menu.
                </li>
                <li>
                    This sprint included integration work with Immunization and
                    Medical Service Plan billing systems. Data is now displayed
                    in the Health Gateway development environment and final
                    testing and validation will take place prior to production
                    launch.
                </li>
            </ul>
        </release-note>
        <release-note
            date="August 27, 2020"
            version="v1.2.5.3"
            title="Fixed Title for COVID Lab Result, Mobile Menu Close"
        >
            <ul>
                <li>
                    This was a minor fix to the COVID-19 Lab Result timeline
                    entry, to show a more descriptive title, rather than the
                    type of test administered.
                </li>
                <li>
                    The mobile menu was updated to show an "X" to close the
                    menu.
                </li>
            </ul>
        </release-note>
        <release-note
            date="August 26, 2020"
            version="v1.2.5"
            title="COVID-19 Test Results, Calendar Enhancements, Application Star Rating, Health Insights"
        >
            <ul>
                <li>
                    To support the COVID-19 response, the Health Gateway will
                    now provide access to COVID-19 test results for BC
                    residents. This feature includes a notification via
                    SMS/Email that the COVID-19 result is available.
                </li>
                <li>
                    We enhanced the date picker for the calendar to be easier to
                    use.
                </li>
                <li>
                    Upon logout, users will now be asked to provide a star
                    rating.
                </li>
                <li>
                    We implemented the first iteration for our Health Insights
                    page to support users in understanding patterns in their
                    health care.
                </li>
            </ul>
        </release-note>
        <release-note
            date="August 13, 2020"
            version="v1.2.4.2"
            title="Hotfix to add Google Verification for Search Console"
        />
        <release-note
            date="August 12, 2020"
            version="v1.2.4"
            title="Landing Page, Troubleshooting, Analytics"
        >
            <ul>
                <li>
                    The landing page was updated to provide some clarity around
                    available data in the Health Gateway and goals to be
                    realized through future features.
                </li>
            </ul>
        </release-note>
        <release-note
            date="July 29, 2020"
            version="v1.2.3"
            title="Calendar Navigation, Email Communications, Decommission HNClient."
        >
            <ul>
                <li>
                    We implemented features that now allow users to navigate
                    within the calendar month view by adding a date picker. Note
                    that only months and years with health records can be
                    selected, enabling faster discovery of health records. Also,
                    when a user toggles between month and list view, the date
                    context remains. That is, a user can flip back and forth
                    between the two views while staying on the same relative
                    date range.
                </li>
                <li>
                    The Health Gateway team can now send communication emails to
                    users.
                </li>
                <li>
                    The remaining code for HNClient, an application used for the
                    previous dispensed medication integration has now been
                    removed. Any associated technical debt is no longer
                    affecting Health Gateway.
                </li>
            </ul>
        </release-note>
        <release-note
            date="July 15, 2020"
            version="v1.2.2"
            title="Navigation, Usability and Token Storage"
        >
            <ul>
                <li>
                    As the Health Gateway could potentially display numerous
                    records for users, we improved the navigation from the month
                    calendar. You can now click on the icon on a specific date
                    and be taken to the list view for details.
                </li>
                <li>
                    The sidebar menu has been updated to show sub-menu items to
                    minimize clutter and only show buttons in the context of the
                    page they are relevant to (Timeline and Profile).
                </li>
                <li>
                    The "Notes" feature of the site will show a popover with a
                    brief description of what the feature can be used for.
                </li>
                <li>
                    The loading spinner was enhanced to convey to the user that
                    records are being fetched.
                </li>
                <li>
                    Finally, token storage was enhanced to allow for larger
                    access tokens in cookie storage.
                </li>
            </ul>
        </release-note>
        <release-note
            date="July 6, 2020"
            version="v1.2.1.1"
            title="Fixed Protective Word"
        >
            <ul>
                <li>
                    We implemented a fix to allow user to enter protective word
                    correctly.
                </li>
            </ul>
        </release-note>
        <release-note
            date="July 3, 2020"
            version="v1.2.1"
            title="Banner Posts, UI Improvements"
        >
            <ul>
                <li>
                    We implemented a feature which will allow us to post banner
                    messages to be displayed on the landing page, such as
                    important updates.
                </li>
                <li>
                    Improvements to the user interface include updates to the
                    timeline cards and renaming the "Notes" filter to "My Notes"
                    for clarity.
                </li>
            </ul>
        </release-note>
        <release-note
            date="June 26, 2020"
            version="v1.2.0.2"
            title="Fixed Brand Name Look-up"
        >
            <ul>
                <li>
                    This hot-fix implemented improvements to the medication
                    query, to address lengthy loading times and occasional
                    timeouts.
                </li>
            </ul>
        </release-note>
        <release-note
            date="June 23, 2020"
            version="v1.2.0.1"
            title="Enhanced Logging"
        >
            <ul>
                <li>We improved our logs to assist with debugging.</li>
            </ul>
        </release-note>
        <release-note
            date="May 4, 2020"
            version="v1.1.8.1"
            title="Launched Enhanced Medications"
        >
            <ul>
                <li>
                    <strong>Full History of Medications</strong>
                    - Users of the Health Gateway can now see the full history
                    of medications in the PharmaNet system, as far back as 1996.
                    Due to system limitations, the Health Gateway returns up to
                    20,000 dispensed medication records.
                </li>
            </ul>
        </release-note>
        <release-note
            date="April 29, 2020"
            version="v1.1.8"
            title="Security, Communications and Profile Menu"
        >
            <ul>
                <li>
                    <strong>Security</strong>
                    - To close out our final sprint for Release 1.1, the Health
                    Gateway team ensured compliance with government security
                    standards as we transition to our enhanced medication
                    service.
                </li>
                <li>
                    <strong>Communications</strong>
                    - We added a communication module for Health Gateway
                    administrators, enabling the ability to add scheduled banner
                    posts to the landing page.
                </li>
                <li>
                    <strong>Profile Menu</strong>
                    - To maintain consistency for users using the menu, we added
                    links to the menu for users who are on the profile page.
                </li>
            </ul>
        </release-note>
        <release-note
            date="April 22, 2020"
            version="v1.1.7.1"
            title="Implemented fix for pagination error"
        >
            <ul>
                <li>
                    Fixed error associated with pagination counts which was
                    resulting in issues with loading of the application.
                </li>
            </ul>
        </release-note>
        <release-note
            date="April 15, 2020"
            version="v1.1.7"
            title="User Interface Enhancements, User Support"
        >
            <ul>
                <li>
                    <strong>User Interface Enhancements</strong>
                    - As new functionality is continually developed, the
                    application menu was revamped to allow for easy navigation,
                    with features accessible from the menu. We also added a
                    pagination feature for those with large numbers of records.
                </li>
                <li>
                    <strong>User Support</strong> - To better support our users,
                    we added an email management feature which will allow the
                    Health Gateway team to resend emails, such as Beta Invites
                    for wait listed users. The feedback form has been updated to
                    allow users to provide feedback and have their questions
                    answered. When messages are sent through this contact form,
                    the Health Gateway team will be able to respond via email.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 31, 2020"
            version="v1.1.6"
            title="Enhanced Medication Service, Filters and Printing"
        >
            <ul>
                <li>
                    <strong>Enhanced Medications</strong>
                    - We have end to end functionality on our new medication
                    service, with the same look and feel, but for most users,
                    the list of dispensed medications will be more comprehensive
                    than the current 14 month view. This feature will be
                    finalized and deployed in the next sprint or two.
                </li>
                <li>
                    <strong>New Feature: Timeline Filters </strong>- We built
                    filters for the health care timeline which allow you to
                    filter between Medications and Notes and any future data
                    sets.
                </li>
                <li>
                    <strong>Printing</strong> - User feedback suggested that a
                    print function would support consultations, as patients
                    often have bring notes on paper to review with their care
                    providers during their appointments. This is the first
                    iteration of the print feature and produces printout of what
                    is displayed on the timeline. Users can select filters and
                    then print as needed.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 17, 2020"
            version="v1.1.5"
            title="Patient Notes, Enhanced Medication Progress, Admin Dashboard Stats"
        >
            <ul>
                <li>
                    <strong>New Feature: Patient Notes</strong>
                    - Based on early feedback and interviews, users expressed a
                    common desire to be able to add their own notes. In this
                    sprint, we are releasing the first iteration of Patient
                    Notes, a simple free text entry into the Health Care
                    Timeline. Users can add, edit and delete notes and if they
                    close their accounts, the notes are deleted from the
                    application.
                </li>
                <li>
                    <strong>Enhanced Medications</strong>
                    - We did further development to enable viewing of the full
                    medication history, currently limited to 14 months. We
                    expect this enhanced medication list to be available in a
                    coming sprint.
                </li>
                <li>
                    <strong>Admin Dashboard Stats</strong>
                    - As more British Columbians register and start using the
                    Health Gateway, we want to monitor the stats so we can best
                    support the growing number of users.
                </li>
            </ul>
        </release-note>
        <release-note
            date="March 3, 2020"
            version="v1.1.4"
            title="Account Management"
        >
            <ul>
                <li>
                    <strong>Terms of Service:</strong>
                    We added a feature which will notify users via email when
                    the Terms of Service has been updated. Users who are logging
                    in for the first time since the Terms of Service have been
                    updated will see a banner notifying them of the update when
                    they login.
                </li>
                <li>
                    <strong>Account Closure:</strong>
                    We added a feature which allows users to close their Health
                    Gateway account from the profile page. If a user clicks on
                    the "Close My Account" link, their account will be
                    deactivated for 30 days, during which they can recover the
                    account. After 30 days has elapsed, the user's account and
                    personally entered information (once note feature is
                    implemented) will be removed permanently.
                </li>
                <li>
                    This release also includes minor fixes for the
                    Administration Module of the Health Gateway.
                </li>
            </ul>
            <p>
                We plan on developing a Patient Notes feature next, allowing
                users to add their own relevant health information. An updated
                Terms of Service will be released when the Patient Notes feature
                is ready. As mentioned above, Patient Notes will be deleted if a
                user chooses to close their account.
            </p>
        </release-note>
        <release-note
            date="February 19, 2020"
            version="v1.1.3"
            title="Immunization and Administration"
        >
            <ul>
                <li>Initial work on providing Immunization data.</li>
                <li>Administration module to manage client feedback.</li>
                <li>Administration module fixes.</li>
            </ul>
        </release-note>
        <release-note
            date="February 5, 2020"
            version="v1.1.2.5"
            title="Fixed Beta User Registration"
        >
            <ul>
                <li>Fixed registration error for beta users.</li>
            </ul>
        </release-note>
        <release-note
            date="February 4, 2020"
            version="v1.1.2.4"
            title="Queuing and Inviting More Users, Education Links and Info, Administration"
        >
            <ul>
                <li>
                    Users will receive a confirmation email after joining the
                    beta wait list.
                </li>
                <li>
                    Email removal feature is now more prominent in the user
                    profile.
                </li>
                <li>
                    HealthLink card directs users to HealthLink BC and 811
                    Service.
                </li>
                <li>
                    Administration module created to provide access to job
                    scheduler and manage the wait list.
                </li>
            </ul>
        </release-note>
        <release-note
            date="January 23, 2020"
            version="v1.1.2.1"
            title="Fixed Drug Loader Programs"
        >
            <ul>
                <li>
                    Implemented fixes for federal and provincial drug loader
                    programs.
                </li>
            </ul>
        </release-note>
        <release-note
            date="January 21, 2020"
            version="v1.1.1"
            title="Filter, User Profile, Beta User Queuing"
        >
            <ul>
                <li>
                    Added search filter to allow users to search for specific
                    medications and prescribing physicians.
                </li>
                <li>
                    Added user profile, where users can see their name and
                    change/remove their email.
                </li>
                <li>
                    Deployed beta user queuing, enabling those interested in
                    joining as early users to authenticate with the BC Services
                    Card and add their email to the wait list.
                </li>
            </ul>
        </release-note>
        <release-note
            date="January 13, 2020"
            version="v1.1.0"
            title="Minor fixes, enhancements and infrastructure upgrade"
        >
            <ul>
                <li>Added authentication error warning and redirect.</li>
                <li>
                    Added Health Gateway to Google Search Console for analytics.
                </li>
                <li>Upgraded build agents to .NET 3.1 from .NET 2.1.</li>
            </ul>
        </release-note>
        <release-note
            date="December 10, 2019"
            version="v1.0.1"
            title="Minor fix"
        >
            <ul>
                <li>
                    Email not verified bug fixed to disappear after email has
                    been verified.
                </li>
            </ul>
        </release-note>
        <release-note
            date="December 6, 2019"
            version="v1.0"
            title="Health Gateway Launch"
        >
            <ul>
                <li>
                    Launched minimum viable product, providing group of patient
                    partners with access to their medication dispense history
                    over the last 14 months.
                </li>
                <li>
                    BC Services Card used for sign-on, meeting the standard for
                    Level 3 Identity Assurance (In-person/Virtual Identity
                    Proofing and 2-Factor Authentication).
                </li>
            </ul>
        </release-note>
    </div>
</template>
