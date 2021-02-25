<template>
    <div class="m-3 px-4">
        <b-row>
            <b-col>
                <b-row class="my-3 fluid">
                    <b-col>
                        <div id="pageTitle">
                            <h1 id="subject">Frequently Asked Questions</h1>
                            <hr />
                        </div>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <b-row>
            <b-col>
                <b-row>
                    <b-col>
                        <b-button
                            v-b-toggle.faqAnswer1
                            variant="link"
                            class="text-left faq-question-title"
                            >Q: What health information is available in the
                            Health Gateway?</b-button
                        >
                        <b-collapse id="faqAnswer1" class="pl-3">
                            <div>
                                <p>
                                    A: The Health Gateway provides access to the
                                    following health information from our
                                    provincial health repositories:
                                </p>
                                <p class="pl-2">
                                    • <strong>Medications</strong>
                                </p>
                                <p class="pl-4">
                                    View your prescriptions from community
                                    dispensed in BC dating back to 1995.
                                </p>
                                <p class="pl-2">
                                    • <strong>Immunization records</strong>
                                </p>
                                <p class="pl-4">
                                    Access immunization records for vaccines
                                    administered by public health and
                                    pharmacies. The Health Gateway also provides
                                    the digital COVID-19 immunization card,
                                    which you can download and print as proof of
                                    your immunization. Immunization records for
                                    vaccines provided by family doctors or
                                    travel clinics are currently unavailable.
                                </p>
                                <p class="pl-2">
                                    • <strong>COVID-19 test results</strong>
                                </p>
                                <p class="pl-4">
                                    Get a text or email when your results are
                                    available. Access test results of children
                                    11 years and younger.
                                </p>
                                <p>
                                    Future data sets will include hospital and
                                    outpatient visits, laboratory test results,
                                    and diagnostic imaging reports.
                                </p>
                            </div>
                        </b-collapse>
                        <hr class="d-none d-sm-block desktop-separator" />
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <b-button
                            v-b-toggle.faqAnswer2
                            variant="link"
                            class="text-left faq-question-title"
                            >Q: Do I use the Health Gateway for COVID-19
                            immunization registration and booking?</b-button
                        >
                        <b-collapse id="faqAnswer2" class="pl-3">
                            <div>
                                <p>
                                    A: No. The Health Gateway provides a digital
                                    record of your COVID-19 immunization once
                                    you have received it, but it is not used for
                                    registration or booking. Visit the
                                    <a
                                        href="https://www2.gov.bc.ca/gov/content/safety/emergency-preparedness-response-recovery/covid-19-provincial-support/vaccines"
                                        target="blank_"
                                        >COVID-19 Immunization Plan</a
                                    >
                                    page for more information on the COVID-19
                                    immunization registration and booking
                                    process.
                                </p>
                            </div>
                        </b-collapse>
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
.faqQuestionTitle {
    padding: 0px;
    color: $primary;
}
.desktop-separator {
    border-top: 2px solid grey;
}
</style>
