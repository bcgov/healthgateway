/// <reference types="cypress" />
// ***********************************************************
// This example plugins/index.js can be used to load plugins
//
// You can change the location of this file or turn off loading
// the plugins file with the 'pluginsFile' configuration option.
//
// You can read more here:
// https://on.cypress.io/plugins-guide
// ***********************************************************
const { rmdir } = require("fs");
const { isFileExist } = require("cy-verify-downloads");

// This function is called when a project is opened or re-opened (e.g. due to
// the project's config changing)

/**
 * @type {Cypress.PluginConfig}
 */
module.exports = (on, config) => {
    // `on` is used to hook into various events Cypress emits
    // `config` is the resolved Cypress config
    on("task", {
        isFileExist,

        deleteFolder(folderName) {
            return new Promise((resolve, reject) => {
                rmdir(
                    folderName,
                    { maxRetries: 10, recursive: true },
                    (err) => {
                        if (err) {
                            return reject(err);
                        }
                        resolve(null);
                    }
                );
            });
        },
    });
};
