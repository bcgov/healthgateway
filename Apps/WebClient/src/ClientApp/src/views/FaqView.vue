<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
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
            dataTestId: "breadcrumb-faq",
        },
    ];
}
</script>
<template>
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <BreadcrumbComponent :items="breadcrumbItems" />
        <page-title title="Frequently Asked Questions" />
        <b-row class="mb-1 fluid text-right">
            <b-col>Last updated: <strong>2022-May-24</strong></b-col>
        </b-row>
        <b-row class="m-0 pt-0"><h3>Lab test results</h3></b-row>
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
                    >I have a question about my lab test. How can I find out
                    more?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer1" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                You can learn more about specific lab tests on
                                the following trusted websites:
                            </p>
                            <ul>
                                <li>
                                    <div>
                                        HealthLink BC -
                                        <a
                                            href="https://www.healthlinkbc.ca/tests-treatments-medications/medical-tests"
                                            target="_blank"
                                            rel="noopener"
                                            >https://www.healthlinkbc.ca/tests-treatments-medications/medical-tests</a
                                        >
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        Lab Tests Online -
                                        <a
                                            href="https://labtestsonline.org/"
                                            target="_blank"
                                            rel="noopener"
                                            >https://labtestsonline.org/</a
                                        >
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        Mayo Clinic Laboratories -
                                        <a
                                            href="https://www.mayocliniclabs.com/"
                                            target="_blank"
                                            rel="noopener"
                                            >https://www.mayocliniclabs.com/</a
                                        >
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        ARUP Laboratories -
                                        <a
                                            href="https://www.aruplab.com/"
                                            target="_blank"
                                            rel="noopener"
                                            >https://www.aruplab.com/</a
                                        >
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        For pathology tests (like a biopsy) -
                                        <a
                                            href="https://www.mypathologyreport.ca/"
                                            target="_blank"
                                            rel="noopener"
                                            >https://www.mypathologyreport.ca/</a
                                        >
                                    </div>
                                </li>
                            </ul>
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
                    >I’m concerned about my lab test results. What should I
                    do?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer2" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Follow the instructions of your health care
                                provider regarding your lab test results. Keep
                                in mind that laboratory tests provide a partial
                                picture of your health. To interpret these
                                results, your health care provider must combine
                                these test results with your medical history,
                                current symptoms, or other diagnostic tests to
                                arrive at a conclusion.
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
                    >I can’t see my lab test results yet. Why not?
                </b-button>
            </b-card-header>
            <b-collapse id="accordion-faqAnswer3" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Most lab results are available about 2-3 days
                                after your test. Pathology tests, like a tissue
                                biopsy, are available about 7-10 days after your
                                test. This ensures that your family doctor or
                                care provider has time to review and interpret
                                your results.
                            </p>
                            <p>
                                You can learn more about how to understand your
                                pathology report at
                                <a
                                    href="https://www.mypathologyreport.ca/"
                                    target="_blank"
                                    rel="noopener"
                                    >https://www.mypathologyreport.ca/</a
                                >
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
                    >My lab test result is “out of range.” What does that
                    mean?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer4" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Many laboratory tests include a reference range
                                to help classify results as “in range” or “out
                                of range.” Remember:
                            </p>
                            <ul>
                                <li>
                                    <div>
                                        Ranges are different between
                                        laboratories
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        “Out of range” results may be normal for
                                        you
                                    </div>
                                </li>
                            </ul>
                            <p>
                                You should discuss your specific results with
                                your health care provider. They will review your
                                results in the context of your medical history,
                                symptoms, and other diagnostic tests.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-row class="m-0 pt-3"><H3>COVID-19 information</H3></b-row>
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
                    >When can I get my COVID-19 booster dose?
                </b-button>
            </b-card-header>
            <b-collapse id="accordion-faqAnswer5" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                For information on booster doses and who is
                                eligible to receive a second booster dose,
                                please visit
                                <a
                                    href="https://www.gov.bc.ca/booster"
                                    target="_blank"
                                    rel="noopener"
                                    >www.gov.bc.ca/booster</a
                                >
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
                    >I got the COVID-19 vaccine outside of BC. How do I add it
                    to Health Gateway?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer6" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Complete the online form at:
                                <a
                                    href="https://www.immunizationrecord.gov.bc.ca"
                                    target="_blank"
                                    rel="noopener"
                                    >www.immunizationrecord.gov.bc.ca</a
                                >
                            </p>
                            <p>
                                If you would like to add other non-COVID
                                vaccination records to Health Gateway, contact
                                your local public health unit. You can find one
                                near you at:
                                <a
                                    href="https://www.immunizebc.ca/finder"
                                    target="_blank"
                                    rel="noopener"
                                    >www.immunizebc.ca/finder</a
                                >
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
                    >My COVID-19 vaccination record has an error. What should I
                    do?
                </b-button>
            </b-card-header>
            <b-collapse id="accordion-faqAnswer7" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Complete the online form at:
                                <a
                                    href="https://www.immunizationrecord.gov.bc.ca"
                                    target="_blank"
                                    rel="noopener"
                                    >www.immunizationrecord.gov.bc.ca</a
                                >
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-row class="m-0 pt-3"><H3>Other health records</H3></b-row>
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
                    >I see immunizations that I received in the pharmacy, like
                    flu shots, but I don’t see immunizations from my family
                    doctor or travel clinic. Why?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer8" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Health Gateway only shows your immunization
                                records from the Provincial Immunization
                                Registry.
                            </p>
                            <ul>
                                <li>
                                    <div>
                                        This includes immunizations from public
                                        health clinics or pharmacies in B.C.
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        It <strong>does not</strong> include
                                        immunizations from family practices or
                                        travel clinics
                                    </div>
                                </li>
                            </ul>
                            <p>
                                Immunizations you got in a pharmacy may be
                                displayed as a medication rather than an
                                immunization. If you can’t find an immunization
                                record, try searching your medications.
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
                    >Why does my health visit show a different practitioner or
                    clinic address than the one I went to?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer9" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Health Gateway shows your health visits billed
                                to the BC Medical Services Plan (MSP). Sometimes
                                different information for a health care provider
                                is shown because of the way billing is done. For
                                example:
                            </p>
                            <ul>
                                <li>
                                    <div>
                                        Health visits are billed to a different
                                        health care provider
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        The billing address is used instead of
                                        the clinic address
                                    </div>
                                </li>
                            </ul>
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
                    >Why can’t I see all my Special Authority requests in Health
                    Gateway?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer10" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Current processing times for
                                <a
                                    href="https://www2.gov.bc.ca/gov/content/health/practitioner-professional-resources/pharmacare/prescribers/special-authority "
                                    target="_blank"
                                    rel="noopener"
                                    >Special Authority</a
                                >
                                requests:
                            </p>
                            <ul>
                                <li>
                                    <div>
                                        Urgent requests – about 1 business day
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        Regular requests – about 12 to 16 weeks
                                    </div>
                                </li>
                            </ul>
                            <p>If you have any questions, you can:</p>
                            <ul>
                                <li>
                                    <div>
                                        Talk to your prescriber or pharmacist
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        Call 1-800-663-7100 (toll-free) or
                                        604-683-7151 (from the Lower Mainland)
                                    </div>
                                </li>
                            </ul>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-row class="m-0 pt-3"><H3>Adding a dependent</H3></b-row>
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
                    >I’m having trouble adding my child/dependent to my Health
                    Gateway account. What should I do?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer11" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                To add your child to your Health Gateway
                                account, make sure:
                            </p>
                            <ul>
                                <li>
                                    <div>
                                        You’re using your child’s BC Services
                                        Card, not their CareCard
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        You’ve typed their full name exactly as
                                        it appears on their BC Services Card,
                                        including any middle names. Also try
                                        entering their full name using all
                                        capital letters
                                    </div>
                                </li>
                                <li>
                                    <div>
                                        Your mobile device is not adding extra
                                        spaces when completing the online form.
                                        You could also try using a laptop or
                                        desktop computer instead
                                    </div>
                                </li>
                            </ul>
                            <p>
                                If you are still unable to add your child, email
                                us at
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
                    v-b-toggle.accordion-faqAnswer12
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >I manage my elderly parents’ health care. Can I access
                    their information in Health Gateway?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer12" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Currently, only parents or guardians can access
                                COVID-19 test results for their children that
                                are 11 years old and younger. We’re working to
                                provide more family and caregiver access in the
                                future.
                            </p>
                        </div></b-card-text
                    >
                </b-card-body>
            </b-collapse>
        </b-card>
        <b-row class="m-0 pt-3"><H3>Learn more about Health Gateway</H3></b-row>
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer13
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >What is the difference between Health Gateway and other
                    health portals, such as My Health?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer13" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Health Gateway is a provincial tool that
                                provides British Columbians with access to their
                                health information, regardless of where they
                                received care or services.
                            </p>
                            <p>
                                My Health and other portals provide access to
                                health information within each region.
                                <a
                                    href="https://www.healthlinkbc.ca/more/health-features/your-health-information"
                                    target="_blank"
                                    rel="noopener"
                                    >Learn more</a
                                >
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
                    v-b-toggle.accordion-faqAnswer14
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >How frequently do you add new features?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer14" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                Health Gateway is updated weekly with new
                                features and information. We prioritize changes
                                based on your feedback. Send us your
                                suggestions:
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
