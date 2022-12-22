import { ActionContext, StoreOptions } from "vuex";

import { AppErrorType } from "@/constants/errorType";

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
import { NotificationModule } from "./modules/notification/types";
import { TimelineModule } from "./modules/timeline/types";
import { UserModule } from "./modules/user/types";
import { VaccinationStatusModule } from "./modules/vaccinationStatus/types";
import { WaitlistModule } from "./modules/waitlist/types";

export interface RootState {
    appError?: AppErrorType;
    isMobile: boolean;
    version: string;
}

export interface GatewayStoreOptions extends StoreOptions<RootState> {
    actions: {
        setAppError(
            context: ActionContext<RootState, RootState>,
            appError: AppErrorType
        ): void;
        setIsMobile(
            context: ActionContext<RootState, RootState>,
            isMobile: boolean
        ): void;
    };
    getters: {
        appError: (state: RootState) => AppErrorType | undefined;
        isMobile: (state: RootState) => boolean;
    };
    mutations: {
        setAppError(state: RootState, appError: AppErrorType): void;
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
        notification: NotificationModule;
        navbar: NavbarModule;
        idle: IdleModule;
        errorBanner: ErrorBannerModule;
        timeline: TimelineModule;
        vaccinationStatus: VaccinationStatusModule;
        waitlist: WaitlistModule;
    };
}
