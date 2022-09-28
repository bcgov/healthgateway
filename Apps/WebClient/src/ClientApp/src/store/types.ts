import { ActionContext, StoreOptions } from "vuex";

import { AuthModule } from "./modules/auth/types";
import { ClinicalDocumentModule } from "./modules/clinicalDocument/types";
import { CommentModule } from "./modules/comment/types";
import { ConfigModule } from "./modules/config/types";
import { EncounterModule } from "./modules/encounter/types";
import { ErrorBannerModule } from "./modules/error/types";
import { IdleModule } from "./modules/idle/types";
import { ImmunizationModule } from "./modules/immunization/types";
import { LaboratoryModule } from "./modules/laboratory/types";
import { MedicationModule } from "./modules/medication/types";
import { NavbarModule } from "./modules/navbar/types";
import { NoteModule } from "./modules/note/types";
import { TimelineModule } from "./modules/timeline/types";
import { UserModule } from "./modules/user/types";
import { VaccinationStatusModule } from "./modules/vaccinationStatus/types";

export interface RootState {
    version: string;
    isMobile: boolean;
}

export interface GatewayStoreOptions extends StoreOptions<RootState> {
    actions: {
        setIsMobile(
            context: ActionContext<RootState, RootState>,
            isMobile: boolean
        ): void;
    };
    getters: {
        isMobile: (state: RootState) => boolean;
    };
    mutations: {
        setIsMobile(state: RootState, isMobile: boolean): void;
    };
    modules: {
        auth: AuthModule;
        config: ConfigModule;
        user: UserModule;
        medication: MedicationModule;
        laboratory: LaboratoryModule;
        comment: CommentModule;
        immunization: ImmunizationModule;
        encounter: EncounterModule;
        clinicalDocument: ClinicalDocumentModule;
        note: NoteModule;
        navbar: NavbarModule;
        idle: IdleModule;
        errorBanner: ErrorBannerModule;
        timeline: TimelineModule;
        vaccinationStatus: VaccinationStatusModule;
    };
}
