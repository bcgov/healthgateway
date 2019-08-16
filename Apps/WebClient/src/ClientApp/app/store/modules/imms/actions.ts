import { ActionTree, Commit } from "vuex";

import { IImmsService } from "@/services/interfaces";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { ImmsState } from "@/models/immsState";
import { RootState } from "@/models/rootState";

function handleError(commit: Commit, error: Error) {
  console.log("ERROR:" + error);
  commit("immsError");
}

export const actions: ActionTree<ImmsState, RootState> = {
  getitems({ commit }): any {
    console.log("imms getitems...");
    const immsService: IImmsService = container.get<IImmsService>(
      SERVICE_IDENTIFIER.ImmsService
    );
    commit("itemsRequest");
    return new Promise((resolve, reject) => {
      immsService
        .getItems()
        .then(data => {
          commit("immsItemsLoaded", data);
          resolve();
        })
        .catch(error => {
          handleError(commit, error);
          reject(error);
        })
        .finally(() => {
          console.log("Finished imms getitems");
        });
    });
  }
};
