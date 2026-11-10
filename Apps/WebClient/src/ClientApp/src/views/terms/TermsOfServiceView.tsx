import { useEffect, useState } from "react";
import { View } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { Loading } from "@/components/site/Loading";
import { HgAlert, PageTitle } from "@/components/ui";
import { Path } from "@/constants/path";
import { isSessionEnded } from "@/models/errors";
import { services } from "@/services/container";
import { logger } from "@/services/logger";

import { TermsHtml } from "./TermsHtml";

/** Port of TermsOfServiceView.vue — loads UserProfile/termsofservice. */
export function TermsOfServiceView() {
    const [html, setHtml] = useState<string>();
    const [error, setError] = useState(false);

    useEffect(() => {
        services.userProfile
            .getTermsOfService()
            .then((t) => setHtml(t.content))
            .catch((e) => {
                if (isSessionEnded(e)) return;
                logger.error("Unable to load terms of service", e);
                setError(true);
            });
    }, []);

    return (
        <View className="flex-1">
            <Breadcrumb
                items={[
                    {
                        text: "Terms of Service",
                        to: Path.TermsOfService,
                        active: true,
                        testId: "breadcrumb-terms-of-service",
                    },
                ]}
            />
            <PageTitle title="Terms of Service" />
            <Loading isLoading={html === undefined && !error} />
            {error ? (
                <HgAlert
                    type="error"
                    live="assertive"
                    text="Unable to load the Terms of Service. Please try again later."
                />
            ) : null}
            {html !== undefined ? <TermsHtml html={html} /> : null}
        </View>
    );
}
