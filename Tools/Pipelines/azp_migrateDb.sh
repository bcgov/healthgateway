#!/usr/bin/env bash
pushd _Database/drop
migrations=$(Release.Artifacts._Database.BuildNumber).efbundle
chmod u+x $migrations
./$migrations --connection "Host=$(db.host);Database=$(db.name);User ID=$(db.user);Password=$(db.password);Command Timeout=300;sslmode=VerifyFull"
popd