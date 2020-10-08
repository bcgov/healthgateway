<template>
    <div class="px-4">
        <b-row>
            <b-col>
                <b-row class="my-3 fluid">
                    <b-col>
                        <div id="pageTitle">
                            <h1 id="subject">Health Gateway Release Notes</h1>
                            <hr />
                        </div>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >October 7, 2020 - v1.2.8 - Unsupported
                                    Browsers, Filter Counts and Pagination, New
                                    Functional Test Tooling, Security
                                    Enhancements
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We added a message for users trying to
                                        use unsupported browsers recommending
                                        use of modern web browsers.
                                    </li>
                                    <li>
                                        We added counts to filter types and an
                                        ability to change the number of records
                                        per page.
                                    </li>
                                    <li>
                                        We adopted a new functional testing tool
                                        which improves our automated testing of
                                        all Health Gateway features.
                                    </li>
                                    <li>
                                        We remediated low severity security
                                        issues as identified in a recent
                                        vulnerability scan of our admin
                                        application.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >September 28, 2020 - v1.2.7.3 - Bug Fix for
                                    Record Details
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We fixed a bug which was causing record
                                        details to collapse when trying to
                                        expand.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >September 25, 2020 - v1.2.7.2 - Medication
                                    History Report Bug Fix
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We fixed a bug causing reports with
                                        large numbers of medication records to
                                        generate blank pdf reports.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >September 23, 2020 - v1.2.7 - Advance
                                    Filter, Footer Menu, Medication History
                                    Report
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We added a filter menu which expands and
                                        collapses, to free up space for content.
                                    </li>
                                    <li>
                                        The links in the footer menu were
                                        updated, making it easier to find more
                                        information about the Health Gateway,
                                        such as release notes, contact
                                        information, terms of service and more.
                                    </li>
                                    <li>
                                        We implemented the first iteration of
                                        our Reports section which will allow
                                        users to save pdf reports of their
                                        health records, starting with medication
                                        history.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >September 10, 2020 - v1.2.6 - Menu
                                    Enhancements, MSP and Immunization
                                    Records</strong
                                >
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We implemented minor enhancements to the
                                        menu buttons and mobile menu.
                                    </li>
                                    <li>
                                        This sprint included integration work
                                        with Immunization and Medical Service
                                        Plan billing systems. Data is now
                                        displayed in the Health Gateway
                                        development environment and final
                                        testing and validation will take place
                                        prior to production launch.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >August 27, 2020 - v1.2.5.3 - Fixed Title
                                    for COVID Lab Result, Mobile Menu
                                    Close</strong
                                >
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        This was a minor fix to the COVID-19 Lab
                                        Result timeline entry, to show a more
                                        descriptive title, rather than the type
                                        of test administered.
                                    </li>
                                    <li>
                                        The mobile menu was updated to show an
                                        "X" to close the menu.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >August 26, 2020 - v1.2.5 - COVID-19 Test
                                    Results, Calendar Enhancements, Application
                                    Star Rating, Health Insights</strong
                                >
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        To support the COVID-19 response, the
                                        Health Gateway will now provide access
                                        to COVID-19 test results for BC
                                        residents. This feature includes a
                                        notification via SMS/Email that the
                                        COVID-19 result is available.
                                    </li>
                                    <li>
                                        We enhanced the date picker for the
                                        calendar to be easier to use.
                                    </li>
                                    <li>
                                        Upon logout, users will now be asked to
                                        provide a star rating.
                                    </li>
                                    <li>
                                        We implemented the first iteration for
                                        our Health Insights page to support
                                        users in understanding patterns in their
                                        health care.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >August 13, 2020 - v1.2.4.2 - Hotfix to add
                                    Google Verification for Search Console
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >August 12, 2020 - v1.2.4 - Landing Page,
                                    Troubleshooting, Analytics
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        The landing page was updated to provide
                                        some clarity around available data in
                                        the Health Gateway and goals to be
                                        realized through future features.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >July 29, 2020 - v1.2.3 - Calendar
                                    Navigation, Email Communications,
                                    Decommission HNClient.
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We implemented features that now allow
                                        users to navigate within the calendar
                                        month view by adding a date picker. Note
                                        that only months and years with health
                                        records can be selected, enabling faster
                                        discovery of health records. Also, when
                                        a user toggles between month and list
                                        view, the date context remains. That is,
                                        a user can flip back and forth between
                                        the two views while staying on the same
                                        relative date range.
                                    </li>
                                    <li>
                                        The Health Gateway team can now send
                                        communication emails to users.
                                    </li>
                                    <li>
                                        The remaining code for HNClient, an
                                        application used for the previous
                                        dispensed medication integration has now
                                        been removed. Any associated technical
                                        debt is no longer affecting Health
                                        Gateway.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >July 15, 2020 - v1.2.2 - Navigation,
                                    Usability and Token Storage
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        As the Health Gateway could potentially
                                        display numerous records for users, we
                                        improved the navigation from the month
                                        calendar. You can now click on the icon
                                        on a specific date and be taken to the
                                        list view for details.
                                    </li>
                                    <li>
                                        The sidebar menu has been updated to
                                        show sub-menu items to minimize clutter
                                        and only show buttons in the context of
                                        the page they are relevant to (Timeline
                                        and Profile).
                                    </li>
                                    <li>
                                        The "Notes" feature of the site will
                                        show a popover with a brief description
                                        of what the feature can be used for.
                                    </li>
                                    <li>
                                        The loading spinner was enhanced to
                                        convey to the user that records are
                                        being fetched.
                                    </li>
                                    <li>
                                        Finally, token storage was enhanced to
                                        allow for larger access tokens in cookie
                                        storage.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >July 6, 2020 - v1.2.1.1 - Fixed Protective
                                    Word
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We implemented a fix to allow user to
                                        enter protective word correctly.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >July 3, 2020 - v1.2.1 - Banner Posts, UI
                                    Improvements
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We implemented a feature which will
                                        allow us to post banner messages to be
                                        displayed on the landing page, such as
                                        important updates.
                                    </li>
                                    <li>
                                        Improvements to the user interface
                                        include updates to the timeline cards
                                        and renaming the "Notes" filter to "My
                                        Notes" for clarity.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >June 26, 2020 - v1.2.0.2 - Fixed Brand Name
                                    Look-up
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        This hot-fix implemented improvements to
                                        the medication query, to address lengthy
                                        loading times and occasional timeouts.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >June 23, 2020 - v1.2.0.1 - Enhanced Logging
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        We improved our logs to assist with
                                        debugging.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >May 4, 2020 - v1.1.8.1 - Launched Enhanced
                                    Medications
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        <strong
                                            >Full History of Medications</strong
                                        >
                                        - Users of the Health Gateway can now
                                        see the full history of medications in
                                        the PharmaNet system, as far back as
                                        1996. Due to system limitations, the
                                        Health Gateway returns up to 20,000
                                        dispensed medication records.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >April 29, 2020 - v1.1.8 - Security,
                                    Communications and Profile Menu
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        <strong>Security</strong>
                                        - To close out our final sprint for
                                        Release 1.1, the Health Gateway team
                                        ensured compliance with government
                                        security standards as we transition to
                                        our enhanced medication service.
                                    </li>
                                    <li>
                                        <strong>Communications</strong>
                                        - We added a communication module for
                                        Health Gateway administrators, enabling
                                        the ability to add scheduled banner
                                        posts to the landing page.
                                    </li>
                                    <li>
                                        <strong>Profile Menu</strong>
                                        - To maintain consistency for users
                                        using the menu, we added links to the
                                        menu for users who are on the profile
                                        page.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >April 22, 2020 - v1.1.7.1 - Implemented fix
                                    for pagination error
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Fixed error associated with pagination
                                        counts which was resulting in issues
                                        with loading of the application.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >April 15, 2020 - v1.1.7 - User Interface
                                    Enhancements, User Support
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        <strong
                                            >User Interface Enhancements</strong
                                        >
                                        - As new functionality is continually
                                        developed, the application menu was
                                        revamped to allow for easy navigation,
                                        with features accessible from the menu.
                                        We also added a pagination feature for
                                        those with large numbers of records.
                                    </li>
                                    <li>
                                        <strong>User Support</strong> - To
                                        better support our users, we added an
                                        email management feature which will
                                        allow the Health Gateway team to resend
                                        emails, such as Beta Invites for wait
                                        listed users. The feedback form has been
                                        updated to allow users to provide
                                        feedback and have their questions
                                        answered. When messages are sent through
                                        this contact form, the Health Gateway
                                        team will be able to respond via email.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >March 31, 2020 - v1.1.6 - Enhanced
                                    Medication Service, Filters and Printing
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        <strong>Enhanced Medications</strong> -
                                        We have end to end functionality on our
                                        new medication service, with the same
                                        look and feel, but for most users, the
                                        list of dispensed medications will be
                                        more comprehensive than the current 14
                                        month view. This feature will be
                                        finalized and deployed in the next
                                        sprint or two.
                                    </li>
                                    <li>
                                        <strong
                                            >New Feature: Timeline Filters </strong
                                        >- We built filters for the health care
                                        timeline which allow you to filter
                                        between Medications and Notes and any
                                        future data sets.
                                    </li>
                                    <li>
                                        <strong>Printing</strong> - User
                                        feedback suggested that a print function
                                        would support consultations, as patients
                                        often have bring notes on paper to
                                        review with their care providers during
                                        their appointments. This is the first
                                        iteration of the print feature and
                                        produces printout of what is displayed
                                        on the timeline. Users can select
                                        filters and then print as needed.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >March 17, 2020 - v1.1.5 - Patient Notes,
                                    Enhanced Medication Progress, Admin
                                    Dashboard Stats
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        <strong
                                            >New Feature: Patient Notes</strong
                                        >
                                        - Based on early feedback and
                                        interviews, users expressed a common
                                        desire to be able to add their own
                                        notes. In this sprint, we are releasing
                                        the first iteration of Patient Notes, a
                                        simple free text entry into the Health
                                        Care Timeline. Users can add, edit and
                                        delete notes and if they close their
                                        accounts, the notes are deleted from the
                                        application.
                                    </li>
                                    <li>
                                        <strong>Enhanced Medications</strong> -
                                        We did further development to enable
                                        viewing of the full medication history,
                                        currently limited to 14 months. We
                                        expect this enhanced medication list to
                                        be available in a coming sprint.
                                    </li>
                                    <li>
                                        <strong>Admin Dashboard Stats</strong> -
                                        As more British Columbians register and
                                        start using the Health Gateway, we want
                                        to monitor the stats so we can best
                                        support the growing number of users.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >March 3, 2020 - v1.1.4 - Account Management
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        <strong>Terms of Service:</strong> We
                                        added a feature which will notify users
                                        via email when the Terms of Service has
                                        been updated. Users who are logging in
                                        for the first time since the Terms of
                                        Service have been updated will see a
                                        banner notifying them of the update when
                                        they login.
                                    </li>
                                    <li>
                                        <strong>Account Closure:</strong> We
                                        added a feature which allows users to
                                        close their Health Gateway account from
                                        the profile page. If a user clicks on
                                        the "Close My Account" link, their
                                        account will be deactivated for 30 days,
                                        during which they can recover the
                                        account. After 30 days has elapsed, the
                                        user's account and personally entered
                                        information (once note feature is
                                        implemented) will be removed
                                        permanently.
                                    </li>
                                    <li>
                                        This release also includes minor fixes
                                        for the Administration Module of the
                                        Health Gateway.
                                    </li>
                                </ul>
                                We plan on developing a Patient Notes feature
                                next, allowing users to add their own relevant
                                health information. An updated Terms of Service
                                will be released when the Patient Notes feature
                                is ready. As mentioned above, Patient Notes will
                                be deleted if a user chooses to close their
                                account.
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >February 19, 2020 - v1.1.3 - Immunization
                                    and Administration
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Initial work on providing Immunization
                                        data.
                                    </li>
                                    <li>
                                        Administration module to manage client
                                        feedback.
                                    </li>
                                    <li>Administration module fixes.</li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >February 5, 2020 - v1.1.2.5 - Fixed Beta
                                    User Registration
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Fixed registration error for beta users.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >February 4, 2020 - v1.1.2.4 - Queuing and
                                    Inviting More Users, Education Links and
                                    Info, Administration
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Users will receive a confirmation email
                                        after joining the beta wait list.
                                    </li>
                                    <li>
                                        Email removal feature is now more
                                        prominent in the user profile.
                                    </li>
                                    <li>
                                        HealthLink card directs users to
                                        HealthLink BC and 811 Service.
                                    </li>
                                    <li>
                                        Administration module created to provide
                                        access to job scheduler and manage the
                                        wait list.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >January 23, 2020 - v1.1.2.1 - Fixed Drug
                                    Loader Programs
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Implemented fixes for federal and
                                        provincial drug loader programs.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >January 21, 2020 – v1.1.1 – Filter, User
                                    Profile, Beta User Queuing
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Added search filter to allow users to
                                        search for specific medications and
                                        prescribing physicians.
                                    </li>
                                    <li>
                                        Added user profile, where users can see
                                        their name and change/remove their
                                        email.
                                    </li>
                                    <li>
                                        Deployed beta user queuing, enabling
                                        those interested in joining as early
                                        users to authenticate with the BC
                                        Services Card and add their email to the
                                        wait list.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >January 13, 2020 – v1.1.0 – Minor fixes,
                                    enhancements and infrastructure upgrade
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Added authentication error warning and
                                        redirect.
                                    </li>
                                    <li>
                                        Added Health Gateway to Google Search
                                        Console for analytics.
                                    </li>
                                    <li>
                                        Upgraded build agents to .NET 3.1 from
                                        .NET 2.1.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >December 10, 2019 - v1.0.1 – Minor fix
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Email not verified bug fixed to
                                        disappear after email has been verified.
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                        <b-row class="mb-2">
                            <b-col>
                                <strong
                                    >December 6, 2019 - v1.0 – Health Gateway
                                    Launch
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <ul>
                                    <li>
                                        Launched minimum viable product,
                                        providing group of patient partners with
                                        access to their medication dispense
                                        history over the last 14 months.
                                    </li>
                                    <li>
                                        BC Services Card used for sign-on,
                                        meeting the standard for Level 3
                                        Identity Assurance (In-person/Virtual
                                        Identity Proofing and 2-Factor
                                        Authentication).
                                    </li>
                                </ul>
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
#pageTitle {
    color: $primary;
}
#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
