<template>
    <div class="mx-sm-3 px-sm-4">
        <b-row class="m-1 fluid">
            <b-col>
                <div id="pageTitle">
                    <h1 id="subject">Frequently Asked Questions</h1>
                    <hr />
                </div>
            </b-col>
        </b-row>
        <b-row class="m-1 fluid text-right">
            <b-col> Last updated: <strong>March 3, 2021</strong> </b-col>
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
                    >Q: What health information is available in the Health
                    Gateway?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer1" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                A: The Health Gateway provides access to the
                                following health information from our provincial
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
                                        administered by public health
                                        and pharmacies. The Health
                                        Gateway also provides digital the
                                        digital COVID-19 immunization card,
                                        which you can which you can download and
                                        print as proof of your immunization.
                                        Immunization records for vaccines
                                        vaccines provided by family doctors or
                                        travel clinics are currently
                                        unavailable.
                                    </div>
                                </li>
                                <li>
                                    <strong>COVID-19 test results</strong>
                                    <div>
                                        Get a text or email when your results
                                        are available. Access test results of
                                        children 11 years and younger.
                                    </div>
                                </li>
                            </ul>
                            <p>
                                Future data sets will include hospital and
                                outpatient visits, laboratory test results, and
                                diagnostic imaging reports.
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
                    >Q: Do I use the Health Gateway for COVID-19 immunization
                    registration and booking?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer2" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                A: No. The Health Gateway provides a digital
                                record of your COVID-19 immunization once you
                                have received it, but it is not used for
                                registration or booking. Visit the
                                <a
                                    href="https://www2.gov.bc.ca/gov/content/safety/emergency-preparedness-response-recovery/covid-19-provincial-support/vaccines"
                                    target="blank_"
                                    >COVID-19 Immunization Plan</a
                                >
                                page for more information on the COVID-19
                                immunization registration and booking process.
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
                    v-b-toggle.accordion-faqAnswer3
                    data-testid="questionBtn"
                    block
                    variant="info"
                    class="text-left"
                    >Q: I see immunizations that I received in the pharmacy,
                    like flu shots, but I don’t see immunizations from my family
                    doctor or travel clinic. Why is that?
                </b-button>
            </b-card-header>
            <b-collapse id="accordion-faqAnswer3" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                A: The Health Gateway provides information from
                                the Provincial Immunization Registry, which
                                contains most immunizations administered by
                                public health and pharmacies in BC. However,
                                immunizations administered by family practice or
                                travel clinics are not recorded in the Registry.
                                Note that immunizations administered by a
                                pharmacy may be displayed in your profile as a
                                dispensed medication rather than an immunization
                                , but these will be added to the Registry in the
                                future.
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
                    >Q: What is the difference between the Health Gateway and
                    other portals, such as My Health?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer4" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                A: The Health Gateway is a provincial service
                                which provides British Columbians with access to
                                information, regardless of where they received
                                care or services.
                            </p>
                            <p>
                                My Health and other regional portals provide
                                access to information within the regional
                                systems. We are collaborating with our health
                                sector partners to ensure we are providing
                                complimentary services.
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
                    >Q: I manage my elderly parent’s health care. Can I access
                    their information through the Health Gateway?
                </b-button>
            </b-card-header>
            <b-collapse id="accordion-faqAnswer5" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                The Health Gateway currently provides guardians
                                with access to their children’s COVID-19 test
                                results. We are working to enable family and
                                caregiver access in the future.
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
                    >Q: How frequently do you add new features?</b-button
                >
            </b-card-header>
            <b-collapse id="accordion-faqAnswer6" role="tabpanel">
                <b-card-body>
                    <b-card-text
                        ><div data-testid="answerTxt">
                            <p>
                                The Health Gateway is updated weekly with new
                                features and information. We prioritize our
                                enhancements based on your feedback, and we
                                encourage you to provide suggestions by
                                e-mailing HealthGateway@gov.bc.ca
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
#pageTitle {
    color: $primary;
}
#pageTitle hr {
    border-top: 2px solid $primary;
}
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
