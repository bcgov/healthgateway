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
        <b-card no-body class="mb-1 border-0">
            <b-card-header
                header-tag="header"
                class="p-1 faq-header"
                role="tab"
            >
                <b-button
                    v-b-toggle.accordion-faqAnswer1
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
                        ><div>
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
                                        vaccines vaccines administered by health
                                        and pharmacies. The Health Health Health
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
                        ><div>
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
