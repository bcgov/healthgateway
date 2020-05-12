import { ActionTree, Commit } from "vuex";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { RootState, LaboratoryState } from "@/models/storeState";
import { ILaboratoryService } from "@/services/interfaces";
import { LaboratoryReport } from "@/models/laboratory";
import RequestResult from "@/models/requestResult";
import { ResultType } from "@/constants/resulttype";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("laboratoryError");
}

const laboratoryService: ILaboratoryService = container.get<ILaboratoryService>(
  SERVICE_IDENTIFIER.LaboratoryService
);

export const actions: ActionTree<LaboratoryState, RootState> = {
  getReports(
    { commit, getters },
    { hdid }
  ): Promise<RequestResult<LaboratoryReport[]>> {
    return new Promise((resolve, reject) => {
      var laboratoryReports: LaboratoryReport[] = getters.getStoredLaboratoryReports();
      if (laboratoryReports.length > 0) {
        console.log("Laboratory found stored, not quering!");
        resolve({
          pageIndex: 0,
          pageSize: 0,
          resourcePayload: laboratoryReports,
          resultMessage: "From storage",
          resultStatus: ResultType.Success,
          totalResultCount: laboratoryReports.length
        });
      } else {
        console.log("Retrieving Laboratory Reports");
        laboratoryService
          .getLaboratoryReports(hdid)
          .then(laboratoryReports => {
            commit("setLaboratoryReports", laboratoryReports);
            resolve(laboratoryReports);
          })
          .catch(error => {
            handleError(commit, error);
            reject(error);
          });
      }
    });
  }
};
