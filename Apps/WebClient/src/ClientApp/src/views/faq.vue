<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import BreadcrumbComponent from "@/components/navmenu/breadcrumb.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";

@Component({
    components: {
        BreadcrumbComponent,
    },
})
export default class FaqView extends Vue {
    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "FAQ",
            to: "/faq",
            active: true,
        },
    ];
}
</script>
<template>
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <BreadcrumbComponent :items="breadcrumbItems" />
        <page-title title="Frequently Asked Questions" />
        <b-row class="mb-3 fluid text-right">
            <b-col>Last updated: <strong>2021-November-4</strong></b-col>
        </b-row>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer1
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >What health information is available in Health
                    Gateway?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer1" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                The Health Gateway provides access to the
                                following health information from the provincial
                                health repositories:
                            </p>
                            <ul>
                                <li>
                                    <strong>Medications</strong>
                                    <div>
                                        View your prescriptions from community
                                        pharmacies dispensed in BC dating back
                                        to 1995.
                                    </div>
                                </li>
                                <li>
                                    <strong>Immunization records</strong>
                                    <div>
                                        Access immunization records for vaccines
                                        administered by public health and
                                        pharmacies. This includes your BC
                                        Vaccine Card, federal vaccine proof, and
                                        your COVID-19 immunization records.
                                    </div>
                                </li>
                                <li>
                                    <strong>COVID-19 test results</strong>
                                    <div>
                                        Get a text or email when your results
                                        are available. Access test results of
                                        your children 11 years and younger.
                                    </div>
                                </li>
                                <li>
                                    <strong>Health Visits</strong>
                                    <div>
                                        View the last seven years of your health
                                        visits, consultations and procedures
                                        billed to the BC Medical Services Plan.
                                    </div>
                                </li>
                                <li>
                                    <strong
                                        >Special Authority Drug Requests</strong
                                    >
                                    <div>
                                        View the status of your
                                        <a
                                            href="https://www2.gov.bc.ca/gov/content/health/health-drug-coverage/pharmacare-for-bc-residents/what-we-cover/drug-coverage/drugs-requiring-pre-approval"
                                            target="blank_"
                                            >Special Authority</a
                                        >
                                        drug coverage requests made since March
                                        2021.
                                    </div>
                                </li>
                            </ul>
                            <p>
                                In the future, Health Gateway will include
                                health authority visits, laboratory test
                                results, and diagnostic imaging reports.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer2
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >I received one or more doses of the COVID-19 vaccine
                    outside of BC. How do I add my out-of-province dose(s) to
                    Health Gateway?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer2" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Health Gateway shows your vaccinations as
                                recorded in the Provincial Immunization
                                Registry. To get your out-of-province COVID-19
                                vaccines added to the Registry, complete the
                                online form at
                                <a
                                    href="https://immunizationrecord.gov.bc.ca"
                                    target="blank_"
                                    >immunizationrecord.gov.bc.ca</a
                                >.
                            </p>
                            <p>
                                If you would like to add other types of vaccine
                                records to your Health Gateway account, contact
                                your local public health unit.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse> </b-card
        ><b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer3
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >My COVID-19 vaccine record has an error, or my COVID-19
                    vaccine record is missing. What should I do?
                </b-button>
            </b-card-header>
            <b-collapse id="accordion-faqAnswer3" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                If you received your COVID-19 vaccine in BC,
                                your vaccine will usually appear in Health
                                Gateway within 48 hours.
                            </p>
                            <p>
                                Please complete the online form at
                                <a
                                    href="https://www.gov.bc.ca/vaccinerecord"
                                    target="blank_"
                                    >immunizationrecord.gov.bc.ca</a
                                >.
                            </p>
                            <p>
                                Note that any discrepancies in your immunization
                                provider, lot number or provider/clinic do not
                                affect the validity of your BC Vaccine Card or
                                federal proof.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer4
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >Why does my health visit record show a different
                    practitioner or clinic address than the one I went
                    to?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer4" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                The Health Gateway shows your health visits
                                billed to the BC Medical Services Plan (MSP).
                                For administrative purposes, in some cases the
                                visit may have been billed to a different
                                practitioner or the address may be the financial
                                office of a healthcare facility.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer5
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >I see immunizations that I received in the pharmacy, like
                    flu shots, but I don’t see immunizations from my family
                    doctor or travel clinic. Why is that?
                </b-button>
            </b-card-header>
            <b-collapse id="accordion-faqAnswer5" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                The Health Gateway provides information from the
                                Provincial Immunization Registry, which contains
                                most immunizations administered by public health
                                and pharmacies in BC. However, immunizations
                                administered by family practice or travel
                                clinics are not recorded in the Registry.
                                Immunizations administered by a pharmacy may be
                                displayed as a dispensed medication rather than
                                an immunization. If you can’t find an
                                immunization record, try searching your
                                dispensed medications.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer6
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >What is the difference between the Health Gateway and other
                    portals, such as My Health?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer6" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Health Gateway is a provincial service that
                                provides British Columbians with access to their
                                health information, regardless of where they
                                received care or services.
                            </p>
                            <p>
                                My Health and other regional portals provide
                                access to information within the regional
                                systems. We are collaborating with our health
                                sector partners to ensure we are providing
                                complementary services.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer7
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >I manage my elderly parent’s health care. Can I access
                    their information through the Health Gateway?
                </b-button>
            </b-card-header>
            <b-collapse id="accordion-faqAnswer7" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Currently, only parents or guardians of children
                                11 years or younger can access their children’s
                                COVID-19 test results. We are working to enable
                                family and caregiver access in the future.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer8
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >Why can’t I see all my Special Authority requests in the
                    Health Gateway?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer8" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Special Authority requests that your prescriber
                                has made since March 2021 are being added
                                gradually to the Health Gateway. The Special
                                Authority team is currently experiencing a high
                                volume of requests. Expected turnaround is 1
                                business day for urgent requests and 12-16 weeks
                                for regular requests. Visit the
                                <a
                                    href="https://www2.gov.bc.ca/gov/content/health/practitioner-professional-resources/pharmacare/prescribers/special-authority "
                                    target="blank_"
                                    >Special Authority site</a
                                >. for more information. If you have any
                                questions, talk to your prescriber or
                                pharmacist, or call 1-800-663-7100 (toll-free)
                                or 604-683-7151 (from the Lower Mainland).
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer9
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >How do I sign up to get a text message when my (or my
                    child’s) COVID-19 test results are available?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer9" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                To get a text message when your COVID-19 test
                                results are available, you must add and verify
                                your mobile phone number in your Health Gateway
                                account. Most people do this step when they
                                register for their Health Gateway account. To
                                check if you have a verified mobile phone
                                number, go to your Profile. You only need to do
                                this once.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer10
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >I’m having trouble adding my child/dependent to my Health
                    Gateway account. What should I do?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer10" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                This service requires that your dependent have a
                                BC Services Card (not a Care Card). To add a
                                dependent, type their full name exactly as it
                                appears on their BC Services Card, including any
                                middle names. If that doesn’t work, try other
                                slight variations of the name (e.g., put the
                                name in all capital letters, remove any hyphens,
                                or try different combinations of the given and
                                last names).
                            </p>
                            <p>
                                If you are still unable to add your dependent,
                                email us at
                                <a href="mailto:HealthGateway@gov.bc.ca"
                                    >HealthGateway@gov.bc.ca</a
                                >.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer11
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >How frequently do you add new features?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer11" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                The Health Gateway is updated weekly with new
                                features and information. We prioritize our
                                enhancements based on your feedback, and we
                                encourage you to provide suggestions by
                                e-mailing
                                <a href="mailto:HealthGateway@gov.bc.ca"
                                    >HealthGateway@gov.bc.ca</a
                                >.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.faq-header {
    button {
        background-color: $soft_background !important;
        border-color: $soft_background !important;
        color: $primary;
        text-decoration-line: underline;
        font-weight: bold;
    }
}
</style>
